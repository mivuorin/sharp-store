module SharpStore.Web.Migrations

open FluentMigrator

[<Migration(1L, description = "Create Order table")>]
type CreateOrderTable() =
    inherit Migration()

    override this.Up() =
        this.Create
            .Table("Order")
            .WithColumn("Id")
            .AsGuid()
            .PrimaryKey()
            .WithColumn("ProductCode")
            .AsString(32)
            .NotNullable()
        |> ignore

    override this.Down() = this.Delete.Table("Order") |> ignore
