module SharpStore.Test.ProductCodeValidationTest

open SharpStore.Web
open Xunit
open FsUnit

let validator = Domain.productCodeValidator "test"

[<Fact>]
let Product_code_is_required () =
    let result = validator ""
    result |> Result.isError |> should equal true

[<Fact>]
let Product_code_is_too_long () =
    let result = validator "asd"
    result |> Result.isError |> should equal true

[<Fact>]
let Valid_product_code () =
    let result = validator "as"
    result |> Result.isOk |> should equal true
