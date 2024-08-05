module SharpStore.Web.Validation

open System

open Validus
open Validus.Operators

open SharpStore.Web.Domain

let productCodeValidator =
    // todo Provide custom validation messages instead of using library ones.
    ValidatorGroup(Check.String.notEmpty).And(Check.String.lessThanLen 3).Build()

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

let orderLineValidatorR (form: OrderLineForm) =
    validate {
        let! productCode = productCodeValidator (nameof form.ProductCode) form.ProductCode
        // todo add test for validating quantity!
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

            and! products =
                form.OrderLines
                |> Array.map orderLineValidatorR
                |> Array.toList
                |> ValidationResult.sequence


            return { ProductCodes = products }
        }
        |> Result.mapError ValidationErrors.toMap
