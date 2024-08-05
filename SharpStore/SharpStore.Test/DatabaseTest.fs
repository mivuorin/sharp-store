module SharpStore.Test.DatabaseTest

open SharpStore.Web
open Xunit
open FsUnit

open SharpStore.Web.Domain
open SharpStore.Web.Database

open Dapper.FSharp.MSSQL

registerTypes ()

[<Fact>]
let Insert_order () =
    let connectionString =
        "Server=localhost;Database=SharpStore;User Id=sa;Password=u4IDQGp119AtWV2SvH38184ufzSG4es7;TrustServerCertificate=true;"

    let expectedId = Domain.orderId ()

    let form: OrderForm =
        { OrderLines =
            [| { ProductCode = "W0001"
                 Quantity = "1" }
               { ProductCode = "G002"
                 Quantity = "2,5" } |] }

    let r = Validation.orderValidator form

    let validatedOrder: ValidatedOrder =
        match r with
        | Ok valid -> valid
        | Error e -> Printf.failwithf $"Invalid order: %A{e}"

    task {
        use connection = connection connectionString

        do! insertOrder connection expectedId validatedOrder

        let! actualOrder =
            select {
                for o in orderTable do
                    where (o.Id = expectedId)
            }
            |> connection.SelectAsync<Order>

        Seq.length actualOrder |> should equal 1

        let! actual =
            select {
                for line in orderLineTable do
                    where (line.OrderId = expectedId)
                    orderBy line.OrderId
            }
            |> connection.SelectAsync<OrderLine>

        actual |> Seq.length |> should equal 2

        let lines = actual |> Seq.toList

        let first = lines |> List.item 0
        first.OrderId |> should equal expectedId
        first.ProductCode |> should equal "W0001"
        first.Quantity |> should equal 1

        let second = lines |> List.item 1
        second.OrderId |> should equal expectedId
        second.ProductCode |> should equal "G002"
        second.Quantity |> should equal 2.5m
    }
