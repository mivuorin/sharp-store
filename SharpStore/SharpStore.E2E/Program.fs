module SharpStore.E2E.Program

open canopy
open canopy.classic
open canopy.runner.classic

let indexPageUrl = "https://localhost:7285"

let index_test () =

    context "Index"

    before (fun () -> url indexPageUrl)

    "hello canopy test"
    &&& fun _ ->
        "h1" == "Welcome to Giraffe"

        waitFor (fun () -> title () = "SharpStore")

    "navigate to order page"
    &&& fun _ ->
        click "Order Here!"

        "h1" == "Products"

let submit_order_test () =
    context "Submit order"

    once (fun () -> url (indexPageUrl + "/order"))

    "order has no order lines"
    &&& fun _ -> "#order-lines > .row" == "Your order is empty!"

    "next is disabled when there is no orders" &&& fun _ -> disabled "Next"

    "remove product"
    &&& fun _ ->
        "#add-product-code" << "G200"
        "#add-product-quantity" << "2"
        click "Add Product"

        "#order-lines > div > span" *= "G200"
        "#order-lines > div > span" *= "2"

        click "Remove"

        "#order-lines > .row" == "Your order is empty!"

    // todo ideal way to split long stateful test into smaller one?
    "full order process"
    &&& fun _ ->
        "#add-product-code" << "W0005"
        "#add-product-quantity" << "12,5"
        click "Add Product"

        "#add-product-code" << "G500"
        "#add-product-quantity" << "20"
        click "Add Product"

        click "Next"

        "h1" == "Contact information"

        "#contact-name" << "Canopy User"
        "#contact-email" << "canopy.user@test.com"
        "#contact-phone" << "+3589616250"

        click "Next"

        "h1" == "Review your order"

        "#order-lines > .row > .col" *= "W0005"
        "#order-lines > .row > .col" *= "12.5"

        "#order-lines > .row > .col" *= "G500"
        "#order-lines > .row > .col" *= "20"

        "#contact-information span" *= "Canopy User"
        "#contact-information span" *= "canopy.user@test.com"
        "#contact-information span" *= "+3589616250"

        click "Submit Order"

        "h1" == "Thank you for your order!"

        "#order-id" != ""

[<EntryPoint>]
let main _ =
    // todo Disable chrome default search engine -dialog
    // eg. pass "--disable-search-engine-choice-screen argument" to selenium
    configuration.failFast.Value <- true
    configuration.runFailedContextsFirst <- false

    start chrome

    index_test ()
    submit_order_test ()

    run ()

    quit ()

    0
