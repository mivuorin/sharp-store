module SharpStore.Test.OrderLineValidationTest

open System
open Xunit
open FsUnit

open System.Threading.Tasks

open SharpStore.Web.Domain
open SharpStore.Web.Validation

let expectedId = Guid.NewGuid()
let getProductId: GetProductId = fun _ -> Task.FromResult(Some expectedId)

[<Fact>]
let Order_line_gadget () =
    let form: OrderLineForm =
        { ProductCode = "G123"
          Quantity = "1" }

    let expected: ValidatedOrderLine =
        { ProductCode = GadgetCode.GadgetCode "G123" |> Gadget
          Quantity = 1m }

    let result = orderLineValidator form

    match result with
    | Ok orderLine -> orderLine |> should equal expected
    | Error _ -> Assert.Fail("Expected Ok result")


[<Fact>]
let Order_line_invalid_gadget () =
    let form: OrderLineForm =
        { ProductCode = "G0001"
          Quantity = "1" }
    // todo this test duplicates product code validation which is not really necessary
    let result = orderLineValidator form
    result |> Result.isError |> should equal true


[<Fact>]
let Order_line_widget () =
    let form: OrderLineForm =
        { ProductCode = "W1234"
          Quantity = "1" }

    let expected: ValidatedOrderLine =
        { ProductCode = WidgetCode.WidgetCode "W1234" |> Widget
          Quantity = 1m }

    let result = orderLineValidator form

    match result with
    | Ok orderLine -> orderLine |> should equal expected
    | Error _ -> Assert.Fail("Expected OK result")
