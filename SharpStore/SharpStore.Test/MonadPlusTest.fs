module SharpStore.Test.MonadPlusTest

open System.Threading.Tasks
open NUnit.Framework
open FsUnit

open FSharpPlus
open FSharpPlus.Data

let calculate a b : Task<int> = task { return a + b }

[<Test>]
let how_to_use_task_monad () =
    task {
        let! result = calculate 1 2
        result |> should equal 3
    }

let sumStrings a b =
    monad {
        let! a = tryParse a
        let! b = tryParse b
        return a + b
    }

[<Test>]
let maybe_monad () =
    sumStrings "1" "1" |> should equal (Some 2)
    sumStrings "1" "fpp" |> should equal None

// how to combine those?
// Following does not compile!
// let calculateStrings a b =
//     task {
//         let! a = tryParse a
//         let! b = tryParse a
//         return! calculate a b
//     }

// let calculateStrings (a:string) (b:string) : Task<Option<int>> =
//     monad {
//         let! a = tryParse a |> OptionT.hoist
//         let! b = tryParse b |> OptionT.hoist
//         return! (calculate a b) |> OptionT.lift
//     }
//     |> OptionT.run

// [<Test>]
// let sum_with_monad_transformer () =
//     task {
//         let! result = calculateStrings "1" "1"
//         result |> should equal (Some 2)
//
//         let! result = calculateStrings "1" "foo"
//         result |> should equal None
//     }
