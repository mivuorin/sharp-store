module SharpStore.Test.ContactValidationTest

open Xunit
open FsUnit

open SharpStore.Web.Domain
open SharpStore.Web.Validation
open SharpStore.Web.ValidationErrors

let validName = "Test User"
let validEmail = "some@email.com"

// todo Property based testing and test data generators could be useful.
let genString length = String.init length (fun _ -> "a")

[<Fact>]
let Name_required () =
    let form: ContactForm =
        { Email = ""
          Name = ""
          Phone = "" }

    let actual = contactValidator form
    let errors = Result.toError actual

    let errors = errorsFor (nameof form.Name) errors
    errors |> should equivalent [ "Please enter your full name" ]


[<Fact>]
let Name_too_long () =
    let form: ContactForm =
        { Email = ""
          Name = genString 51
          Phone = "" }

    let actual = contactValidator form
    let errors = Result.toError actual

    let errors = errorsFor (nameof form.Name) errors
    errors |> should equivalent [ "Name is too long" ]

[<Fact>]
let Email_empty () =
    let form: ContactForm =
        { Email = ""
          Name = ""
          Phone = "" }

    let actual = contactValidator form
    let errors = Result.toError actual

    let errors = errorsFor (nameof form.Email) errors
    errors |> should equivalent [ "Please enter valid email" ]

[<Fact>]
let Email_invalid () =
    let form: ContactForm =
        { Email = "foo"
          Name = ""
          Phone = "" }

    let actual = contactValidator form
    let errors = Result.toError actual

    let errors = errorsFor (nameof form.Email) errors
    errors |> should equivalent [ "Please enter valid email" ]

[<Fact>]
let Phone_too_long () =
    let form: ContactForm =
        { Email = ""
          Name = ""
          Phone = genString 17 }

    let actual = contactValidator form
    let errors = Result.toError actual

    let errors = errorsFor (nameof form.Phone) errors
    errors |> should equivalent [ "Please enter valid phone number" ]

[<Fact>]
let Phone_valid () =
    let form: ContactForm =
        { Email = ""
          Name = ""
          Phone = genString 16 }

    let actual = contactValidator form
    let errors = Result.toError actual

    let errors = errorsFor (nameof form.Phone) errors
    errors |> should equivalent List.empty

[<Fact>]
let Valid_contact () =
    let form: ContactForm =
        { Email = validEmail
          Name = validName
          Phone = "123-1234567" }

    let actual = contactValidator form |> Result.toValue

    let expected: Contact =
        { Email = validEmail
          Name = validName
          Phone = Some "123-1234567" }

    actual |> should equal expected

[<Fact>]
let Phone_empty () =
    let form: ContactForm =
        { Email = validEmail
          Name = validName
          Phone = "" }

    let contact = contactValidator form |> Result.toValue

    contact.Phone |> should equal None
