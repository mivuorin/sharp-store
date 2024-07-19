module SharpStore.Test.OrderFormValidationTest

open Xunit
open FsUnit

open SharpStore.Web.Domain

[<Fact>]
let Product_code_is_required () =
    let form: OrderForm = { ProductCode = "" }
    let result = orderValidator form
    result |> Result.isError |> should equal true
