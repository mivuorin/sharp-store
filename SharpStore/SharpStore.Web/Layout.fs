module SharpStore.Web.Layout

open Giraffe.ViewEngine

// Custom bootstrap attributes
let _data_bs_toggle = attr "data-bs-toggle"
let _data_bs_target = attr "data-bs-target"

let private navbar =
    nav [ _class "navbar navbar-expand-sm mb-4" ] [
        // todo fix other links alignment onto 2nd line
        div [ _class "container-fluid align-items-end" ] [
            a [
                _class "navbar-brand"
                _href "/"
            ] [
                div [ _class "d-flex flex-row column-gap-2 align-items-center" ] [
                    img [
                        _style "height: 3rem"
                        _src "/shovel_64.png"
                    ]
                    div [ _class "d-flex flex-column" ] [
                        div [ _class "fs-2" ] [ str "SharpStore" ]
                        div [ _class "fs-6" ] [ str "Best source for Widgets & Gadgets" ]
                    ]
                ]
            ]

            button [
                _class "navbar-toggler"
                _type "button"
                _data_bs_toggle "collapse"
                _data_bs_target "#navbar-collapse-content"
            ] [ span [ _class "navbar-toggler-icon" ] [] ]

            div [
                _id "navbar-collapse-content"
                _class "collapse navbar-collapse"
            ] [
                ul [ _class "navbar-nav me-auto mb-auto" ] [
                    li [ _class "nav-item" ] [
                        a [
                            _class "nav-link"
                            _href "/about"
                        ] [ str "About" ]
                    ]
                    li [ _class "nav-item" ] [
                        a [
                            _class "nav-link"
                            _href "/catalog"
                        ] [ str "Catalog" ]
                    ]
                    li [ _class "nav-item" ] [
                        a [
                            _class "nav-link"
                            _href "/contact"
                        ] [ str "Contact us" ]
                    ]
                ]
                a [
                    _class "btn btn-primary"
                    _href "/order"
                ] [ str "Order Here!" ]
            ]
        ]
    ]

// todo wrapping single xmlnode into list is a bit fishy composition
let main (content: XmlNode list) : XmlNode =
    html [ _data "bs-theme" "dark" ] [
        head [] [
            meta [ _charset "utf-8" ]
            meta [
                _name "viewport"
                _content "width=device-width, initial-scale=1"
            ]
            title [] [ str "SharpStore" ]
            link [
                _href "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
                _rel "stylesheet"
                _integrity "sha384-QWTKZyjpPEjISv5WaRU9OFeRpok6YctnYmDr5pNlyT2bRjXh0JMhjY6hW+ALEwIH"
                _crossorigin "anonymous"
            ]
        ]
        body [] [
            div [ _class "container-sm" ] [
                navbar
                div [ _class "container" ] content
            ]
            script [
                _src "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"
                _integrity "sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz"
                _crossorigin "anonymous"
            ] []

            script [
                _src "https://unpkg.com/htmx.org@2.0.1"
                _integrity "sha384-QWGpdj554B4ETpJJC9z+ZHJcA/i59TyjxEPXiiUgN2WmTyV5OEZWCD6gQhgkdpB/"
                _crossorigin "anonymous"
            ] []
        ]

    ]
