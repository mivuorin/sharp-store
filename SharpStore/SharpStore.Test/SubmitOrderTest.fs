module SharpStore.Test.SubmitOrderTest

open System
open System.Threading.Tasks
open Xunit
open FsUnit

open SharpStore.Web
open SharpStore.Web.Domain

[<Fact>]
let Submit_order_when_order_is_invalid () =
    let insertOrder: InsertOrder = fun _ -> Task.CompletedTask
    let getProductId: GetProductId = fun _ -> Task.FromResult None

    let submitOrder =
        Service.submitOrder Validation.orderValidator getProductId Guid.NewGuid insertOrder

    task {
        let form: OrderForm = { OrderLines = [||] }

        let! actual = submitOrder form
        actual |> Option.isNone |> should equal true
    }

[<Fact>]
let Submit_order () =
    let expectedOrderId = Guid.NewGuid()
    let orderId: OrderId = fun () -> expectedOrderId

    let expected = Some { OrderCreated.Id = expectedOrderId }

    let form: OrderForm =
        { OrderLines =
            [| { ProductCode = "W0001"
                 Quantity = "1" } |] }

    // todo How to check that correct product is passed to insertOrder? With mock is would be possible
    let insertOrder: InsertOrder = fun _ -> Task.CompletedTask

    let getProductId: GetProductId = fun _ -> Task.FromResult None

    task {
        let! actual = Service.submitOrder Validation.orderValidator getProductId orderId insertOrder form
        actual |> should equal expected
    }
