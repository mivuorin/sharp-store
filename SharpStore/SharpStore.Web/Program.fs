open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe

let routes = choose [ route "/" >=> text "Hello Giraffe World!" ]

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseGiraffe(routes)
    app.Run()

    0 // Exit code
