namespace PromUa
open System

module Executor =
    let arguments = "promua last, prom daily"

    let parserArgs = function
                     | [| "prom"; "last" |] -> S.argTuple <- Argument.PromUa("last")
                     | [| "prom"; "daily" |] -> S.argTuple <- Argument.PromUa("daily")
                     | _ -> printf "Bad arguments, use %s" arguments
                            Environment.Exit(1)
    let parser = function
                 | PromUa d ->
                     P.parserPromUa d 
                 | _ -> ()
