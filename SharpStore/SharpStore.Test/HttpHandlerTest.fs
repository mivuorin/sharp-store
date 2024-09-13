module SharpStore.Test.HttpHandlerTest

open System.IO
open Microsoft.AspNetCore.Http
open SharpStore.Web
open Xunit
open FsUnit

[<Fact>]
let Index_view_handler_example_test () =

    let context = DefaultHttpContext()
    context.Request.Method <- "GET"
    context.Response.Body <- new MemoryStream()

    task {
        let! result = Index.get Giraffe.Core.earlyReturn context
        result |> Option.isSome |> should equal true

        let result = result.Value
        result.Response.StatusCode |> should equal 200

        context.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore

        use reader = new StreamReader(result.Response.Body)
        let content = reader.ReadToEnd()

        content |> should contain "Welcome to Giraffe"
    }
