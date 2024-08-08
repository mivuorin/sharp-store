module SharpStore.Web.OrderController

open System

open Giraffe
open Giraffe.Htmx.Handlers
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Giraffe.EndpointRouting

open Microsoft.FSharp.Collections

open SharpStore.Web.Domain
open SharpStore.Web.Session
open Validus

type OrderLineModel =
    { Form: OrderLineForm
      Errors: Map<string, string list> }

type OrderViewModel =
    { Order: Order
      OrderLineModel: OrderLineModel }

// todo Move controller logic into testable actions
type BeginOrderAction = unit -> OrderViewModel
type DeleteOrderAction = int -> OrderViewModel

let initOrderLineForm: OrderLineForm =
    { ProductCode = ""
      Quantity = "" }

let initOrderLineModel: OrderLineModel =
    { Form = initOrderLineForm
      Errors = Map.empty }

let init order : OrderViewModel =
    { Order = order
      OrderLineModel = initOrderLineModel }

let errorFeedback errors : XmlNode list =
    match errors with
    | [] -> List.empty
    | errors -> errors |> List.map (fun e -> div [ _class "invalid-feedback" ] [ str e ])

let classes errors =
    seq {
        yield "form-control"

        if errors |> List.isEmpty |> not then
            yield "is-invalid"
    }
    |> String.concat " "

let textField id name placeHolder value errors =
    let productCodeErrors = errors |> Map.tryFind name |> Option.defaultValue []

    (input [
        _id id
        _class (classes productCodeErrors)
        _type "text"
        _placeholder placeHolder
        _name name
        _value value
     ]
     :: (errorFeedback productCodeErrors))

let orderLine index (orderLine: OrderLine) =
    div [ _class "row mb-3" ] [
        div [ _class "col-5" ] [
            span [ _class "form-control-plaintext" ] [ orderLine.ProductCode |> ProductCode.value |> str ]
        ]
        div [ _class "col-3" ] [
            span [ _class "form-control-plaintext" ] [
                // todo culture & formatting?
                orderLine.Quantity |> string |> str
            ]
        ]
        div [ _class "col" ] [
            input [
                _class "btn btn-link"
                _type "button"
                _hxDelete $"/order/line/delete/{index}"
                _hxSwap "none"
                _value "Remove"
            ]
        ]
    ]

let orderLines (lines: OrderLine list) =
    div
        [ _id "order-lines"
          _hxGet "/order/line"
          _hxTrigger "OrderLinesChanged from:body"
          _hxSwap "outerHTML" ] // todo maybe swap only innerHtml
        (List.mapi orderLine lines)

let addOrderLineForm (orderLineForm: OrderLineForm) (errors: Map<string, string list>) =
    form [
        _id "add-order-line-form"
        _hxPost "/order/line"
        _hxSwap "outerHTML"
    ] [
        div [ _class "row mb-3" ] [
            div
                [ _class "col-5" ]
                (textField "add-product-code" "ProductCode" "Product Code" orderLineForm.ProductCode errors)

            div
                [ _class "col-3" ]
                (textField "add-product-quantity" "Quantity" "Quantity" orderLineForm.Quantity errors)

            div [ _class "col" ] [
                button [
                    _class "btn btn-secondary"
                    _type "submit"
                ] [ str "Add Product" ]
            ]
        ]
    ]

let orderFormView (model: OrderViewModel) =
    // todo disable submit if no order lines
    let submit =
        div [ _class "col" ] [
            button [
                _class "btn btn-primary"
                _hxPost "/order"
            ] [ str "Submit" ]
        ]

    div
        [ _id "order-form" ]
        ([ orderLines model.Order.OrderLines ]
         @ [ addOrderLineForm model.OrderLineModel.Form model.OrderLineModel.Errors ]
         @ [ submit ])

let view (model: OrderViewModel) =
    [ h1 [] [ str "Order Form" ]
      p [] [ str "Fill out following order form." ]
      orderFormView model ]
    |> Layout.main

let submittedView (orderId: Guid) =
    [ h1 [] [ str "Order Submitted" ]
      p [] [ str "Thank you for your order." ]
      p [] [
          str "Your order id is: "
          str (orderId.ToString())
      ] ]
    |> Layout.main

let initOrderView (session: ISession) (createOrder: Order.CreateOrder) : BeginOrderAction =
    fun () ->
        let order = session.TryFind "order"

        let order =
            match order with
            | None ->
                let order = createOrder ()
                session.Add "order" order
                order
            | Some order -> order

        init order

let get: HttpHandler =
    fun next ctx ->
        let init = ctx.GetService<BeginOrderAction>()
        let model = init ()
        let content = model |> view
        htmlView content next ctx

let getOrderLines: HttpHandler =
    fun next ctx ->
        let session = ctx.GetService<ISession>()
        let order: Order = session.Find "order"

        let content = orderLines order.OrderLines
        htmlView content next ctx

let addOrderLine: HttpHandler =
    fun next ctx ->
        task {
            let session = ctx.GetService<ISession>()
            let validateOrderLine = ctx.GetService<ValidateOrderLine>()

            let! form = ctx.BindFormAsync<OrderLineForm>()

            let! validated = validateOrderLine form

            // todo Move OrderForm mapping logic into validateOrderLine function
            match validated with
            | Ok orderLine ->
                let order: Order = session.Find "order"

                let order = { order with OrderLines = orderLine :: order.OrderLines }
                session.Add "order" order

                let content = addOrderLineForm initOrderLineForm Map.empty
                return! (withHxTrigger "OrderLinesChanged" >=> htmlView content) next ctx

            | Error errors ->
                let content = addOrderLineForm form (errors |> ValidationErrors.toMap)
                return! htmlView content next ctx
        }


let deleteOrderLine index : HttpHandler =
    fun next ctx ->
        let session = ctx.GetService<ISession>()

        let order: Order = session.Find "order"
        let order = { order with OrderLines = order.OrderLines |> List.removeAt index }
        session.Add "order" order

        (withHxTrigger "OrderLinesChanged" >=> Successful.NO_CONTENT) next ctx

let submitOrder: HttpHandler =
    fun next ctx ->
        task {
            let session = ctx.GetService<ISession>()
            let insertOrder = ctx.GetService<InsertOrder>()

            // todo refactor into testable function (Action / Service)
            let order = session.Find "order"
            do! insertOrder order
            session.Remove "order"

            let short = ShortGuid.fromGuid order.Id
            let url = $"/thanks/{short}"
            return! withHxRedirect url next ctx
        }

let complete (orderId: Guid) : HttpHandler = orderId |> submittedView |> htmlView

let orderEndpoints =
    [ GET [
          route "/order" get
          route "/order/line" getOrderLines
          routef "/thanks/%O" complete
      ]
      POST [
          route "/order" submitOrder
          route "/order/line" addOrderLine
      ]
      DELETE [ routef "/order/line/delete/%i" deleteOrderLine ] ]
