module SharpStore.Test.OrderFormValidationTest

open Xunit
open FsUnit

open SharpStore.Web.Domain
open SharpStore.Web.Domain.WidgetCode
open SharpStore.Web.Domain.GadgetCode

open SharpStore.Web.Validation

[<Fact>]
let Order_line_gadget () =
    let form: OrderLineForm =
        { ProductCode = "G123"
          Quantity = "1" }

    let expected: ValidatedOrderLine =
        { ProductCode = GadgetCode "G123" |> Gadget
          Quantity = 1m }

    let result = orderLineValidator form

    match result with
    | Ok orderLine -> orderLine |> should equal expected
    | Error _ -> Assert.Fail("Expected Ok result")

[<Fact>]
let Order_line_widget () =
    let form: OrderLineForm =
        { ProductCode = "W1234"
          Quantity = "1" }

    let expected: ValidatedOrderLine =
        { ProductCode = WidgetCode "W1234" |> Widget
          Quantity = 1m }

    let result = orderLineValidator form

    match result with
    | Ok orderLine -> orderLine |> should equal expected
    | Error _ -> Assert.Fail("Expected OK result")

[<Fact>]
let One_order_line_is_required () =
    let form: OrderForm = { OrderLines = Array.empty }

    let result = orderValidator form
    result |> Result.isError |> should equal true

[<Fact>]
let Valid_order () =
    let form: OrderForm =
        { OrderLines =
            [| { ProductCode = "W1234"
                 Quantity = "1" }
               { ProductCode = "G123"
                 Quantity = "1" } |] }

    let result = orderValidator form

    let expected: ValidatedOrder =
        { ProductCodes =
            [ { ProductCode = WidgetCode "W1234" |> Widget
                Quantity = 1m }
              { ProductCode = GadgetCode "G123" |> Gadget
                Quantity = 1m } ] }

    match result with
    | Ok actual -> actual |> should equal expected
    | Error _ -> Assert.Fail("Expected Ok result")
