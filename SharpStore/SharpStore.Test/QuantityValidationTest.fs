module SharpStore.Test.QuantityValidationTest

open Xunit
open FsUnit

open SharpStore.Web

let validator = Validation.quantityValidator "test"

[<Fact>]
let Quantity_is_required () =
    let result = validator ""
    result |> Result.isError |> should equal true

[<Fact>]
let Quantity_must_be_over_zero () =
    let result = validator "0"
    result |> Result.isError |> should equal true

[<Fact>]
let Quantity_must_be_less_than_50 () =
    let result = validator "50"
    result |> Result.isError |> should equal true

[<Fact>]
let Valid_quantity () =
    let result = validator "1,5"
    result |> Result.isOk |> should equal true
