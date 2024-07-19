module SharpStore.Web.OrderController

open Giraffe
open Giraffe.ViewEngine
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core

open SharpStore.Web.Domain

type Model =
    { Form: OrderForm
      Errors: Map<string, string list> }

let init: Model =
    { Form = { ProductCode = "" }
      Errors = Map.empty }

let view (model: Model) =
    [ h1 [] [ str "Order Form" ]
      p [] [ str "Fill out following order form." ]
      form [ _method "POST" ] [
          // Cannot use same field for validation, because error message contains field ID
          let productCodeField = "ProductCode"
          label [ _for productCodeField ] [ str "Product code" ]
          br []

          input [
              _id productCodeField
              _type "text"
              _name productCodeField
              _value model.Form.ProductCode
          ]

          br []
          // todo Cleanup errors into partial view
          match model.Errors.TryFind productCodeField with
          | None -> emptyText
          | Some errors ->
              let lines = errors |> List.map (fun e -> li [] [ str e ])
              ul [ _style "color:red" ] lines

          input [
              _id "submit"
              _type "submit"
              _name "submit"
          ]
      ] ]
    |> Layout.main

let submittedView =
    [ h1 [] [ str "Order Submitted" ]
      p [] [ str "Thank you for your order." ] ]
    |> Layout.main

let get: HttpHandler = init |> view |> htmlView

let post: SubmitOrder -> HttpHandler =
    fun submitOrder ->
        let submitHandler form =
            match submitOrder form with
            | Ok _ ->
                // todo show created order id
                redirectTo false "/thanks/"
            | Error e ->
                { Form = form
                  Errors = e }
                |> view
                |> htmlView

        // todo Culture specific form parsing
        tryBindForm<OrderForm> RequestErrors.BAD_REQUEST None submitHandler


let complete: HttpHandler = htmlView submittedView
