module SharpStore.Web.Input

open Giraffe.ViewEngine
open Validus

type Input =
    { id: string
      name: string
      value: string
      inputType: string
      errors: ValidationErrors
      placeholder: string option
      label: string option }

let private options id name value inputType errors : Input =
    { id = id
      name = name
      value = value
      inputType = inputType
      errors = errors
      placeholder = None
      label = None }

let text id name value errors : Input = options id name value "text" errors

let tel id name value errors : Input = options id name value "tel" errors

let placeHolder value options : Input =
    { options with placeholder = Some value }

let label value options : Input = { options with label = Some value }

let view (options: Input) : XmlNode =
    let cssClass =
        seq {
            yield "form-control"

            if ValidationErrors.hasError options.name options.errors then
                yield "is-invalid"
        }
        |> String.concat " "

    let attributes =
        seq {
            yield _id options.id
            yield _name options.name
            yield _type "text"
            yield _value options.value
            yield _class cssClass
            yield _type options.inputType

            if options.placeholder.IsSome then
                yield _placeholder options.placeholder.Value
        }
        |> Seq.toList

    let content =
        seq {
            if options.label.IsSome then
                yield
                    HtmlElements.label [
                        _class "form-label"
                        _for options.id
                    ] [ str options.label.Value ]

            yield input attributes

            yield!
                ValidationErrors.errorsFor options.name options.errors
                |> List.map (fun e -> div [ _class "invalid-feedback" ] [ str e ])

        }
        |> Seq.toList

    div [] content
