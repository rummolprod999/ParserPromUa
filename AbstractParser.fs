namespace PromUa
open Logging
open System
open Microsoft.FSharp.Data
open System.Linq
open System.Collections.Generic
open System.IO
open System.Linq
open System.Text
open System.Xml
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NewtonExt
open Download

[<AbstractClass>]
type AbstractParser() =
    static member  EndPoint = "https://my.zakupki.prom.ua"
    
    member __.Login() =
        let url = sprintf "%s%s" AbstractParser.EndPoint "/remote/api/v1/login"
        let data = sprintf "{ \"phone_email\": \"%s\", \"password\": \"%s\"}" S.User.User S.User.Pass
        let res = HttpClientPromUa.DownloadSringPromUa(url, data, "post") 
        let j = JObject.Parse(res)
        let token = GetStringFromJtoken j "acc_token"
        if token = "" then
            Log.logger j
            Environment.Exit(1)
        S.Token <- token
        //Console.WriteLine(token)
        ()