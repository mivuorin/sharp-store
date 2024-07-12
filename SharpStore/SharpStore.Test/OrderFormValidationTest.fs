module SharpStore.Test.OrderFormValidationTest

open Xunit
open FsUnit

open SharpStore.Web

[<Fact>]
let Product_code_is_required () =
    let form: Order.OrderForm = { ProductCode = "" }
    let result = Order.validateForm form
    result |> Result.isError |> should equal true
