module SharpStore.Web.OrderController

open System

open Giraffe
open Giraffe.Htmx.Handlers
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Giraffe.EndpointRouting

open Microsoft.FSharp.Collections
open SharpStore.Web.Domain
open Validus

type Model =
    { OrderForm: OrderForm
      OrderLineForm: OrderLineForm
      Errors: Map<string, string list> }

let initOrderLineForm: OrderLineForm =
    { ProductCode = ""
      Quantity = "" }

let init: Model =
    { OrderForm = { OrderLines = Array.empty }
      OrderLineForm = initOrderLineForm
      Errors = Map.empty }

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

let fieldName collectionName index name = $"{collectionName}[{index}].{name}"

let orderFormView (model: Model) =
    let addProduct =
        div [ _class "row mb-3" ] [
            div
                [ _class "col-5" ]
                (textField "add-product-code" "ProductCode" "Product Code" model.OrderLineForm.ProductCode model.Errors)

            div
                [ _class "col-3" ]
                (textField "add-product-quantity" "Quantity" "Quantity" model.OrderLineForm.Quantity model.Errors)

            div [ _class "col" ] [
                button [
                    _class "btn btn-secondary"
                    _type "submit"
                    _hxPost "/order/line"
                    _hxTrigger "click"
                    _hxTarget "#order-form"
                    _hxSwap "outerHTML"
                ] [ str "Add Product" ]
            ]
        ]

    let orderLineForm fieldName index (product: OrderLineForm) =
        div [ _class "row mb-3" ] [
            div [ _class "col-5" ] [
                input [
                    _class "form-control-plaintext"
                    _type "text"
                    _name (fieldName index (nameof product.ProductCode))
                    _readonly
                    _value product.ProductCode
                ]
            ]
            div [ _class "col-3" ] [
                input [
                    _class "form-control-plaintext"
                    _type "text"
                    _name (fieldName index (nameof product.Quantity))
                    _readonly
                    _value (string product.Quantity) // todo culture & formatting?
                ]
            ]
            div [ _class "col" ] [
                input [
                    _class "btn btn-link"
                    _type "button"
                    _hxPost $"/order/line/delete/{index}"
                    _hxTarget "#order-form"
                    _hxSwap "outerHTML"
                    _value "Remove"
                ]
            ]
        ]

    let productCodes =
        model.OrderForm.OrderLines
        |> Array.mapi (orderLineForm (fieldName (nameof model.OrderForm.OrderLines)))
        |> Array.toList

    form
        [ _id "order-form" ]
        (productCodes
         @ [ addProduct ]
         @ [ div [ _class "col" ] [
                 button [
                     _class "btn btn-primary"
                     _hxPost "/order"
                 ] [ str "Submit" ]
             ] ])

let view (model: Model) =
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

let get: HttpHandler = init |> view |> htmlView

let post: HttpHandler =
    fun next ctx ->
        task {
            let submitOrder = ctx.GetService<SubmitOrder>()

            let! form = ctx.BindFormAsync<OrderForm>()
            let! result = submitOrder form

            // Send client side redirect or just response with result?
            match result with
            | Some created ->
                let short = ShortGuid.fromGuid created.Id
                let url = $"/thanks/{short}"
                return! withHxRedirect url next ctx

            // todo Disable submit if there is problem in order
            | None ->
                return!
                    HttpStatusCodeHandlers.ServerErrors.internalError
                        (text "Something went wrong when processing order!")
                        next
                        ctx
        }

let addOrderLine: HttpHandler =
    fun next ctx ->
        task {
            let validateOrderLine = ctx.GetService<ValidateOrderLine>()

            let! orderForm = ctx.BindFormAsync<OrderForm>()
            let! orderLineForm = ctx.BindFormAsync<OrderLineForm>()

            let! validated = validateOrderLine orderLineForm

            // todo Move OrderForm mapping logic into validateOrderLine function
            let model =
                match validated with
                | Ok _ ->
                    { OrderForm = { orderForm with OrderLines = Array.append orderForm.OrderLines [| orderLineForm |] }
                      OrderLineForm = initOrderLineForm
                      Errors = Map.empty }

                | Error errors ->
                    { OrderForm = orderForm
                      OrderLineForm = orderLineForm
                      Errors = errors |> ValidationErrors.toMap } // todo better type for ValidationErrors?

            let content = orderFormView model
            return! htmlView content next ctx
        }

let deleteOrderLine index : HttpHandler =
    fun next ctx ->
        task {
            let! orderForm = ctx.BindFormAsync<OrderForm>()
            let! orderLineForm = ctx.BindFormAsync<OrderLineForm>()

            let model =
                { OrderForm = { orderForm with OrderLines = orderForm.OrderLines |> Array.removeAt index }
                  OrderLineForm = orderLineForm
                  Errors = Map.empty }

            let content = orderFormView model
            return! htmlView content next ctx
        }

let complete (orderId: Guid) : HttpHandler = orderId |> submittedView |> htmlView

let orderEndpoints =
    [ GET [
          route "/order" get
          routef "/thanks/%O" complete
      ]
      POST [
          route "/order" post
          route "/order/line" addOrderLine
          // todo Change to use http delete request if order state is stored in session and not in http form.
          routef "/order/line/delete/%i" deleteOrderLine
      ] ]
