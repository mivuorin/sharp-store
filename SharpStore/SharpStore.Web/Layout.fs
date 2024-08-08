module SharpStore.Web.Layout

open Giraffe.ViewEngine

// Custom bootstrap attributes
let _data_bs_toggle = attr "data-bs-toggle"
let _data_bs_target = attr "data-bs-target"

let scripts =
    [ script [
          _src "https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"
          _integrity "sha384-YvpcrYf0tY3lHB60NNkmXc5s9fDVZLESaAA55NDzOxhy9GkcIdslK1eN7N6jIeHz"
          _crossorigin "anonymous"
      ] []
      script [
          _src "https://unpkg.com/htmx.org@2.0.1"
          _integrity "sha384-QWGpdj554B4ETpJJC9z+ZHJcA/i59TyjxEPXiiUgN2WmTyV5OEZWCD6gQhgkdpB/"
          _crossorigin "anonymous"
      ] [] ]


let main (content: XmlNode list) : XmlNode =
    html [] [
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
        body
            []
            ([ nav [ _class "navbar navbar-expand-lg" ] [
                   div [ _class "container-fluid" ] [
                       a [
                           _class "navbar-brand"
                           _href "/"
                       ] [
                           span [] [
                               // todo create logo in vector format
                               img [
                                   _style "height: 2rem"
                                   _src "/shovel_64.png"
                               ]
                               str "SharpStore"
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
                                       _class "nav-link active"
                                       _href "/about"
                                   ] [ str "About" ]
                               ]
                           ]
                           a [
                               _class "btn btn-primary"
                               _href "/order"
                           ] [ str "Order" ]
                       ]
                   ]
               ]

               div [ _class "container" ] content

               ]
             @ scripts)
    ]
