namespace PromUa

open System
open DocumentBuilder
open MySql.Data.MySqlClient
open System
open System.Data
open S
open Download
open Logging
open Newtonsoft.Json.Linq
open NewtonExt

type DocumentPromUa() =

      [<DefaultValue>] val mutable id: string
      [<DefaultValue>] val mutable hasLots: bool
      [<DefaultValue>] val mutable status: string
      [<DefaultValue>] val mutable publishDate: DateTime
      [<DefaultValue>] val mutable endDate: DateTime
      [<DefaultValue>] val mutable createDate: DateTime
      [<DefaultValue>] val mutable biddingDate: DateTime
      [<DefaultValue>] val mutable amount: decimal
      [<DefaultValue>] val mutable descr: string
      [<DefaultValue>] val mutable purObj: string
      [<DefaultValue>] val mutable orgName: string
      [<DefaultValue>] val mutable currency: string
      inherit AbstractDocument("Zakupki.Prom.ua", AbstractParser.EndPoint, 204)
      interface IDocument with
        override __.Worker() =
            __.WorkerEntity()

      new(id, hasLots, status, publishDate, endDate, createDate, biddingDate, amount, descr, purObj, orgName, currency) as this = DocumentPromUa()
                                                                                                                                  then this.id <- id
                                                                                                                                       this.hasLots <- hasLots
                                                                                                                                       this.status <- status
                                                                                                                                       if publishDate = DateTime.MinValue then this.publishDate <- createDate else this.publishDate <- publishDate
                                                                                                                                       if endDate = DateTime.MinValue then this.endDate <- createDate else this.endDate <- endDate

                                                                                                                                       this.createDate <- createDate
                                                                                                                                       this.biddingDate <- biddingDate
                                                                                                                                       this.amount <- amount
                                                                                                                                       this.descr <- descr
                                                                                                                                       this.purObj <- purObj
                                                                                                                                       this.orgName <- orgName
                                                                                                                                       this.currency <- currency
      member __.WorkerEntity() =
            let builder = DocumentBuilder()
            use con = new MySqlConnection(Settings.ConS)
            let res =
                       builder {
                            con.Open()
                            let selectTend = sprintf "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND notice_version = @notice_version AND doc_publish_date = @doc_publish_date AND date_version = @date_version" Settings.Pref
                            let cmd: MySqlCommand = new MySqlCommand(selectTend, con)
                            cmd.Prepare()
                            cmd.Parameters.AddWithValue("@purchase_number", __.id) |> ignore
                            cmd.Parameters.AddWithValue("@type_fz", __.typeFz) |> ignore
                            cmd.Parameters.AddWithValue("@end_date", __.endDate) |> ignore
                            cmd.Parameters.AddWithValue("@notice_version", __.status) |> ignore
                            cmd.Parameters.AddWithValue("@doc_publish_date", __.publishDate) |> ignore
                            cmd.Parameters.AddWithValue("@date_version", __.createDate) |> ignore
                            let reader: MySqlDataReader = cmd.ExecuteReader()
                            if reader.HasRows then reader.Close()
                                                   return! Error ""
                            reader.Close()
                            let o = __.ReturnPageTender()
                            Console.WriteLine(o.ToString())
                            return ""
                            }
            match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
      
      member private __.ReturnPageTender(): JObject =
          let url = sprintf "%s/remote/api/v2/entity/%s" AbstractParser.EndPoint __.id
          let res = HttpClientPromUa.DownloadSringPromUa(url)
          let j = JObject.Parse(res)
          match GetStringFromJtoken j "status" with
          | "ok" -> j
          | _ -> failwith <| j.ToString()
          