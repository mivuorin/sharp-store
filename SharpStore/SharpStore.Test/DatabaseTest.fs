module SharpStore.Test.DatabaseTest

open Xunit
open FsUnit

open SharpStore.Web.Domain
open SharpStore.Web.Database

registerTypes ()

[<Fact>]
let Insert_order () =
    let connectionString =
        "Server=localhost;Database=SharpStore;User Id=sa;Password=u4IDQGp119AtWV2SvH38184ufzSG4es7;TrustServerCertificate=true;"

    let order: ValidatedOrder = { ProductCode = "123" }

    task {
        use connection = connection connectionString
        let! inserted = insertOrder connection (orderId ()) order
        inserted |> should equal 1
    }
