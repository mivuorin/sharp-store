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

[<Migration(2L, description = "Separate Order table to Order and OrderLines")>]
type CreateOrderLineTable() =
    inherit Migration()

    override this.Up() =
        let orderLine = this.Create.Table("OrderLine")

        orderLine.WithColumn("Id").AsGuid().PrimaryKey() |> ignore
        orderLine.WithColumn("OrderId").AsGuid().NotNullable() |> ignore
        orderLine.WithColumn("ProductCode").AsString(32).NotNullable() |> ignore

        this.Create
            .ForeignKey()
            .FromTable("OrderLine")
            .ForeignColumn("OrderId")
            .ToTable("Order")
            .PrimaryColumn("Id")
        |> ignore

        this.Delete.Column("ProductCode").FromTable("Order") |> ignore

    override this.Down() =
        this.Delete.Table("OrderLine") |> ignore

[<Migration(3L, description = "Add Quantity column to OrderLine")>]
type AddQuantityColumnToOrderLineTable() =
    inherit Migration()

    override this.Up() =
        this.Alter.Table("OrderLine").AddColumn("Quantity").AsDecimal().NotNullable()
        |> ignore

    override this.Down() =
        this.Delete.Column("Quantity").FromTable("OrderLine") |> ignore
