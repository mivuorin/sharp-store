module SharpStore.Web.Database

open System
open System.Data
open System.Threading.Tasks
open Dapper.FSharp.MSSQL

open SharpStore.Web.Domain

let registerTypes () = OptionTypes.register ()

type Connection = unit -> IDbConnection

let connection (connectionString: string) : Connection =
    fun () -> new Microsoft.Data.SqlClient.SqlConnection(connectionString)

type Order =
    { Id: Guid
      ContactId: Guid }

type OrderLine =
    { Id: Guid
      OrderId: Guid
      ProductId: Guid
      Quantity: decimal }

type Product =
    { Id: Guid
      ProductCode: string }

type Contact =
    { Id: Guid
      Name: string
      Email: string
      Phone: string option }

let orderTable = table<Order>
let orderLineTable = table<OrderLine>
let productTable = table<Product>
let contactTable = table<Contact>

let toOrderLine (orderId: Guid) (orderLine: Domain.OrderLine) =
    { Id = Guid.NewGuid()
      OrderId = orderId
      ProductId = orderLine.ProductId
      Quantity = orderLine.Quantity }

let insertOrder (connection: Connection) : InsertOrder =
    fun order ->
        task {
            use connection = connection ()
            connection.Open()

            let transaction: IDbTransaction = connection.BeginTransaction()

            let contact: Contact =
                { Id = Guid.NewGuid()
                  Name = order.Contact.Name
                  Email = order.Contact.Email
                  Phone = order.Contact.Phone }

            let insertContact =
                insert {
                    into contactTable
                    value contact
                }

            do! connection.InsertAsync(insertContact, transaction) :> Task

            let insertOrder =
                insert {
                    into orderTable

                    value
                        { Id = order.Id
                          ContactId = contact.Id }
                }

            do! connection.InsertAsync(insertOrder, transaction) :> Task

            let orderLines = order.OrderLines |> List.map (toOrderLine order.Id)

            // todo Insert order lines crashes when there is no order lines.
            let insertOrderLines =
                insert {
                    into orderLineTable
                    values orderLines
                }

            do! connection.InsertAsync(insertOrderLines, transaction) :> Task

            transaction.Commit()
        }

let getProductId (connection: Connection) : GetProductId =
    fun code ->
        task {
            use connection = connection ()

            let code = ProductCode.value code

            let! products =
                select {
                    for product in productTable do
                        where (product.ProductCode = code)
                }
                |> connection.SelectAsync<Product>

            return Seq.tryHead products |> Option.map _.Id
        }
