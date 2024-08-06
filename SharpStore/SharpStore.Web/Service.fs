module SharpStore.Web.Service

    open System
    open System.Threading.Tasks

    open SharpStore.Web.Domain
    open Validus

    let orderId: OrderId = Guid.NewGuid

    // todo write unit tests!
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
                              Quantity = validatedOrderLine.Quantity }
                            |> Ok
                | Error errors -> return Error errors
            }


    let toOrder (orderId:OrderId) (getProductId:GetProductId) (validatedOrder:ValidatedOrder) : Task<Order> =
        task {
            // todo ugly and should check that orderLines is not empty, and also validate it!
            let! orderLines =
                validatedOrder.OrderLines
                |> List.map (fun (orderLine:ValidatedOrderLine) ->
                    task {
                        let! productId = getProductId orderLine.ProductCode
                        return productId |> Option.map (fun id -> { ProductId = id; Quantity = orderLine.Quantity } )
                    } )
                |> Task.WhenAll

            let orderLines =
                orderLines
                |> Array.choose id
                |> Array.toList

            return { Order.Id = orderId()
                     OrderLines = orderLines }
        }

    let submitOrder
        (validator: OrderValidator)
        (getProductId: GetProductId)
        (orderId: OrderId)
        (insertOrder: InsertOrder)
        : SubmitOrder =
        fun form ->
            task {
                let validated = validator form
                match validated with
                | Ok validatedOrder ->
                    // todo DI violation passing references to child
                    let! order = toOrder orderId getProductId validatedOrder
                    do! insertOrder order
                    return Some { OrderCreated.Id = order.Id }
                | ValidationResult.Error _ ->
                    return None
            }
