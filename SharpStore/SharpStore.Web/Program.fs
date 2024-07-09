module SharpStore.Web.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe

let routes = choose [ route "/" >=> htmlView Index.view ]

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseGiraffe(routes)
    app.Run()

    0
