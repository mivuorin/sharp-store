module SharpStore.Web.Database

open System
open System.Data
open Dapper.FSharp.MSSQL

open SharpStore.Web.Domain

let registerTypes () = OptionTypes.register ()

let connection: string -> IDbConnection =
    fun connectionString -> new Microsoft.Data.SqlClient.SqlConnection(connectionString)

type private Order =
    { Id: Guid
      ProductCode: string }

let private orderTable = table<Order>

let insertOrder: IDbConnection -> InsertOrder =
    fun connection ->
        fun orderId validatedOrder ->
            let order =
                { Id = orderId
                  ProductCode = validatedOrder.ProductCode }

            insert {
                into orderTable
                value order
            }
            |> connection.InsertAsync
