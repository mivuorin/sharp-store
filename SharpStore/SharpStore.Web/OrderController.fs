module SharpStore.Web.OrderController

open System
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

let submittedView (orderId: Guid) =
    [ h1 [] [ str "Order Submitted" ]
      p [] [ str "Thank you for your order." ]
      p [] [
          str "Your order id is: "
          str (orderId.ToString())
      ] ]
    |> Layout.main

let get: HttpHandler = init |> view |> htmlView

let postHandler: HttpHandler = fun next ctx -> next ctx

let post: HttpHandler =
    fun next ctx ->
        task {
            let submitOrder = ctx.GetService<SubmitOrder>()

            // todo use TryBind instead of Bind for better error handling?
            let! form = ctx.BindFormAsync<OrderForm>()
            let! result = submitOrder form

            match result with
            | Ok created ->
                let short = ShortGuid.fromGuid created.id
                let url = $"/thanks/{short}"
                return! redirectTo false url next ctx

            | Error e ->
                let model =
                    { Form = form
                      Errors = e }

                return! htmlView (view model) next ctx
        }

let complete (orderId: Guid) : HttpHandler = orderId |> submittedView |> htmlView
