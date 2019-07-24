namespace PromUa
open Logging
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
                     try
                        P.parserPromUa d
                     with e -> Log.logger e
                 | _ -> ()
