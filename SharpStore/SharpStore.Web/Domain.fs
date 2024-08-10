﻿namespace SharpStore.Web.Domain

open System
open System.Threading.Tasks
open Microsoft.FSharp.Core

open Validus

[<CLIMutable>]
type OrderLineForm =
    { ProductCode: string
      Quantity: string }

// Giraffe bindFormAsync<T> function has bug which leaves list uninitialized (null) when
// there is no form values to bind. To avoid this use array instead of list.
[<CLIMutable>]
type OrderForm = { OrderLines: OrderLineForm array }

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

// todo ValidatedOrderLine is currently middle type only used in validation.
type ValidatedOrderLine =
    { ProductCode: ProductCode
      Quantity: decimal }

type Contact =
    { Name: string
      Email: string
      Phone: string option }

// todo redesign ValidatedOrder.
type ValidatedOrder = { OrderLines: ValidatedOrderLine list }

type OrderLine =
    { ProductId: Guid
      ProductCode: ProductCode
      Quantity: decimal }

// todo ValidatedContact instead of reusing ContactForm!
// todo there's requirement for type like submitOrder: -> OrderLine list -> Contact -> Shipping -> OrderCreated

type Order =
    { Id: Guid
      // todo could use F#+ NonEmptyList
      OrderLines: OrderLine list
      Contact: Contact }

type OrderCreated = { Id: Guid }

// Replace OrderLineValidator with Validius.Validator type?
type OrderLineValidator = OrderLineForm -> ValidationResult<ValidatedOrderLine>
type OrderValidator = OrderForm -> ValidationResult<ValidatedOrder>
type ContactValidator = ContactForm -> ValidationResult<Contact>

// todo Have own type for order id instead of guid.
type GenerateOrderId = unit -> Guid

// Services
type ValidateOrderLine = OrderLineForm -> Task<ValidationResult<OrderLine>>

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

module Order =
    let create orderId lines contact : Order =
        { Id = orderId
          OrderLines = lines
          Contact = contact }

module OrderLineForm =
    let init: OrderLineForm =
        { ProductCode = ""
          Quantity = "" }

module ContactForm =
    let init: ContactForm =
        { Name = ""
          Email = ""
          Phone = "" }

    let from (contact: Contact) =
        { ContactForm.Name = contact.Name
          Email = contact.Email
          Phone = Option.defaultValue "" contact.Phone }
