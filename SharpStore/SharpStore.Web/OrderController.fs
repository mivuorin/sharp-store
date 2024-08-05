module SharpStore.Web.OrderController

open System

open Giraffe
open Giraffe.Htmx.Handlers
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx
open Giraffe.EndpointRouting

open Microsoft.FSharp.Collections
open SharpStore.Web.Domain
open SharpStore.Web.Validation

type Model =
    { OrderForm: OrderForm
      ProductForm: ProductForm
      Errors: Map<string, string list> }

let init: Model =
    { OrderForm = { ProductCodes = Array.empty }
      ProductForm = { ProductCode = "" }
      Errors = Map.empty }

// todo refactor into own Validation module and replace Map<string, string list> with proper type.
let errorsForFieldIndex id index (errors: Map<string, string list>) : string list =
    // todo Refactor and use nameof
    let fieldId = id + string index
    Map.tryFind fieldId errors |> Option.defaultValue []

let errorMessage id message =
    div [ _class "invalid-feedback" ] [ message ]

let classString classes = String.concat " " classes

let orderForm (model: Model) =
    let addProduct =
        let errors = model.Errors |> Map.tryFind "ProductCode" |> Option.defaultValue []

        let errorFeedback errors : XmlNode list =
            match errors with
            | [] -> List.empty
            | errors -> errors |> List.map (fun e -> div [ _class "invalid-feedback" ] [ str e ])

        let classes =
            seq {
                yield "form-control"

                if errors |> List.isEmpty |> not then
                    yield "is-invalid"
            }

        div [ _class "row mb-3" ] [
            div
                [ _class "col-8" ]
                (input [
                    _id "add-product-code"
                    _class (classString classes)
                    _type "text"
                    _placeholder "Product code"
                    _name "ProductCode"
                    _value model.ProductForm.ProductCode
                 ]
                 :: (errorFeedback errors))
            div [ _class "col" ] [
                button [
                    _class "btn btn-secondary"
                    _type "submit"
                    _hxPost "/order/product"
                    _hxTrigger "click"
                    _hxTarget "#order-form"
                    _hxSwap "outerHTML"
                ] [ str "Add Product" ]
            ]
        ]

    let productCode index value =
        div [ _class "row mb-3" ] [
            div [ _class "col-8" ] [
                input [
                    _class "form-control-plaintext"
                    _type "text"
                    _name "ProductCodes"
                    _readonly
                    _value value
                ]
            ]
            div [ _class "col" ] [
                input [
                    _class "btn btn-link"
                    _type "button"
                    _hxPost $"/order/product/delete/{index}"
                    _hxTarget "#order-form"
                    _hxSwap "outerHTML"
                    _value "Remove"
                ]
            ]
        ]

    let productCodes =
        model.OrderForm.ProductCodes |> Array.mapi productCode |> Array.toList

    form
        [ _id "order-form" ]
        (productCodes
         @ [ addProduct ]
         @ [ div [ _class "col" ] [
                 button [
                     _class "btn btn-primary"
                     _hxPost "/order"
                 ] [ str "Submit" ]
             ] ])

let view (model: Model) =
    [ h1 [] [ str "Order Form" ]
      p [] [ str "Fill out following order form." ]
      orderForm model ]
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
        task {
            let submitOrder = ctx.GetService<SubmitOrder>()
            // todo form should have at leas one order line to be able to submit

            let! form = ctx.BindFormAsync<OrderForm>()
            let! result = submitOrder form

            // Send client side redirect or just response with result?
            match result with
            | Ok created ->
                let short = ShortGuid.fromGuid created.id
                let url = $"/thanks/{short}"

                return! withHxRedirect url next ctx

            | Error _ ->
                return!
                    HttpStatusCodeHandlers.ServerErrors.internalError
                        (text "Something went wrong when processing order!")
                        next
                        ctx
        }

let complete (orderId: Guid) : HttpHandler = orderId |> submittedView |> htmlView

let addProduct: HttpHandler =
    fun next ctx ->
        task {
            let! form = ctx.BindFormAsync<OrderForm>()
            let! productForm = ctx.BindFormAsync<ProductForm>()

            let validated = productValidator productForm

            let model =
                match validated with
                | Ok product ->
                    { OrderForm = { form with ProductCodes = Array.append form.ProductCodes [| product.ProductCode |] }
                      ProductForm = { ProductCode = "" }
                      Errors = Map.empty }

                | Error errors ->
                    { OrderForm = form
                      ProductForm = productForm
                      Errors = errors }

            let content = orderForm model
            return! htmlView content next ctx
        }

let deleteProduct index : HttpHandler =
    fun next ctx ->
        task {
            let! form = ctx.BindFormAsync<OrderForm>()
            let! productForm = ctx.BindFormAsync<ProductForm>()

            let model =
                { OrderForm = { form with ProductCodes = form.ProductCodes |> Array.removeAt index }
                  ProductForm = productForm
                  Errors = Map.empty }

            let content = orderForm model
            return! htmlView content next ctx
        }

let orderEndpoints =
    [ GET [
          route "/order" get
          routef "/thanks/%O" complete
      ]
      POST [
          route "/order" post
          route "/order/product" addProduct
          // todo Change to use http delete request if order state is stored in session and not in http form.
          routef "/order/product/delete/%i" deleteProduct
      ] ]
