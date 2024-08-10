module SharpStore.Web.ValidationErrors

open Validus

let empty: ValidationErrors = ValidationErrors.collect []

let errorsFor key (e: ValidationErrors) =
    e |> ValidationErrors.toMap |> Map.tryFind key |> Option.defaultValue []

let hasError key (e: ValidationErrors) =
    e |> errorsFor key |> List.isEmpty |> not
