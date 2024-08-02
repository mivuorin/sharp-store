module SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core
open Validus

[<CLIMutable>]
type OrderForm = { ProductCodes: string list }

// todo Own type for product code.
type ValidatedOrder = { ProductCodes: string list }

type OrderCreated = { id: Guid }

type OrderValidator = OrderForm -> Result<ValidatedOrder, Map<string, string list>>

type OrderCreatedResult = Result<OrderCreated, Map<string, string list>>

// todo Have own type for order id instead of guid.
type OrderId = unit -> Guid
let orderId: OrderId = Guid.NewGuid

type SubmitOrder = OrderForm -> Task<OrderCreatedResult>


// todo Move to validator module

let productCodeValidator =
    // todo Provide custom validation messages instead of using library ones.
    ValidatorGroup(Check.String.notEmpty).And(Check.String.lessThanLen 3).Build()

let orderValidator: OrderValidator =
    fun form ->
        let productCodesField = nameof form.ProductCodes

        validate {
            // todo This should be coupled to OrderForm type some way?
            // todo When validating list of values, id should contain field index.
            let! productCodes =
                form.ProductCodes
                |> List.mapi (fun i -> productCodeValidator (productCodesField + string i))
                |> ValidationResult.sequence

            return { ProductCodes = productCodes }
        }
        |> Result.mapError ValidationErrors.toMap

type InsertOrder = Guid -> ValidatedOrder -> Task


let submitOrder: OrderValidator -> OrderId -> InsertOrder -> SubmitOrder =
    fun validator orderId insertOrder ->
        fun form ->
            task {
                let validated = validator form

                match validated with
                | Result.Error error -> return Error error
                | Result.Ok order ->
                    let id = orderId ()
                    do! insertOrder id order
                    return Ok { id = id }
            }
