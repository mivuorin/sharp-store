module SharpStore.Web.Layout

open Giraffe.ViewEngine

let main content =
    html [] [
        head [] [ title [] [ str "SharpStore" ] ]
        body [] content
    ]
