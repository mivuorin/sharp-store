module SharpStore.Web.ProductReel

open System
open Giraffe
open Giraffe.ViewEngine
open Giraffe.ViewEngine.Htmx

open Microsoft.FSharp.Core
open SharpStore.Web.Domain


let card (product: Product) =
    let id = Random.Shared.Next(10, 50)

    div [ _class "card" ] [
        img [
            _class "card-img-top"
            _src $"https://picsum.photos/id/{id}/200?grayscale"
            _alt "Product photo"
        ]
        div [ _class "card-body" ] [
            h5 [ _class "card-title" ] [ product.ProductCode |> str ]
            p [] [ str "TODO Product Description" ]
        ]
    ]

let carouselItem index products =
    let cards = products |> List.map card

    let cssClass =
        seq {
            yield "carousel-item"

            if index = 0 then
                yield "active"
        }
        |> String.concat " "

    div [ _class cssClass ] [ div [ _class "d-flex flex-row justify-content-evenly" ] cards ]

let rec carousel products : XmlNode =
    let items = products |> List.chunkBySize 3 |> List.mapi carouselItem

    div [
        _id "product-reel"
        _class "carousel slide"
    ] [
        div [ _class "carousel-inner" ] items
        button [
            _class "carousel-control-prev"
            _type "button"
            _data "bs-target" "#product-reel"
            _data "bs-slide" "prev"
        ] [
            span [
                _class "carousel-control-prev-icon"
            //_aria_hidden todo aria labels
            ] []
            span [ _class "visually-hidden" ] [ str "Previous" ]
        ]
        button [
            _class "carousel-control-next"
            _type "button"
            _data "bs-target" "#product-reel"
            _data "bs-slide" "next"
        ] [
            span [
                _class "carousel-control-next-icon"
            //_aria_hidden todo aria labels
            ] []
            span [ _class "visually-hidden" ] [ str "Next" ]
        ]
    ]

let view =
    div [ _class "container" ] [
        p [] [

        ]
        div [
            _id "product-reel"
            _hxGet "/product-reel"
            _hxSwap "outerHTML"
            _hxTrigger "load"
        ] []

    ]


let get: HttpHandler =
    fun next ctx ->
        task {
            let getProducts = ctx.GetService<GetProducts>()
            let! products = getProducts ()
            let content = carousel products
            return! htmlView content next ctx
        }
