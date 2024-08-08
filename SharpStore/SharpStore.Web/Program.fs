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

let endpoints = [ GET [ route "/" Index.view ] ] @ OrderController.orderEndpoints

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
        .AddTransient<ValidateOrderLine>(
            Func<IServiceProvider, ValidateOrderLine>(fun (prov: IServiceProvider) ->
                Service.validateOrderLine (prov.GetService<OrderLineValidator>()) (prov.GetService<GetProductId>()))
        )
        .AddTransient<OrderLineValidator>(
            Func<IServiceProvider, OrderLineValidator>(fun (prov: IServiceProvider) -> Validation.orderLineValidator)
        )
        .AddTransient<Order.CreateOrder>(
            Func<IServiceProvider, Order.CreateOrder>(fun prov -> Order.create Service.orderId)
        )
        .AddTransient<OrderController.BeginOrderAction>(
            Func<IServiceProvider, OrderController.BeginOrderAction>(fun prov ->
                OrderController.initOrderView (prov.GetService<ISession>()) (prov.GetService<Order.CreateOrder>()))
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
