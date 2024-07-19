module SharpStore.Test.SubmitOrderTest

open System
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

    let form: OrderForm = { ProductCode = "" }

    submitOrder validator Guid.NewGuid form |> should equal expected

[<Fact>]
let Submit_should_create_order_id () =
    let validator: OrderValidator = fun _ -> Ok { ProductCode = "ProductCode" }

    let orderId = Guid.NewGuid()

    let expected: OrderCreatedResult = { id = orderId } |> Ok

    let form: OrderForm = { ProductCode = "" }

    submitOrder validator (fun () -> orderId) form |> should equal expected
