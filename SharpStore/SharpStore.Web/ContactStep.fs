module SharpStore.Web.ContactStep

open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

open SharpStore.Web.Domain
open SharpStore.Web.Session

let contactStep (contactForm: ContactForm) errors : XmlNode =
    div [ _id "order-step" ] [
        h1 [] [ str "Contact information" ]
        p [] [ str "Please fill out your contact information." ]
        form [
            _id "contact-form"
            _hxPost "/order/contact"
            _hxSwap "outerHTML"
            _hxTarget "#order-step"
        ] [
            div [ _class "col mb-3" ] [
                Input.text "contact-name" (nameof contactForm.Name) contactForm.Name errors
                |> Input.label "Full Name"
                |> Input.view
            ]
            div [ _class "row" ] [
                div [ _class "col-6 mb-3" ] [
                    Input.text "contact-email" (nameof contactForm.Email) contactForm.Email errors
                    |> Input.label "Email"
                    |> Input.view
                ]
                div [ _class "col-6 mb-3" ] [
                    Input.tel "contact-phone" (nameof contactForm.Phone) contactForm.Phone errors
                    |> Input.label "Phone"
                    |> Input.view
                ]
            ]
            div [ _class "d-grid gap-2 d-sm-flex justify-content-sm-between" ] [
                button [
                    _class "btn btn-secondary"
                    _type "button"
                    _hxGet "/order"
                    _hxTarget "#order-step"
                ] [ str "Back" ]

                button [
                    _class "btn btn-primary"
                    _type "submit"
                ] [ str "Next" ]
            ]
        ]
    ]

let get: HttpHandler =
    fun next ctx ->
        let session = ctx.GetService<ISession>()

        let contact = session.TryFind "Contact"

        let form =
            contact |> Option.map ContactForm.from |> Option.defaultValue ContactForm.empty

        htmlView (contactStep form ValidationErrors.empty) next ctx

let post: HttpHandler =
    fun next ctx ->
        task {
            let! form = ctx.BindFormAsync<ContactForm>()

            let validated = Validation.contactValidator form

            match validated with
            | Ok contact ->
                let session = ctx.GetService<ISession>()
                session.Add "Contact" contact

                // todo Browser history for order steps
                return! (ReviewStep.get next ctx)
            | Error errors -> return! htmlView (contactStep form errors) next ctx
        }
