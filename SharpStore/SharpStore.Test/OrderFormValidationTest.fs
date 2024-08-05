module SharpStore.Test.OrderFormValidationTest

open SharpStore.Web
open Xunit
open FsUnit

open SharpStore.Web.Domain

[<Fact>]
let Product_code_validation_errors_are_indexed () =
    let form: OrderForm =
        { ProductCodes =
            [| ""
               "1234" |] }

    let result = Validation.orderValidator form
    result |> Result.isError |> should equal true

    match result with
    | Ok _ -> Assert.Fail("Expected Error result")
    | Error errors ->
        errors
        |> Map.keys
        |> should equivalent [
            "ProductCodes0"
            "ProductCodes1"
        ]

[<Fact>]
let Correct_order () =
    let form: OrderForm =
        { ProductCodes =
            [| "01"
               "02" |] }

    let result = Validation.orderValidator form

    let expected: ValidatedOrder =
        { ProductCodes =
            [ "01"
              "02" ] }

    match result with
    | Ok actual -> actual |> should equal expected
    | Error _ -> Assert.Fail("Expected Ok result")
