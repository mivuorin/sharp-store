# sharp-store

F# Web App experiment with DDD

## Develop

1. Start SQL Server docker container
    ```shell
    docker-compose up 
    ```
2. Solution folder:
    ```shell
   cd SharpStore
    ```
3. Install dotnet tools
    ```shell
   dotnet tool restore
    ```
4. Build
    ```shell
    dotnet build
    ```
5. Run
   ```shell
   dotnet run --project SharpStore.Web
   ```
6. Or run with watch
   ```shell
   dotnet watch run --project SharpStore.Web
   ```
7. Or use configured launch settings and run from IDE

## Code formatting (Fantomas)

Project uses Fantomas for code formatting and styles.

To format all files:

```shell
dotnet fantomas .
```

## SQL Server 2022 & Migrations

Project uses [Sql Server 2022](https://hub.docker.com/r/microsoft/mssql-server) database docker image.

[FluentMigration](https://github.com/fluentmigrator/fluentmigrator) database migration tool cannot create database, so
it needs to be created manually.
After starting container, run following sql script with any sql tool.

```tsql
create database SharpStore
```

FluentMigration CLI is installed as local dotnet tool. Because of bug or user error, full connection string needs to be
provided when running migration commands.

Run all migrations:

```shell
dotnet fm migrate -p SqlServer2016 -c "Server=localhost;Database=SharpStore;User Id=sa;Password=u4IDQGp119AtWV2SvH38184ufzSG4es7;TrustServerCertificate=true;" -a .\SharpStore.Web\bin\Debug\net8.0\SharpStore.Web.dll
```

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

## Dependency Injection vs. Composition in Giraffe and Asp.NET

It's not possible to fully compose Asp.NET application from pure functions because some dependencies have state and life
cycle.

For example database connection needs to be closed and disposed properly at the end of request, which couples it to http
requests. This could be fixed by adding more logic into database layer, with some connection factory which would manage
connection state, but this would add extra responsibilities and increase database layer complexity.

Proper solution is to use existing Asp.NET IOC for managing instance life cycle and dependency injection.

Sadly Giraffe has no abstraction for registering dependencies or dependency injection and relies on service locator
antipattern.

```fsharp
let submitOrder = ctx.GetService<SubmitOrder>()
```

