namespace PromUa

open System
open System.Globalization
open System.Text.RegularExpressions
open RegExpLibrary

module StringExt =

    type System.String with

        member this.Get1FromRegexp(regex: string): string option =
            match this with
            | RegexMatch1 regex gr1 -> Some(gr1)
            | _ -> None

        member this.GetPriceFromString(?template): string =
            let templ = defaultArg template @"([\d, ]+)"
            match this.Get1FromRegexp templ with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""

        member this.DateFromStringRus(pat: string) =
            try
                DateTime.ParseExact(this, pat, CultureInfo.CreateSpecificCulture("ru-RU"))
            with ex -> DateTime.MinValue
