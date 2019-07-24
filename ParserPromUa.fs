namespace PromUa
open System
open Download
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

type ParserPromUa(dir : string) =
      inherit AbstractParser()
      interface Iparser with

            override __.Parsing() =
                  __.Login()
                  match dir with
                  | "daily" -> __.GetTendersList()
                  | "last" | _ -> ()

      
      member private __.GetTendersList() =
          let dtn = DateTime.Now
          let dateNow = String.Format("{0:dd.MM.yyyy}", dtn.AddDays(1.))
          let dateNowMinusAfewDays = String.Format("{0:dd.MM.yyyy}", dtn.AddDays(-2.))
          let url = String.Format("{0}{1}{2}{3}{4}{5}", AbstractParser.EndPoint, "/remote/api/v2/entity?", "range_date_created_dt_start=", dateNowMinusAfewDays, "&range_date_created_dt_end=", dateNow)
          //let url = "https://my.zakupki.prom.ua/remote/api/v2/entity?range_date_created_dt_start=21.07.2019&range_date_created_dt_end=23.07.2019"
          let res = HttpClientPromUa.DownloadSringPromUa(url)
          let j = JObject.Parse(res)
          match GetStringFromJtoken j "status" with
          | "ok" -> ()
          | _ -> failwith <| j.ToString()
          Console.WriteLine(j)
          ()