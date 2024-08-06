namespace SharpStore.Web.Domain

module WidgetCode =

    type WidgetCode = WidgetCode of string

    let value (WidgetCode code) = code

module GadgetCode =

    type GadgetCode = GadgetCode of string

    let value (GadgetCode code) = code

type ProductCode =
    | Widget of WidgetCode.WidgetCode
    | Gadget of GadgetCode.GadgetCode

module ProductCode =
    let value (code: ProductCode) =
        match code with
        | Widget widgetCode -> WidgetCode.value widgetCode
        | Gadget gadgetCode -> GadgetCode.value gadgetCode
