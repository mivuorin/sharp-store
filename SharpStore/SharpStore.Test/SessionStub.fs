module SharpStore.Test.SessionStub

open FsUnit
open SharpStore.Web.Session

// Because ISession has generic functions which type is defined by caller.
// downcast and boxing is required to allow compiler to know function types before calling code.
// todo maybe better to use some existing mocking library
let sessionStub (add: string -> obj -> unit) (find: string -> obj) (remove: string -> unit) (tryFind: string -> obj) =
    { new ISession with
        override this.Add key value = add key (box value)
        override this.Find key = downcast (find key)
        override this.Remove key = remove key
        override this.TryFind key = downcast (tryFind key) }
