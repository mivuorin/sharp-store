module SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core
open Validus

[<CLIMutable>]
type OrderForm = { ProductCode: string }

type ValidatedOrder = { ProductCode: string }

type OrderCreated = { id: Guid }

type OrderValidator = OrderForm -> Result<ValidatedOrder, Map<string, string list>>

type OrderCreatedResult = Result<OrderCreated, Map<string, string list>>

// todo Have own type for order id instead of guid.
type OrderId = unit -> Guid
let orderId: OrderId = Guid.NewGuid

type SubmitOrder = OrderForm -> Task<OrderCreatedResult>


// todo Move to validator module

let productCodeValidator =
    ValidatorGroup(Check.String.notEmpty).And(Check.String.lessThanLen 3).Build()

let orderValidator: OrderValidator =
    fun form ->
        validate {
            let! productCode = productCodeValidator "ProductCode" form.ProductCode
            let order: ValidatedOrder = { ProductCode = productCode }
            return order
        }
        |> Result.mapError ValidationErrors.toMap

type InsertOrder = Guid -> ValidatedOrder -> Task<int>

let submitOrder: OrderValidator -> OrderId -> InsertOrder -> SubmitOrder =
    fun validator orderId insertOrder ->
        fun form ->
            task {
                let validated = validator form

                match validated with
                | Result.Error error -> return Error error
                | Result.Ok order ->
                    let id = orderId ()
                    // todo check inserted?
                    let! _ = insertOrder id order

                    return Ok { id = orderId () }
            }
