module SharpStore.Web.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe

// todo Case insensitive routing (routeCi)
let routes =
    choose [
        GET
        >=> choose [
            route "/" >=> htmlView Index.view
            route "/order" >=> Order.get
            route "/thanks" >=> Order.complete
        ]
        POST >=> choose [ route "/order" >=> Order.post ]
    ]

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseGiraffe(routes)
    app.Run()

    0
