module SharpStore.Web.ReviewStep

open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

open SharpStore.Web.Domain
open SharpStore.Web.Session

let reviewProducts (orderLines: OrderLine list) =
    let lines =
        orderLines
        |> List.map (fun line ->
            div [ _class "row" ] [
                div [ _class "col" ] [ line.ProductCode |> ProductCode.value |> str ]
                // todo Proper formatting of quantity!
                div [ _class "col" ] [ line.Quantity |> string |> str ]
            ])

    div [ _id "order-lines" ] lines

let reviewStep (orderLines: OrderLine list) (contact: Contact) =
    div [ _id "order-step" ] [
        h1 [] [ str "Review your order" ]
        p [] [ str "Please check that all your order information is correct before submitting order." ]

        h4 [] [ str "Products" ]
        reviewProducts orderLines

        h4 [] [ str "Contact information" ]
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
