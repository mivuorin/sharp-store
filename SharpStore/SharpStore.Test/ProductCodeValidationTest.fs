module SharpStore.Test.ProductCodeValidationTest

open Xunit
open FsUnit

open SharpStore.Web.Validation

let widgetValidator = widgetCodeValidator "test"
let gadgetValidator = gadgetCodeValidator "test"

[<Fact>]
let Invalid_widget_code_empty () =
    widgetValidator "" |> Result.isError |> should equal true

[<Fact>]
let Invalid_widget_code_too_short () =
    widgetValidator "W123" |> Result.isError |> should equal true

[<Fact>]
let Invalid_widget_code_too_long () =
    widgetValidator "W12345" |> Result.isError |> should equal true

[<Fact>]
let Invalid_widget_code_not_starting_with_w () =
    widgetValidator "a1234" |> Result.isError |> should equal true

[<Fact>]
let Valid_widget_code () =
    widgetValidator "w1234" |> Result.isOk |> should equal true

[<Fact>]
let Valid_widget_code_capital_letter () =
    widgetValidator "W1234" |> Result.isOk |> should equal true

[<Fact>]
let Invalid_gadget_code_empty () =
    gadgetValidator "" |> Result.isError |> should equal true

[<Fact>]
let Invalid_gadget_code_too_short () =
    gadgetValidator "G12" |> Result.isError |> should equal true

[<Fact>]
let Invalid_gadget_code_too_long () =
    gadgetValidator "G1234" |> Result.isError |> should equal true

[<Fact>]
let Invalid_gadget_code_not_starting_with_g () =
    gadgetValidator "g1234" |> Result.isError |> should equal true

[<Fact>]
let Valid_gadget_code () =
    gadgetValidator "g123" |> Result.isOk |> should equal true

[<Fact>]
let Valid_gadget_code_capital_letter () =
    gadgetValidator "g123" |> Result.isOk |> should equal true
