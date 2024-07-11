module SharpStore.Web.Order
// todo rename Order module to OrderController

open System
open Giraffe
open Giraffe.ViewEngine
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core
open Validus

// todo Move to domain
type OrderType = { ProductCode: String }

[<CLIMutable>]
type OrderForm = { ProductCode: string }

type Model =
    { Form: OrderForm
      Errors: Map<string, string list> }

let init: Model =
    { Form = { ProductCode = "" }
      Errors = Map.empty }

let view (model: Model) =
    html [] [
        head [] [ title [] [ str "SharpStore" ] ]
        body [] [
            h1 [] [ str "Order Form" ]
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

                input [ _id "submit"; _type "submit"; _name "submit" ]
            ]
        ]
    ]

let orderSubmittedView =
    html [] [
        head [] [ title [] [ str "SharpStore" ] ]
        body [] [ h1 [] [ str "Order Submitted" ]; p [] [ str "Thank you for your order." ] ]
    ]

let productCodeValidator =
    ValidatorGroup(Check.String.notEmpty).And(Check.String.lessThanLen 3).Build()

let validateForm (form: OrderForm) =
    validate {
        let! productCode = productCodeValidator "ProductCode" form.ProductCode
        let order: OrderType = { ProductCode = productCode }
        return order
    }

let submitOrder form =
    match validateForm form with
    | Ok _ ->
        // todo do something with valid order
        redirectTo false "/thanks"
    | Error e ->
        { Form = form
          Errors = ValidationErrors.toMap e }
        |> view
        |> htmlView

let get: HttpHandler = init |> view |> htmlView

let post: HttpHandler =
    // todo Culture specific form parsing
    tryBindForm<OrderForm> RequestErrors.BAD_REQUEST None submitOrder

let complete: HttpHandler = htmlView orderSubmittedView
