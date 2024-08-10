module SharpStore.Web.Migrations

open System
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

[<Migration(4L, description = "Product table")>]
type ProductTable() =
    inherit Migration()

    override this.Up() =
        let product = this.Create.Table("Product")
        product.WithColumn("Id").AsGuid().PrimaryKey() |> ignore
        product.WithColumn("ProductCode").AsString(32).NotNullable() |> ignore

        this.Delete.Column("ProductCode").FromTable("OrderLine") |> ignore

        this.Alter.Table("OrderLine").AddColumn("ProductId").AsGuid().NotNullable()
        |> ignore

        this.Create
            .ForeignKey()
            .FromTable("OrderLine")
            .ForeignColumn("ProductId")
            .ToTable("Product")
            .PrimaryColumn("Id")
        |> ignore

        this.Create.UniqueConstraint().OnTable("Product").Column("ProductCode")
        |> ignore

        this.Insert
            .IntoTable("Product")
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "W0001" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "W0002" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "W0003" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "W0004" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "W0005" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "G100" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "G200" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "G300" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "G400" |}
            )
            .Row(
                {| Id = Guid.NewGuid()
                   ProductCode = "G500" |}
            )
        |> ignore

    override this.Down() =
        this.Delete
            .ForeignKey()
            .FromTable("OrderLine")
            .ForeignColumn("ProductId")
            .ToTable("Product")
            .PrimaryColumn("Id")

        this.Alter
            .Table("OrderLine")
            .AddColumn("ProductCode")
            .AsString(32)
            .NotNullable()
        |> ignore

        this.Delete.Column("ProductId").FromTable("OrderLine") |> ignore
        this.Delete.UniqueConstraint().FromTable("Product").Column("ProductCode")
        this.Delete.Table("Product") |> ignore


[<Migration(5L, description = "Contact table")>]
type ContactTable() =
    inherit Migration()

    override this.Up() =
        let contact = this.Create.Table("Contact")
        contact.WithColumn("Id").AsGuid().PrimaryKey() |> ignore
        contact.WithColumn("Name").AsString(50).NotNullable() |> ignore
        contact.WithColumn("Email").AsString(254).NotNullable() |> ignore
        contact.WithColumn("Phone").AsString(16).Nullable() |> ignore

        this.Alter.Table("Order").AddColumn("ContactId").AsGuid().NotNullable |> ignore

        this.Create
            .ForeignKey()
            .FromTable("Order")
            .ForeignColumn("ContactId")
            .ToTable("Contact")
            .PrimaryColumn("Id")
        |> ignore

    override this.Down() =
        this.Delete
            .ForeignKey()
            .FromTable("Order")
            .ForeignColumn("ContactId")
            .ToTable("Contact")
            .PrimaryColumn("Id")

        this.Delete.Table("Contact") |> ignore

        this.Delete.Column("ContactId").FromTable("Order") |> ignore
