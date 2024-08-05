module SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core

[<CLIMutable>]
type ProductForm = { ProductCode: string }

// Giraffe bindFormAsync<T> function has bug which leaves list uninitialized (null) when
// there is no form values to bind. To avoid this use array instead of list.
[<CLIMutable>]
type OrderForm = { ProductCodes: string array }

type ValidatedProduct = { ProductCode: string }

// todo Own type for product code.
type ValidatedOrder = { ProductCodes: string list }

type OrderCreated = { id: Guid }

type ProductValidator = ProductForm -> Result<ValidatedProduct, Map<string, string list>>
type OrderValidator = OrderForm -> Result<ValidatedOrder, Map<string, string list>>

type OrderCreatedResult = Result<OrderCreated, Map<string, string list>>

// todo Have own type for order id instead of guid.
type OrderId = unit -> Guid
let orderId: OrderId = Guid.NewGuid

type SubmitOrder = OrderForm -> Task<OrderCreatedResult>

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
