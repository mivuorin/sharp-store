# sharp-store

F# Web App experiment with DDD

## Develop

1. Install dotnet tools

       dotnet tools restore

2. Restore nuget packages

       dotnet nuget restore

3. Build

       dotnet build

4. Run

       dotnet run --project SharpStore.Web

5. Or run with watch

       dotnet watch run --project SharpStore.Web

6. Or use configured launch settings and run from IDE

## Code formatting (Fantomas)

Project uses Fantomas for code formatting and styles.

To format all files:

    dotnet fantomas .

## Forms & Validation

Project uses Giraffe.ViewEngine without standard Asp.NET Razor. Giraffe.ViewEngine is very lightweight, which means that
certain features like form state, error messages, anti-forgery tokens etc. need custom implementations.

Giraffe does not provide any validation library. It would be possible to use .NET Data Annotation Validation which would
work together with Razor view engine, but would require functional wrappers and does not really provide any benefit when
Razor helper tags are not used in rendering.

Project uses [Validius](https://github.com/pimbrouwers/Validus) which provides functional composable validators.

To add validation into HttpHandler pipeline with `validateModel`, Giraffe requires that validatable dto
implements `IModelValidation<T>` interface

This couples validation logic to dto which is unwanted.

## Unit testing Giraffe

Giraffe's main abstraction is `HttpHandler` function:

```fsharp
HttpFunc -> AspNetCore.Http.HttpContext -> HttpFuncResult
```

Which translates to following when wrapper types are removed:

```fsharp
(HttpContext -> Task<HttpContext option>) -> HttpContext -> Task<HttpContext option>
```

There is no abstraction layer over `HttpContext` like in Asp.NET MVC where Controllers would return `ActionResult` which
could be independently unit tested without coupling to `HttpContext`.

To unit test HttpHandler, full or mocked `HttpContext` is required, which makes testing very cumbersome because tests
and asserts need to work with concrete HttpRequest and HttpResponse objects.

With suggested best practice for using endpoint routing, composed Giraffe "app" extension point is removed which
complicates unit testing routes even more.

Example unit test for simple view:

```fsharp
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
        let! result = Index.view Giraffe.Core.earlyReturn context
        result |> Option.isSome |> should equal true

        let result = result.Value
        result.Response.StatusCode |> should equal 200

        context.Response.Body.Seek(0, SeekOrigin.Begin) |> ignore

        use reader = new StreamReader(result.Response.Body)
        let content = reader.ReadToEnd()

        content |> should contain "Welcome to Giraffe"
    }
```
