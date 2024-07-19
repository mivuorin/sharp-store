module SharpStore.Web.Domain

open System
open Validus

[<CLIMutable>]
type OrderForm = { ProductCode: string }

type ValidatedOrder = { ProductCode: string }

type OrderCreated = { id: Guid }

type OrderValidator = OrderForm -> Result<ValidatedOrder, Map<string, string list>>

type OrderCreatedResult = Result<OrderCreated, Map<string, string list>>

type OrderId = unit -> Guid
let orderId: OrderId = Guid.NewGuid

type SubmitOrder = OrderForm -> OrderCreatedResult

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

let submitOrder: OrderValidator -> OrderId -> SubmitOrder =
    fun validator orderId form -> validator form |> Result.map (fun _ -> { id = orderId () })
