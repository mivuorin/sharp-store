module SharpStore.Test.DatabaseTest

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

    let expectedId = orderId ()

    let expectedFirst =
        { ProductCode = "01"
          Quantity = 1m }

    let expectedSecond =
        { ProductCode = "02"
          Quantity = 2.5m }

    let validatedOrder: ValidatedOrder =
        { ProductCodes =
            [ expectedFirst
              expectedSecond ] }

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
            }
            |> connection.SelectAsync<OrderLine>

        actual |> Seq.length |> should equal 2

        let lines = actual |> Seq.toList

        let first = lines |> List.item 0
        first.OrderId |> should equal expectedId
        first.ProductCode |> should equal expectedFirst.ProductCode
        first.Quantity |> should equal expectedFirst.Quantity

        let second = lines |> List.item 1
        second.OrderId |> should equal expectedId
        second.ProductCode |> should equal expectedSecond.ProductCode
        second.Quantity |> should equal expectedSecond.Quantity
    }
