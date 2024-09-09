module SharpStore.Web.Validation

open System

open System.Net.Mail
open Validus
open Validus.Operators

open SharpStore.Web.Domain

let widgetCodeValidator: Validator<string, WidgetCode> =
    (Check.String.pattern "^([wW]+)(\d{4})$") *|* WidgetCode.WidgetCode

let gadgetCodeValidator: Validator<string, GadgetCode> =
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

let private constant message : ValidationMessage = fun _ -> message

let nameValidator: Validator<string, string> =
    Check.WithMessage.String.notEmpty (constant "Please enter your full name")
    >=> Check.WithMessage.String.lessThanLen 51 (constant "Name is too long")

let emailValidator: Validator<string, string> =
    let rule value =
        let success, _ = MailAddress.TryCreate value
        success

    Validator.create (constant "Please enter valid email") rule

// todo Validator composition hack just for converting types to Option
let emptyToOption: Validator<String, string option> =
    let foo =
        function
        | "" -> None
        | value -> Some value

    Validator.create (constant "") (fun _ -> true) *|* (foo)

let phoneValidator: Validator<string, string option> =
    emptyToOption
    >=> Check.optional (Check.WithMessage.String.betweenLen 1 16 (constant "Please enter valid phone number"))

let contactValidator: ContactValidator =
    fun form ->
        validate {
            let! name = nameValidator (nameof form.Name) form.Name
            and! email = emailValidator (nameof form.Email) form.Email
            and! phone = phoneValidator (nameof form.Phone) form.Phone

            return
                { Name = name
                  Email = email
                  Phone = phone }
        }
