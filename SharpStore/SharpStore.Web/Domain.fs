namespace SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core

open Validus

// Giraffe bindFormAsync<T> function has bug which leaves list uninitialized (null) when
// there is no form values to bind. To avoid this use array instead of list.
[<CLIMutable>]
type OrderLineForm =
    { ProductCode: string
      Quantity: string }


[<CLIMutable>]
type OrderForm = { OrderLines: OrderLineForm array }

type ValidatedOrderLine =
    { ProductCode: ProductCode
      Quantity: decimal }

type ValidatedOrder = { OrderLines: ValidatedOrderLine list }

type OrderLine =
    { ProductId: Guid
      ProductCode: ProductCode
      Quantity: decimal }

type Order =
    { Id: Guid
      OrderLines: OrderLine list } // todo could use F#+ NonEmptyList

type OrderCreated = { Id: Guid }

type OrderLineValidator = OrderLineForm -> ValidationResult<ValidatedOrderLine>
type OrderValidator = OrderForm -> ValidationResult<ValidatedOrder>

// todo Have own type for order id instead of guid.
type OrderId = unit -> Guid

// services
type ValidateOrderLine = OrderLineForm -> Task<ValidationResult<OrderLine>>

// Database
type InsertOrder = Order -> Task
type GetProductId = ProductCode -> Task<Guid option>

module Order =
    type CreateOrder = unit -> Order

    let create (orderId: OrderId) : CreateOrder =
        fun () ->
            { Order.Id = orderId ()
              OrderLines = [] }
