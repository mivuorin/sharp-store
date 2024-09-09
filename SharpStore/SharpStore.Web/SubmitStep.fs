module SharpStore.Web.SubmitStep

open System

open Giraffe
open Giraffe.Htmx
open Giraffe.ViewEngine

open SharpStore.Web.Domain
open SharpStore.Web.Session

let submittedView (orderId: Guid) =
    [ h1 [] [ str "Thank you for your order!" ]
      p [] [ str "Your order has been successfully placed and will be processed soon." ]
      p [] [ str "Your order id is: " ]
      div [
          _id "order-id"
          _class "card text-center mb-3 mx-4"
      ] [ div [ _class "card-body" ] [ p [ _class "card-text" ] [ str (orderId.ToString()) ] ] ]
      p [] [ str "You will receive an email confirmation shortly with details about your order." ]
      p [] [ str "If you have any questions, please don't hesitate to contact us." ] ]
    |> Layout.main

let post: HttpHandler =
    fun next ctx ->
        task {
            // todo refactor to submit function
            let session = ctx.GetService<ISession>()
            let generateOrderId = ctx.GetService<GenerateOrderId>() // todo hardcoded dependency
            let insertOrder = ctx.GetService<InsertOrder>()

            let lines = session.Find<OrderLine list> "OrderLines"
            let contact = session.Find<Contact> "Contact"

            let orderId = generateOrderId ()
            let order = Order.create orderId lines contact

            do! insertOrder order

            session.Remove "OrderLines"
            session.Remove "Contact"

            let short = ShortGuid.fromGuid order.Id
            let url = $"/thanks/{short}" // todo Get url from route endpoint.
            return! withHxRedirect url next ctx
        }


let get (orderId: Guid) : HttpHandler = orderId |> submittedView |> htmlView
