module SharpStore.Test.Result

let toValue =
    function
    | Ok value -> value
    | Error _ -> failwith "Expected Ok result"

let toError =
    function
    | Ok _ -> failwith "Expected Error result"
    | Error error -> error
