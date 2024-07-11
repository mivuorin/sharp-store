module SharpStore.Web.Index

open Giraffe.ViewEngine

let view =
    [ h1 [] [ str "Welcome to Giraffe" ]
      p [ _id "welcome" ] [
          str "This is a dev experiment for "
          a [ _href "https://github.com/giraffe-fsharp"; _target "blank" ] [ str "Giraffe" ]
      ]
      rawText "<p>Rawdogging text is possible but should be <b>avoided!</b></p>"
      encodedText "<p>Always encode user input!</p>" ]
    |> Layout.main
