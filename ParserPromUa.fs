namespace PromUa
open System
open Download
open Logging
open Newtonsoft.Json.Linq
open NewtonExt
open DocumentBuilder

type ParserPromUa(dir: string) =
      inherit AbstractParser()

      let mutable startUrl = ""

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
          startUrl <- String.Format ("{0}{1}{2}{3}{4}{5}", AbstractParser.EndPoint, "/remote/api/v2/entity?", "range_date_created_dt_start=", dateNowMinusAfewDays, "&range_date_created_dt_end=", dateNow)
          let res = HttpClientPromUa.DownloadSringPromUa(startUrl)
          let j = JObject.Parse(res)
          match GetStringFromJtoken j "status" with
          | "ok" -> __.StartParsingListTenders j
          | _ -> failwith <| j.ToString()
          ()
      
      member private __.GetClassifiers() =
           let starturl = String.Format ("{0}{1}", AbstractParser.EndPoint, "/remote/api/v1/primary_classifier")
           let res = HttpClientPromUa.DownloadSringPromUa(starturl)
           printfn "%s" res
           ()
      member private __.StartParsingListTenders(j: JObject) =
            __.PrepareList(j)
            let totalItems = GetIntFromJtoken j "data.total_items"
            if totalItems = 0 then failwith "totalItems is zero"
            let mutable maxpage = totalItems / 20 + 1
            if maxpage > 500 then
                  maxpage <- 500
            let mutable res = ""
            for i in 2..maxpage do
                  try
                        let url = sprintf "%s&page=%d" startUrl i
                        res <- HttpClientPromUa.DownloadSringPromUa(url)
                        let t = JObject.Parse(res)
                        __.PrepareList(t)
                  with e -> Log.logger (e)
            ()


      member private __.PrepareList(j: JObject) =
            let items = j.GetElements("..items")
            for el in items do
                  try
                       __.CreateDoc el
                  with ex -> Log.logger (ex, el.ToString())
            ()

      member private __.CreateDoc(item: JToken) =
            let builder = DocumentBuilder()
            let res =
                   builder {
                         let! id = item.StDString "id" <| sprintf "id not found %s" (item.ToString())
                         let! hasLots = item.StDBool "has_lots" <| sprintf "has_lots not found %s" (item.ToString())
                         let status = GetStringFromJtoken item "status_text"
                         let publishDate = item.StDDateTimeB "tendering_period.start"
                         let endDate = item.StDDateTimeB "tendering_period.end"
                         let! createDate = item.StDDateTime "date_created" <| sprintf "date_created not found %s" (item.ToString())
                         let biddingDate = item.StDDateTimeB "auction_period.start"
                         let amount = GetDecimalFromJtoken item "amount"
                         let descr = GetStringFromJtoken item "descr"
                         let! purObj = item.StDString "title" <| sprintf "title not found %s" (item.ToString())
                         let orgName = GetStringFromJtoken item "merchant_name"
                         let currency = GetStringFromJtoken item "currency_iso_code"
                         let doc  = DocumentPromUa(id, hasLots, status, publishDate, endDate, createDate, biddingDate, amount, descr, purObj, orgName, currency)
                         try
                               doc.WorkerEntity()
                         with e -> Log.logger(e, item.ToString())
                         return ""
                   }
            match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger (r, item.ToString())
            ()
