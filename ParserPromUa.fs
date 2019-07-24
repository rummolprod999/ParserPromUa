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
          let url = String.Format("{0}{1}{2}{3}{4}{5}", AbstractParser.EndPoint, "/remote/api/v2/entity?search=search_query&", "range_date_created_dt_start=", dateNow, "&range_date_created_dt_end=", dateNowMinusAfewDays)
          let url = "https://my.zakupki.prom.ua/remote/api/v2/entity?search=search_query&range_date_created_dt_start=21.07.2019&range_date_created_dt_end=23.07.2019"
          let res = HttpClientPromUa.DownloadSringPromUa(url)
          Console.WriteLine(res)
          let j = JObject.Parse(res)
          Console.WriteLine(j)
          ()