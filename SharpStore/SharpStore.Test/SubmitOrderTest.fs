module SharpStore.Test.SubmitOrderTest

open System
open System.Threading.Tasks
open Validus
open Xunit
open FsUnit

open SharpStore.Web.Domain

[<Fact>]
let Submit_should_return_validation_errors_when_form_is_invalid () =
    let errors =
        ValidationErrors.create "ProductCode" [ "Error message" ]
        |> ValidationErrors.toMap

    let validator: OrderValidator = fun _ -> Error errors

    let expected: OrderCreatedResult = errors |> Error

    let form: OrderForm = { OrderLines = [||] }

    let insertOrder: InsertOrder = fun _ _ -> Task.CompletedTask

    task {
        let! actual = submitOrder validator Guid.NewGuid insertOrder form
        actual |> should equal expected
    }

[<Fact>]
let Submit_should_create_order_id () =
    let validator: OrderValidator = fun _ -> Ok { ProductCodes = [] }

    let expectedOrderId = Guid.NewGuid()
    let orderId: OrderId = fun () -> expectedOrderId

    let expected: OrderCreatedResult = { id = expectedOrderId } |> Ok

    let form: OrderForm = { OrderLines = [||] }

    let insertOrder: InsertOrder = fun _ _ -> Task.CompletedTask

    task {
        let! actual = submitOrder validator orderId insertOrder form
        actual |> should equal expected
    }
