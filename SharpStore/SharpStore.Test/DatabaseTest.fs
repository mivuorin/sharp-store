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

    let expectedOrderLines =
        [ "01"
          "02" ]

    let validatedOrder: ValidatedOrder = { ProductCodes = expectedOrderLines }

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

        let! actualOrderLines =
            select {
                for line in orderLineTable do
                    where (line.OrderId = expectedId)
            }
            |> connection.SelectAsync<OrderLine>

        actualOrderLines |> Seq.length |> should equal 2
        actualOrderLines |> Seq.map _.ProductCode |> should equal expectedOrderLines
    }
