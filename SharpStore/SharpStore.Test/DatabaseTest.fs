module SharpStore.Test.DatabaseTest

open SharpStore.Web
open Xunit
open FsUnit

open SharpStore.Web.Domain

open Dapper.FSharp.MSSQL

Database.registerTypes ()

let connectionString =
    "Server=localhost;Database=SharpStore;User Id=sa;Password=u4IDQGp119AtWV2SvH38184ufzSG4es7;TrustServerCertificate=true;MultipleActiveResultSets=True;"

let connectionFactory = Database.connection connectionString

[<Fact>]
let Insert_order () =
    let expectedId = OrderIdGenerator.gen ()
    let getProductId = Database.getProductId connectionFactory

    let validateOrderLine = Validation.orderLineValidator getProductId

    task {
        use connection = connectionFactory ()

        let! line1 =
            validateOrderLine
                { ProductCode = "W0001"
                  Quantity = "1" }

        let! line2 =
            validateOrderLine
                { ProductCode = "G200"
                  Quantity = "2,5" }

        let orderLines =
            [ (Result.toValue line1)
              (Result.toValue line2) ]

        let contactForm: ContactForm =
            { Email = "test@test.com"
              Name = "Test User"
              Phone = "+1230006549874" }

        let contact = Validation.contactValidator contactForm |> Result.toValue

        let order = Order.create expectedId orderLines contact

        do! Database.insertOrder connectionFactory order

        let! actualOrder =
            select {
                for o in Database.orderTable do
                    where (o.Id = expectedId)
            }
            |> connection.SelectAsync<Database.Order>

        Seq.length actualOrder |> should equal 1

        let! actual =
            select {
                for line in Database.orderLineTable do
                    where (line.OrderId = expectedId)
                    orderBy line.Quantity
            }
            |> connection.SelectAsync<Database.OrderLine>

        // todo join Product table

        actual |> Seq.length |> should equal 2

        let lines = actual |> Seq.toList

        let first = lines |> List.item 0
        first.OrderId |> should equal expectedId
        // first.ProductCode |> should equal "W0001"
        first.Quantity |> should equal 1

        let second = lines |> List.item 1
        second.OrderId |> should equal expectedId
        // second.ProductCode |> should equal "G002"
        second.Quantity |> should equal 2.5m
    }

[<Fact>]
let Get_product_id () =
    task {
        let code =
            Validation.productCodeValidator "test" "W0001"
            |> Result.defaultWith (fun _ -> failwith "invalid product code")

        let! actualId = Database.getProductId connectionFactory code
        actualId |> Option.isSome |> should equal true
    }
