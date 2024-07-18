module SharpStore.E2E.Program

open System
open canopy
open canopy.classic
open canopy.runner.classic

let index_test () =

    context "Index"

    "hello canopy test"
    &&& fun _ ->

        url "https://localhost:7285"

        "h1" == "Welcome to Giraffe"

        waitFor (fun () -> title () = "SharpStore")


[<EntryPoint>]
let main _ =
    start chrome

    index_test ()

    run ()

    quit ()

    0
