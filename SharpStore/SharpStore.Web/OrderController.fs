module SharpStore.Web.OrderController

open System
open Giraffe
open Giraffe.ViewEngine
open Giraffe.EndpointRouting

open Microsoft.FSharp.Collections
open Microsoft.FSharp.Core

open SharpStore.Web.Domain

let (|IndexedAction|_|) (prefix: String) (action: string option) =
    if action.IsNone then
        None
    else if action.Value.StartsWith(prefix) then
        action.Value.Substring(prefix.Length) |> int |> Some
    else
        None

let indexedAction prefix (index: int) = prefix + string index

type Model =
    { Form: OrderForm
      Errors: Map<string, string list> }

let init: Model =
    { Form = { ProductCodes = [ "" ] }
      Errors = Map.empty }

// todo refactor into own Validation module and replace Map<string, string list> with proper type.
let errorsForField id index (errors: Map<string, string list>) : string list =
    // todo Refactor and use nameof
    let fieldId = id + string index
    Map.tryFind fieldId errors |> Option.defaultValue []

let errorMessage id message =
    div [ _class "invalid-feedback" ] [ message ]

let classString classes = String.concat " " classes

let productCodeField (errors: Map<string, string list>) index value =
    let errorFeedback =
        match (errorsForField "ProductCodes" index errors) with
        | [] -> List.empty
        | errors -> errors |> List.map (fun e -> div [ _class "invalid-feedback" ] [ str e ])

    let fieldErrors = errorsForField "ProductCodes" index errors
    let hasErrors = fieldErrors |> List.isEmpty |> not

    let classes =
        seq {
            yield "form-control"

            if hasErrors then
                yield "is-invalid"
        }

    div [ _class "row mb-3" ] [
        div
            [ _class "col-8" ]
            ([ input [
                   _class (classString classes)
                   _type "text"
                   _name "ProductCodes"
                   _placeholder "Product code"
                   _value value
               ] ]
             @ errorFeedback)
        div [ _class "col" ] [
            button [
                _class "btn btn-link"
                _type "submit"
                _name "action"
                _value (indexedAction "delete" index)
            ] [ str "Remove" ]
        ]
    ]

let view (model: Model) =
    let productCodes: XmlNode list =
        model.Form.ProductCodes |> List.mapi (productCodeField model.Errors)

    [ h1 [] [ str "Order Form" ]
      p [] [ str "Fill out following order form." ]
      form
          [ _method "POST" ]
          (productCodes
           @ [ div [ _class "col mb-3" ] [
                   button [
                       _class "btn btn-secondary"
                       _type "submit"
                       _name "action"
                       _value "add"
                   ] [ str "Add Product" ]
               ] ]

           @ [ div [ _class "row" ] [
                   div [ _class "col" ] [
                       button [
                           _class "btn btn-primary"
                           _type "submit"
                           _name "action"
                           _value "submit"
                       ] [ str "Submit" ]
                   ]

               ] ]) ]

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

let post: HttpHandler =
    fun next ctx ->
        // todo Use form binding for action?
        let action = ctx.GetFormValue "action"

        match action with
        | IndexedAction "delete" index ->
            task {
                let! form = ctx.BindFormAsync<OrderForm>()

                let productCodes =
                    if form.ProductCodes.Length < 2 then
                        [ "" ]
                    else
                        List.removeAt index form.ProductCodes

                let form = { form with ProductCodes = productCodes }
                let validated = orderValidator form

                let model =
                    match validated with
                    | Ok _ ->
                        { Form = form
                          Errors = Map.empty }
                    | Error errors ->
                        { Form = form
                          Errors = errors }

                return! htmlView (view model) next ctx
            }
        | Some "add" ->
            task {
                let! form = ctx.BindFormAsync<OrderForm>()

                let validated = orderValidator form

                // Allow adding new rows only when rows are valid
                let model =
                    match validated with
                    | Ok _ ->
                        { Form = { form with ProductCodes = form.ProductCodes @ [ "" ] }
                          Errors = Map.empty }

                    | Error errors ->
                        { Form = form
                          Errors = errors }

                return! htmlView (view model) next ctx
            }

        | Some "submit" ->
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

        | _ -> RequestErrors.BAD_REQUEST "Invalid form action" next ctx

let complete (orderId: Guid) : HttpHandler = orderId |> submittedView |> htmlView

let orderEndpoints =
    [ GET [
          route "/order" get
          routef "/thanks/%O" complete
      ]
      POST [ route "/order" post ] ]
