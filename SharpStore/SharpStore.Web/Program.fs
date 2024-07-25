module SharpStore.Web.Program

open System
open System.Data
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open Giraffe
open Giraffe.EndpointRouting
open SharpStore.Web.Domain

let endpoints =
    [ GET [
          route "/" Index.view
          route "/order" OrderController.get
          routef "/thanks/%O" OrderController.complete
      ]
      POST [ route "/order" OrderController.post ] ]


[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    let connectionString = builder.Configuration.GetConnectionString("default")

    // todo Better way to register dependencies? Maybe https://github.com/Zaid-Ajaj/Giraffe.GoodRead
    builder.Services
        .AddScoped<IDbConnection>(fun provider -> Database.connection connectionString)
        .AddTransient<InsertOrder>(
            Func<IServiceProvider, InsertOrder>(fun (prov: IServiceProvider) ->
                prov.GetService<IDbConnection>() |> Database.insertOrder)
        )
        .AddTransient<SubmitOrder>(
            Func<IServiceProvider, SubmitOrder>(fun provider ->
                provider.GetService<InsertOrder>() |> submitOrder orderValidator orderId)
        )
        .AddGiraffe()
    |> ignore

    let app = builder.Build()

    app.UseRouting().UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
    |> ignore

    app.Run()

    0
