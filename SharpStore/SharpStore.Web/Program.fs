module SharpStore.Web.Program

open System
open System.Text.Json.Serialization
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http

open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open Giraffe
open Giraffe.EndpointRouting

open SharpStore.Web.Domain
open SharpStore.Web.Session

// TODO Figure out how get url for route so that links wont break every time route is changed.
let orderEndpoints =
    [ GET [
          route "/order" OrderLinesStep.get
          route "/order/line" OrderLinesStep.getOrderLinesState
          route "/order/contact" ContactStep.get
          route "/order/review" ReviewStep.get
          routef "/thanks/%O" SubmitStep.get
      ]
      POST [
          route "/order" SubmitStep.post
          route "/order/line" OrderLinesStep.post
          route "/order/contact" ContactStep.post
      ]
      DELETE [ routef "/order/line/delete/%i" OrderLinesStep.delete ] ]

let endpoints = [ GET [ route "/" Index.get ] ] @ orderEndpoints

[<EntryPoint>]
let main args =

    let builder = WebApplication.CreateBuilder(args)

    let connectionString = builder.Configuration.GetConnectionString("default")

    // todo Better way to register dependencies? Maybe https://github.com/Zaid-Ajaj/Giraffe.GoodRead
    builder.Services
        .AddTransient<Database.Connection>(
            Func<IServiceProvider, Database.Connection>(fun (prov: IServiceProvider) ->
                Database.connection connectionString)
        )
        .AddTransient<InsertOrder>(
            Func<IServiceProvider, InsertOrder>(fun (prov: IServiceProvider) ->
                prov.GetService<Database.Connection>() |> Database.insertOrder)
        )
        .AddTransient<GetProductId>(
            Func<IServiceProvider, GetProductId>(fun (prov: IServiceProvider) ->
                prov.GetService<Database.Connection>() |> Database.getProductId)
        )
        .AddTransient<OrderLineValidator>(
            Func<IServiceProvider, OrderLineValidator>(fun (prov: IServiceProvider) ->
                Validation.orderLineValidator (prov.GetService<GetProductId>()))
        )
        .AddTransient<GenerateOrderId>(
            Func<IServiceProvider, GenerateOrderId>(fun (prov: IServiceProvider) -> OrderIdGenerator.gen)
        )
        .AddTransient<ISession, Session>()
        .AddHttpContextAccessor()
        .AddDistributedMemoryCache()
        .AddSession(fun (options: SessionOptions) ->
            options.Cookie.HttpOnly <- true
            options.Cookie.IsEssential <- true)
        .AddGiraffe()
        .AddSingleton<Json.ISerializer>(
            SystemTextJson.Serializer(JsonFSharpOptions.Default().ToJsonSerializerOptions())
        )
    |> ignore

    let app = builder.Build()

    app
        .UseStaticFiles()
        .UseSession()
        .UseRouting()
        .UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
    |> ignore

    app.Run()

    0
