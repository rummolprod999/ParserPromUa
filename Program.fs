// Learn more about F# at http://fsharp.org
namespace PromUa
open System

module EntryPoint =

        [<EntryPoint>]
        let main argv =
            if argv.Length = 0 then
                printf "Bad arguments, use %s" Executor.arguments
                Environment.Exit(1)
            Executor.parserArgs argv
            Stn.getSettings()
            Executor.parser (S.argTuple)
            0
