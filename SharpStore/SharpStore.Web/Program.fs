module SharpStore.Web.Program

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Giraffe
open Giraffe.EndpointRouting

open SharpStore.Web.Domain

let submitOrder: SubmitOrder = submitOrder orderValidator orderId

let endpoints =
    [ GET [
          route "/" Index.view
          route "/order" OrderController.get
          route "/thanks" OrderController.complete
      ]
      POST [ route "/order" (OrderController.post submitOrder) ] ]

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    builder.Services.AddGiraffe() |> ignore

    let app = builder.Build()

    app.UseRouting().UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
    |> ignore

    app.Run()

    0
