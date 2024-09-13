module SharpStore.Web.Index

open Giraffe
open Giraffe.ViewEngine

let get: HttpHandler =
    [ div [ _class "jumbotron" ] [
          div [ _class "container" ] [
              h1 [ _class "fw-bold display-4" ] [
                  str "Welcome to Our Widget and Gadget Store"
              ]
              p [ _class "lead" ] [ str "Discover the latest and greatest gadgets and widgets to enhance your life." ]
              hr [ _class "my-4" ]
              p [] [
                  strong [] [ str "Bulk up your savings and your supplies! "]
                  str "Our store offers unbeatable deals on a wide range of widgets and gadgets, perfect for businesses, hobbyists, and anyone who needs a lot."
              ]
              p [ _class "lead"  ] [
                  str "Ready to start your order? Click "
                  a [
                      _class "text-uppercase"
                      _href "/order"
                  ] [ str "here" ]
                  str " to begin."
              ]
          ]
      ] ]
    |> Layout.main
    |> htmlView
