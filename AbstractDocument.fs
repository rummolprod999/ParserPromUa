namespace PromUa

[<AbstractClass>]
type AbstractDocument() =
    static member val Add : int = 0 with get, set
    static member val Upd : int = 0 with get, set