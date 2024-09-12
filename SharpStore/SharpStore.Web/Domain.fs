namespace SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core

open Validus

[<CLIMutable>]
type OrderLineForm =
    { ProductCode: string
      Quantity: string }

[<CLIMutable>]
type ContactForm =
    { Name: string
      Email: string
      Phone: string }

type WidgetCode = WidgetCode of string

type GadgetCode = GadgetCode of string

type ProductCode =
    | Widget of WidgetCode
    | Gadget of GadgetCode

type Contact =
    { Name: string
      Email: string
      Phone: string option }

type OrderLine =
    { ProductId: Guid
      ProductCode: ProductCode
      Quantity: decimal }

type Order =
    { Id: Guid
      // todo could use F#+ NonEmptyList
      OrderLines: OrderLine list
      Contact: Contact }

type OrderCreated = { Id: Guid }

type OrderLineValidator = OrderLineForm -> Task<ValidationResult<OrderLine>>

type ContactValidator = ContactForm -> ValidationResult<Contact>

// todo Have own type for order id instead of guid.
type GenerateOrderId = unit -> Guid

// Database
type InsertOrder = Order -> Task
type GetProductId = ProductCode -> Task<Guid option>

module WidgetCode =
    let value (WidgetCode code) = code

module GadgetCode =
    let value (GadgetCode code) = code

module ProductCode =
    let value (code: ProductCode) =
        match code with
        | Widget widgetCode -> WidgetCode.value widgetCode
        | Gadget gadgetCode -> GadgetCode.value gadgetCode

// todo create ProductCode from string

module Order =
    let create orderId lines contact : Order =
        { Id = orderId
          OrderLines = lines
          Contact = contact }

module OrderLineForm =
    let empty: OrderLineForm =
        { ProductCode = ""
          Quantity = "" }

module ContactForm =
    let empty: ContactForm =
        { Name = ""
          Email = ""
          Phone = "" }

    let from (contact: Contact) =
        { ContactForm.Name = contact.Name
          Email = contact.Email
          Phone = Option.defaultValue "" contact.Phone }
