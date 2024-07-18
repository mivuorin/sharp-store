module SharpStore.Web.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe
open Giraffe.EndpointRouting

let endpoints =
    [ GET [
          route "/" Index.view
          route "/order" Order.get
          route "/thanks" Order.complete
      ]
      POST [ route "/order" Order.post ] ]

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseRouting().UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
    |> ignore

    app.Run()

    0
