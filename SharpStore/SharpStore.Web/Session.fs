namespace SharpStore.Web.Session

open Giraffe
open Microsoft.AspNetCore.Http

type ISession =
    abstract Find<'a> : key: string -> 'a
    abstract TryFind<'a> : key: string -> 'a option
    abstract Add<'a> : key: string -> value: 'a -> unit
    abstract Remove: key: string -> unit

type Session(serializer: Json.ISerializer, ctxAccessor: IHttpContextAccessor) =
    interface ISession with
        member this.Find<'a>(key: string) =
            let bytes = ctxAccessor.HttpContext.Session.Get(key)
            serializer.Deserialize<'a>(bytes)

        member this.Add<'a> (key: string) (value: 'a) =
            let bytes = serializer.SerializeToBytes(value)
            ctxAccessor.HttpContext.Session.Set(key, bytes)

        member this.Remove(key: string) =
            ctxAccessor.HttpContext.Session.Remove(key)

        member this.TryFind<'a>(key: string) : 'a option =
            let found, bytes = ctxAccessor.HttpContext.Session.TryGetValue(key)

            if found then
                serializer.Deserialize(bytes) |> Some
            else
                None
