module SharpStore.Web.OrderLinesStep

open Giraffe
open Giraffe.Htmx
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

open SharpStore.Web.Domain
open SharpStore.Web.Session

type OrderLineModel =
    { Form: OrderLineForm
      Errors: Map<string, string list> }

let orderLineRow (index: int) (orderLine: OrderLine) =
    tr [] [
        th [ _scope "row" ] [ index + 1 |> string |> str ]
        td [] [ orderLine.ProductCode |> ProductCode.value |> str ]
        td [] [ orderLine.Quantity |> string |> str ]
        td [] [
            input [
                _class "btn btn-link btn-xs"
                _type "button"
                _hxDelete $"/order/line/delete/{index}"
                _hxSwap "none"
                _value "Remove"
            ]
        ]
    ]

let orderLinesTable (lines: OrderLine list) =
    let rows =
        if lines = List.Empty then
            [ tr [] [
                  td [
                      _class "text-center"
                      _colspan "4"
                  ] [ str "Your order is empty. Please add at least one item before continuing." ]
              ] ]
        else
            lines |> List.mapi orderLineRow

    table [ _class "table table-striped align-middle" ] [
        thead [] [
            tr [] [
                th [
                    _scope "col"
                    _class "col-1"
                ] [ str "#" ]
                th [
                    _scope "col"
                    _class "col-4"
                ] [ str "Product" ]
                th [
                    _scope "col"
                    _class "col-5"
                ] [ str "Quantity" ]
                th [
                    _scope "col"
                    _class "col-2"
                ] []
            ]
        ]
        tbody [ _class "table-group-divider" ] rows
    ]

let nextButton =
    button [
        _id "next-button"
        _class "btn btn-primary"
        _hxGet "/order/contact"
        _hxTarget "#order-step"
        _hxSwap "outerHTML"
        _hxSwapOob "true"
    // _hxPushUrl "true" todo use browser history for steps
    ] [ str "Next" ]

let nextButtonDisabled =
    button [
        _id "next-button"
        _class "btn btn-primary"
        _hxSwapOob "true"
        _disabled
    ] [ str "Next" ]

let orderLinesStepState (lines: OrderLine list) =
    div [] [
        orderLinesTable lines
        if lines = List.Empty then
            nextButtonDisabled
        else
            nextButton
    ]

let addOrderLineForm (model: OrderLineForm) errors =
    form [
        _id "add-order-line-form"
        _hxPost "/order/line"
        _hxSwap "outerHTML"
    ] [
        div [ _class "row mb-3" ] [
            div [ _class "col-5" ] [
                Input.text "add-product-code" (nameof model.ProductCode) model.ProductCode errors
                |> Input.placeHolder "Product Code"
                |> Input.view
            ]
            div [ _class "col-5" ] [
                Input.text "add-product-quantity" (nameof model.Quantity) model.Quantity errors
                |> Input.placeHolder "Quantity"
                |> Input.view
            ]
            div [ _class "col-2" ] [
                button [
                    _class "btn btn-secondary btn-xs"
                    _type "submit"
                ] [ str "Add Product" ]
            ]
        ]
    ]

let orderLinesStep (model: OrderLineForm) errors =
    div [ _id "order-step" ] [ // todo html helper for order-step container
        h1 [] [ str "Products" ]
        p [] [ str "Add widgets and gadgets to order." ]

        div [
            _id "order-lines"
            _class "mb-3"
            _hxGet "/order/line"
            _hxTrigger "load, OrderLinesChanged from:body"
            _hxSwap "innerHTML"
        ] []

        addOrderLineForm model errors

        div [ _class "col d-flex flex-row-reverse" ] [ button [ _id "next-button" ] [ str "Next" ] ]
    ]

let get: HttpHandler =
    fun next ctx ->
        // Navigating back from previous step is htmx request
        let content =
            if ctx.Request.IsHtmx then
                orderLinesStep OrderLineForm.init ValidationErrors.empty
            else
                [ orderLinesStep OrderLineForm.init ValidationErrors.empty ] |> Layout.main

        htmlView content next ctx

let getOrderLinesState: HttpHandler =
    fun next ctx ->
        let session = ctx.GetService<ISession>()
        let lines = session.TryFind "OrderLines" |> Option.defaultValue List.Empty

        let content = orderLinesStepState lines
        htmlView content next ctx

let post: HttpHandler =
    fun next ctx ->
        task {
            let session = ctx.GetService<ISession>()
            let validateOrderLine = ctx.GetService<ValidateOrderLine>()

            let! form = ctx.BindFormAsync<OrderLineForm>()
            let! validated = validateOrderLine form

            match validated with
            | Ok orderLine ->
                let lines = session.TryFind "OrderLines" |> Option.defaultValue List.Empty

                lines @ [ orderLine ] |> session.Add "OrderLines"

                let content = addOrderLineForm OrderLineForm.init ValidationErrors.empty
                return! (withHxTrigger "OrderLinesChanged" >=> htmlView content) next ctx

            | Error errors ->
                let content = addOrderLineForm form errors
                return! htmlView content next ctx
        }

let delete index : HttpHandler =
    fun next ctx ->
        let session = ctx.GetService<ISession>()

        let lines = session.Find "OrderLines"

        let lines = lines |> List.removeAt index
        session.Add "OrderLines" lines

        (withHxTrigger "OrderLinesChanged" >=> Successful.NO_CONTENT) next ctx
