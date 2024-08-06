module SharpStore.Web.Validation

open System

open Validus
open Validus.Operators

open SharpStore.Web.Domain

let widgetCodeValidator: Validator<string, WidgetCode.WidgetCode> =
    (Check.String.pattern "^([wW]+)(\d{4})$") *|* WidgetCode.WidgetCode

let gadgetCodeValidator: Validator<string, GadgetCode.GadgetCode> =
    (Check.String.pattern "^([gG]+)(\d{3})$") *|* GadgetCode.GadgetCode

let productCodeValidator: Validator<string, ProductCode> =
    (widgetCodeValidator *|* Widget <|> gadgetCodeValidator *|* Gadget)

let stringDecimalValidator: Validator<string, string> =
    let msg = sprintf "Please provide a valid %s"

    let rule (value: string) =
        let success, _ = Decimal.TryParse(value)
        success

    Validator.create msg rule

let decimalValidator: Validator<string, decimal> =
    stringDecimalValidator *|* Decimal.Parse

let quantityValidator: Validator<string, decimal> =
    decimalValidator
    >=> Check.Decimal.greaterThan 0m
    >=> ValidatorGroup(Check.Decimal.greaterThan 0m)
        .And(Check.Decimal.lessThan 50m)
        .Build()

let orderLineValidator: OrderLineValidator =
    fun form ->
        validate {
            let! productCode = productCodeValidator (nameof form.ProductCode) form.ProductCode
            and! quantity = quantityValidator (nameof form.Quantity) form.Quantity

            return
                { ProductCode = productCode
                  Quantity = quantity }
        }

let orderValidator: OrderValidator =
    fun form ->
        let orderLineCountValidator = ValidatorGroup(Check.Array.notEmpty).Build()

        validate {
            let! _ = orderLineCountValidator "OrderLines" form.OrderLines
            and! orderLines =
                form.OrderLines
                |> Array.map orderLineValidator
                |> Array.toList
                |> ValidationResult.sequence

            return { OrderLines = orderLines }
        }
