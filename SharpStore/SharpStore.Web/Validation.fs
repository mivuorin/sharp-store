module SharpStore.Web.Validation

open Validus

open SharpStore.Web.Domain

let productCodeValidator =
    // todo Provide custom validation messages instead of using library ones.
    ValidatorGroup(Check.String.notEmpty).And(Check.String.lessThanLen 3).Build()

let productValidator: ProductValidator =
    fun form ->
        validate {
            let! productCode = productCodeValidator (nameof form.ProductCode) form.ProductCode
            return { ProductCode = productCode }
        }
        |> Result.mapError ValidationErrors.toMap

let orderValidator: OrderValidator =
    fun form ->
        let productCodesField = nameof form.ProductCodes

        validate {
            // todo This should be coupled to OrderForm type some way?
            let! productCodes =
                form.ProductCodes
                |> Array.mapi (fun i -> productCodeValidator (productCodesField + string i))
                |> Array.toList
                |> ValidationResult.sequence

            return { ProductCodes = productCodes }
        }
        |> Result.mapError ValidationErrors.toMap
