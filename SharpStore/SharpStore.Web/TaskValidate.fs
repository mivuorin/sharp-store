module SharpStore.Web.TaskValidate

open System.Threading.Tasks
open Validus

// Validius does not support asynchronous validation.
// Monad which combines task and ValidationResult monads.
// todo use F#+ generic monad to implement async validation

type TaskValidator<'a, 'b> = string -> 'a -> Task<ValidationResult<'b>>

type TaskValidationResultBuilder() =
    member _.Bind(x: Task<ValidationResult<'a>>, f: 'a -> Task<ValidationResult<'b>>) =
        task {
            match! x with
            | ValidationResult.Ok value -> return! f value
            | ValidationResult.Error value -> return ValidationResult.Error value
        }

    member _.Return(x) = task { return ValidationResult.Ok x }

let asTask a = task { return a }

let taskValidate = TaskValidationResultBuilder()
