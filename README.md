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
