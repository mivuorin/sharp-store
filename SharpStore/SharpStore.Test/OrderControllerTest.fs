module SharpStore.Test.OrderControllerTest

open System
open FsUnit
open SharpStore.Web
open SharpStore.Web.Domain
open SharpStore.Web.Session
open Xunit

// Because ISession has generic functions which type is defined by caller.
// downcast and boxing is required to allow compiler to know function types before calling code.
// todo maybe better to use some existing mocking library
let sessionMock (add: string -> obj -> unit) (find: string -> obj) (remove: string -> unit) (tryFind: string -> obj) =
    { new ISession with
        override this.Add key value = add key (box value)
        override this.Find key = downcast (find key)
        override this.Remove key = remove key
        override this.TryFind key = downcast (tryFind key) }

[<Fact>]
let Init_when_order_does_not_exist_in_session () =

    let order =
        { Id = Guid.NewGuid()
          OrderLines = [] }

    let createOrder: Order.CreateOrder = fun () -> order

    let session =
        { new ISession with
            override this.Add key value =
                key |> should equal "order"
                value |> should equal order

            override this.Find(key) = failwith "unused"
            override this.Remove(key) = failwith "unused"
            override this.TryFind(key) = None }

    let model = OrderController.initOrderView session createOrder ()

    let expectedOrderLine =
        { OrderLineForm.ProductCode = ""
          Quantity = "" }

    model.Order |> should equal order
    model.OrderLineModel.Form |> should equal expectedOrderLine
    model.OrderLineModel.Errors |> should equal Map.empty<string, string list>


[<Fact>]
let Init_when_order_exist_in_session () =
    let order =
        { Id = Guid.NewGuid()
          OrderLines = [] }

    let createOrder: Order.CreateOrder = fun _ -> failwith "createOrder"

    let session =
        sessionMock (fun _ _ -> failwith "add") (fun _ -> failwith "find") (fun _ -> failwith "remove") (fun _ ->
            Some order)

    let model = OrderController.initOrderView session createOrder ()

    model.OrderLineModel.Form
    |> should
        equal
        { OrderLineForm.ProductCode = ""
          Quantity = "" }

    model.Order |> should equal order
    model.OrderLineModel.Errors |> should equal Map.empty<string, string list>
