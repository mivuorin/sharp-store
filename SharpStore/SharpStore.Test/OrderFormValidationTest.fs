module SharpStore.Test.OrderFormValidationTest

open SharpStore.Web
open Xunit
open FsUnit

open SharpStore.Web.Domain

[<Fact>]
let One_order_line_is_required () =
    let form: OrderForm = { OrderLines = Array.empty }

    let result = Validation.orderValidator form
    result |> Result.isError |> should equal true

[<Fact>]
let Valid_order () =
    let form: OrderForm =
        { OrderLines =
            [| { ProductCode = "01"
                 Quantity = "1" } |] }

    let result = Validation.orderValidator form

    let validatedProducts =
        [ { ProductCode = "01"
            Quantity = 1m } ]

    let expected: ValidatedOrder = { ProductCodes = validatedProducts }

    match result with
    | Ok actual -> actual |> should equal expected
    | Error _ -> Assert.Fail("Expected Ok result")
