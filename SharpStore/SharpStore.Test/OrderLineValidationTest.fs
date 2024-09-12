module SharpStore.Test.OrderLineValidationTest

open System
open SharpStore.Web
open Xunit
open FsUnit

open SharpStore.Web.Domain
open SharpStore.Web.Validation

let gadgetId = Guid.NewGuid()
let widgetId = Guid.NewGuid()

let getProductId: GetProductId =
    fun code ->
        task {
            match code with
            | ProductCode.Gadget(GadgetCode "G001") -> return Some gadgetId
            | ProductCode.Widget(WidgetCode "W1000") -> return Some widgetId
            | _ -> return None
        }

[<Fact>]
let Valid_gadget () =
    let form: OrderLineForm =
        { ProductCode = "G001"
          Quantity = "1" }

    let expected: OrderLine =
        { ProductId = gadgetId
          ProductCode = GadgetCode.GadgetCode "G001" |> Gadget
          Quantity = 1m }

    task {
        let! result = orderLineValidator getProductId form

        result |> Result.toValue |> should equal expected
    }

[<Fact>]
let Valid_widget () =
    let form: OrderLineForm =
        { ProductCode = "W1000"
          Quantity = "1,5" }

    let expected: OrderLine =
        { ProductId = widgetId
          ProductCode = WidgetCode.WidgetCode "W1000" |> Widget
          Quantity = 1.5m }

    task {
        let! result = orderLineValidator getProductId form

        result |> Result.toValue |> should equal expected
    }

[<Fact>]
let Product_code_not_found () =
    let form: OrderLineForm =
        { ProductCode = "W2000"
          Quantity = "1" }

    let notFound: GetProductId = fun _ -> task { return Option.None }

    task {
        let! result = orderLineValidator notFound form

        result
        |> Result.toError
        |> ValidationErrors.errorsFor "ProductCode"
        |> should equal [ "Product does not exist." ]
    }
