namespace SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core

[<CLIMutable>]
type OrderLineForm =
    { ProductCode: string
      Quantity: string }

// Giraffe bindFormAsync<T> function has bug which leaves list uninitialized (null) when
// there is no form values to bind. To avoid this use array instead of list.
[<CLIMutable>]
type OrderForm = { OrderLines: OrderLineForm array }

type ValidatedOrderLine =
    { ProductCode: ProductCode
      Quantity: decimal }

type ValidatedOrder = { ProductCodes: ValidatedOrderLine list }

type OrderCreated = { id: Guid }

type OrderLineValidator = OrderLineForm -> Result<ValidatedOrderLine, Map<string, string list>>
type OrderValidator = OrderForm -> Result<ValidatedOrder, Map<string, string list>>

type OrderCreatedResult = Result<OrderCreated, Map<string, string list>>

// todo Have own type for order id instead of guid.
type OrderId = unit -> Guid

type SubmitOrder = OrderForm -> Task<OrderCreatedResult>

type InsertOrder = Guid -> ValidatedOrder -> Task

module Domain =
    let orderId: OrderId = Guid.NewGuid

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
