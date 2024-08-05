module SharpStore.Web.Database

open System
open System.Data
open System.Threading.Tasks
open Dapper.FSharp.MSSQL

open SharpStore.Web.Domain

let registerTypes () = OptionTypes.register ()

let connection: string -> IDbConnection =
    fun connectionString -> new Microsoft.Data.SqlClient.SqlConnection(connectionString)

type Order = { Id: Guid }

type OrderLine =
    { Id: Guid
      OrderId: Guid
      ProductCode: string
      Quantity: decimal }

let orderTable = table<Order>
let orderLineTable = table<OrderLine>

let toOrderLine orderId (orderLine: ValidatedOrderLine) =
    let code =
        match orderLine.ProductCode with
        | Widget widget -> WidgetCode.value widget
        | Gadget gadget -> GadgetCode.value gadget

    { Id = Guid.NewGuid()
      OrderId = orderId
      ProductCode = code
      Quantity = orderLine.Quantity }

let insertOrder: IDbConnection -> InsertOrder =
    fun connection ->
        fun orderId validatedOrder ->
            task {
                connection.Open()

                let transaction: IDbTransaction = connection.BeginTransaction()

                let insertOrder =
                    insert {
                        into orderTable
                        value { Id = orderId }
                    }

                do! connection.InsertAsync(insertOrder, transaction) :> Task

                let orderLines = validatedOrder.ProductCodes |> List.map (toOrderLine orderId)

                let insertOrderLines =
                    insert {
                        into orderLineTable
                        values orderLines
                    }

                do! connection.InsertAsync(insertOrderLines, transaction) :> Task

                transaction.Commit()
            }
