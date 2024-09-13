module SharpStore.Web.ReviewStep

open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

open SharpStore.Web.Domain
open SharpStore.Web.Session

let orderLinesTable (lines: OrderLine list) =
    let row index (orderLine: OrderLine) =
        tr [] [
            th [ _scope "row" ] [ index + 1 |> string |> str ]
            td [] [ orderLine.ProductCode |> ProductCode.value |> str ]
            td [] [ orderLine.Quantity |> string |> str ]
        ]

    table [
        _id "order-lines"
        _class "table table-striped align-middle"
    ] [
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
                    _class "col"
                ] [ str "Quantity" ]
            ]
        ]
        tbody [ _class "table-group-divider" ] (lines |> List.mapi row)
    ]

let reviewStep (orderLines: OrderLine list) (contact: Contact) =
    div [ _id "order-step" ] [
        h1 [] [ str "Review your order" ]
        p [] [ str "Please check that all your order information is correct before submitting order." ]

        h2 [] [ str "Products" ]
        orderLinesTable orderLines

        h2 [] [ str "Contact information" ]
        // todo helper text here.
        div [ _id "contact-information" ] [
            div [ _class "row" ] [
                label [ _class "col-sm-2 col-form-label" ] [ str "Name" ]
                div [ _class "col-sm-10" ] [ span [ _class "form-control-plaintext" ] [ str contact.Name ] ]
            ]
            div [ _class "row" ] [
                label [ _class "col-sm-2 col-form-label" ] [ str "Email" ]
                div [ _class "col-sm-10" ] [ span [ _class "form-control-plaintext" ] [ str contact.Email ] ]
            ]
            div [ _class "row" ] [
                label [ _class "col-sm-2 col-form-label" ] [ str "Phone" ]
                div [ _class "col-sm-10" ] [
                    span [ _class "form-control-plaintext" ] [
                        contact.Phone |> Option.defaultValue "No phone number" |> str
                    ]
                ]
            ]
        ]

        div [ _class "d-grid gap-2 d-sm-flex justify-content-sm-between" ] [
            button [
                _class "btn btn-secondary"
                _type "button"
                _hxGet "/order/contact"
                _hxTarget "#order-step"
            ] [ str "Back" ]

            button [
                _class "btn btn-primary"
                _type "submit"
                _hxPost "/order"
                _hxTarget "#order-step"
                _hxSwap "outerHTML"
            ] [ str "Submit Order" ]
        ]
    ]

let get: HttpHandler =
    fun next ctx ->
        let session = ctx.GetService<ISession>()

        let orderLines = session.Find<OrderLine list> "OrderLines"
        let contact = session.Find<Contact> "Contact"

        htmlView (reviewStep orderLines contact) next ctx
