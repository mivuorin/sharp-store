module SharpStore.Web.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open Giraffe
open Giraffe.EndpointRouting

open SharpStore.Web.Domain

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
            Func<IServiceProvider, OrderLineValidator>(fun (prov: IServiceProvider) ->
                Validation.orderLineValidator)
        )
        .AddTransient<SubmitOrder>(
            Func<IServiceProvider, SubmitOrder>(fun prov ->
                Service.submitOrder
                    Validation.orderValidator
                    (prov.GetService<GetProductId>())
                    Service.orderId
                    (prov.GetService<InsertOrder>()))
        )
        .AddGiraffe()
    |> ignore

    let app = builder.Build()

    app
        .UseStaticFiles()
        .UseRouting()
        .UseEndpoints(fun e -> e.MapGiraffeEndpoints(endpoints))
    |> ignore

    app.Run()

    0
