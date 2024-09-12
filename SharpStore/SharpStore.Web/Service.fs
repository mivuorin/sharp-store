module SharpStore.Web.Service

open System

open SharpStore.Web.Domain
open Validus

let generateOrderId: GenerateOrderId = Guid.NewGuid

let validateOrderLine (orderLineValidator: OrderLineValidator) (getProductId: GetProductId) : ValidateOrderLine =
    fun form ->
        task {
            let validationResult = orderLineValidator form

            // todo Composing validation and task computational expressions should be possible with F#+

            match validationResult with
            | Ok validatedOrderLine ->
                // todo Suboptimal ux: checking if product code exist happens after validating product line. eg. missing product code appears valid when quantity is invalid
                let! productId = getProductId validatedOrderLine.ProductCode

                match productId with
                | None -> return ValidationErrors.create "ProductCode" [ "Product does not exist." ] |> Error
                | Some productId ->
                    return
                        { ProductId = productId
                          ProductCode = validatedOrderLine.ProductCode
                          Quantity = validatedOrderLine.Quantity }
                        |> Ok
            | Error errors -> return Error errors
        }
