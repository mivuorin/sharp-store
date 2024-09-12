module SharpStore.Web.OrderIdGenerator

open System
open SharpStore.Web.Domain

let gen: GenerateOrderId = Guid.NewGuid
