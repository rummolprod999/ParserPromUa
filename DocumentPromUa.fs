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
open System.Collections.Generic

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
                            let item = __.ReturnPageTender()
                            let tendOrigNum = GetStringFromJtoken item "data.tuid"
                            let (cancelStatus, updated) = __.SetCancelStatus(con, __.createDate, __.id)
                            let printForm = GetStringFromJtoken item "data.public_link_tender"
                            let idEtp = __.GetEtp con S.Settings
                            let pwName = GetStringFromJtoken item "data.method_type_text"
                            let numVersion = 1
                            let idPlacingWay = ref 0
                            if pwName <> "" then idPlacingWay := __.GetPlacingWay con pwName S.Settings
                            let IdOrg = ref 0
                            if __.orgName <> "" then
                                let selectOrg = sprintf "SELECT id_organizer FROM %sorganizer WHERE full_name = @full_name" S.Settings.Pref
                                let cmd3 = new MySqlCommand(selectOrg, con)
                                cmd3.Prepare()
                                cmd3.Parameters.AddWithValue("@full_name", __.orgName) |> ignore
                                let reader = cmd3.ExecuteReader()
                                match reader.HasRows with
                                | true ->
                                    reader.Read() |> ignore
                                    IdOrg := reader.GetInt32("id_organizer")
                                    reader.Close()
                                | false ->
                                    reader.Close()
                                    let addOrganizer = sprintf "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn, contact_email = @contact_email" S.Settings.Pref
                                    let contactPerson = GetStringFromJtoken item "data.procuring_entity.contacts.main.name"
                                    let postAddress = GetStringFromJtoken item "data.procuring_entity.address"
                                    let factAddress = ""
                                    let phone = GetStringFromJtoken item "data.procuring_entity.contacts.main.phones[0]"
                                    let inn = GetStringFromJtoken item "data.procuring_entity.srn"
                                    let email = GetStringFromJtoken item "data.procuring_entity.contacts.main.email"
                                    let cmd5 = new MySqlCommand(addOrganizer, con)
                                    cmd5.Parameters.AddWithValue("@full_name", __.orgName) |> ignore
                                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                                    cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                                    cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                                    cmd5.Parameters.AddWithValue("@contact_phone", phone) |> ignore
                                    cmd5.Parameters.AddWithValue("@inn", inn) |> ignore
                                    cmd5.Parameters.AddWithValue("@contact_email", email) |> ignore
                                    cmd5.ExecuteNonQuery() |> ignore
                                    IdOrg := int cmd5.LastInsertedId
                                    ()
                            let idTender = ref 0
                            let insertTender = String.Format ("INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region, extend_scoring_date = @extend_scoring_date, extend_bidding_date = @extend_bidding_date", S.Settings.Pref)
                            let cmd9 = new MySqlCommand(insertTender, con)
                            cmd9.Prepare()
                            cmd9.Parameters.AddWithValue("@id_xml", __.id) |> ignore
                            cmd9.Parameters.AddWithValue("@purchase_number", __.id) |> ignore
                            cmd9.Parameters.AddWithValue("@doc_publish_date", __.publishDate) |> ignore
                            cmd9.Parameters.AddWithValue("@href", printForm) |> ignore
                            cmd9.Parameters.AddWithValue("@purchase_object_info", __.purObj) |> ignore
                            cmd9.Parameters.AddWithValue("@type_fz", __.typeFz) |> ignore
                            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg) |> ignore
                            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay) |> ignore
                            cmd9.Parameters.AddWithValue("@id_etp", idEtp) |> ignore
                            cmd9.Parameters.AddWithValue("@end_date", __.endDate) |> ignore
                            cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue) |> ignore
                            cmd9.Parameters.AddWithValue("@bidding_date", __.biddingDate) |> ignore
                            cmd9.Parameters.AddWithValue("@cancel", cancelStatus) |> ignore
                            cmd9.Parameters.AddWithValue("@date_version", __.createDate) |> ignore
                            cmd9.Parameters.AddWithValue("@num_version", numVersion) |> ignore
                            cmd9.Parameters.AddWithValue("@notice_version", __.status) |> ignore
                            cmd9.Parameters.AddWithValue("@xml", printForm) |> ignore
                            cmd9.Parameters.AddWithValue("@print_form", printForm) |> ignore
                            cmd9.Parameters.AddWithValue("@extend_scoring_date", tendOrigNum) |> ignore
                            cmd9.Parameters.AddWithValue("@extend_bidding_date", __.descr) |> ignore
                            cmd9.Parameters.AddWithValue("@id_region", 0) |> ignore
                            cmd9.ExecuteNonQuery() |> ignore
                            idTender := int cmd9.LastInsertedId
                            match updated with
                            | true -> AbstractDocument.Upd <- AbstractDocument.Upd + 1
                            | false -> AbstractDocument.Add <- AbstractDocument.Add + 1
                            let documents = item.GetElements("data.documents.tender")
                            __.AddDocs con !idTender documents
                            let idCustomer = ref 0
                            if __.orgName <> "" then
                                let selectCustomer =
                                    sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" S.Settings.Pref
                                let cmd3 = new MySqlCommand(selectCustomer, con)
                                cmd3.Prepare()
                                cmd3.Parameters.AddWithValue("@full_name", __.orgName) |> ignore
                                let reader = cmd3.ExecuteReader()
                                match reader.HasRows with
                                | true ->
                                    reader.Read() |> ignore
                                    idCustomer := reader.GetInt32("id_customer")
                                    reader.Close()
                                | false ->
                                    reader.Close()
                                    let insertCustomer =
                                        sprintf "INSERT INTO %scustomer SET reg_num = @reg_num, full_name = @full_name, inn = @inn"
                                            S.Settings.Pref
                                    let RegNum = Guid.NewGuid().ToString()
                                    let inn = GetStringFromJtoken item "data.procuring_entity.srn"
                                    let cmd14 = new MySqlCommand(insertCustomer, con)
                                    cmd14.Prepare()
                                    cmd14.Parameters.AddWithValue("@reg_num", RegNum) |> ignore
                                    cmd14.Parameters.AddWithValue("@full_name", __.orgName) |> ignore
                                    cmd14.Parameters.AddWithValue("@inn", inn) |> ignore
                                    cmd14.ExecuteNonQuery() |> ignore
                                    idCustomer := int cmd14.LastInsertedId
                            match __.hasLots with
                            | false -> __.AddLots(con, !idTender, item, !idCustomer)
                            | true -> __.AddLotsExtends(con, !idTender, item, !idCustomer)
                            __.AddVerNumber con __.id S.Settings
                            __.TenderKwords con (!idTender) S.Settings
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
      
      member private __.ReturnPageLot(idLot: string): JObject =
          let url = sprintf "%s/remote/api/v2/entity/%s/lot/%s" AbstractParser.EndPoint __.id idLot
          let res = HttpClientPromUa.DownloadSringPromUa(url)
          let j = JObject.Parse(res)
          match GetStringFromJtoken j "status" with
          | "ok" -> j
          | _ -> failwith <| j.ToString()
          
      member private __.AddDocs (con: MySqlConnection) (idTender: int) (items: List<JToken>) =
          for doc in items do
              let docName = GetStringFromJtoken doc "title"
              let url = GetStringFromJtoken doc "url"
              let description = GetStringFromJtoken doc "descr"
              if docName <> "" || url <> "" then
                  let addAttach = sprintf "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description" S.Settings.Pref
                  let cmd5 = new MySqlCommand(addAttach, con)
                  cmd5.Parameters.AddWithValue("@id_tender", idTender) |> ignore
                  cmd5.Parameters.AddWithValue("@file_name", docName) |> ignore
                  cmd5.Parameters.AddWithValue("@url", url) |> ignore
                  cmd5.Parameters.AddWithValue("@description", description) |> ignore
                  cmd5.ExecuteNonQuery() |> ignore
          ()

      member private __.AddLots(con: MySqlConnection, idTender: int, item: JObject, idCustomer: int) =
            let idLot = ref 0
            let insertLot = sprintf "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency" S.Settings.Pref
            let cmd12 = new MySqlCommand(insertLot, con)
            cmd12.Parameters.AddWithValue("@id_tender", idTender) |> ignore
            cmd12.Parameters.AddWithValue("@lot_number", 1) |> ignore
            cmd12.Parameters.AddWithValue("@max_price", __.amount) |> ignore
            cmd12.Parameters.AddWithValue("@currency", __.currency) |> ignore
            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId
            let products = item.GetElements("data.items")
            for p in products do
                let poName1 = GetStringFromJtoken p "primary_classifier.name_with_code"
                let poName2 = GetStringFromJtoken p "descr"
                let poName = match (poName1, poName2) with
                             | (x, "") -> x
                             | (x, y) -> sprintf "%s %s" x y
                let quant = GetDecimalFromJtoken p "quantity"
                let okei = GetStringFromJtoken p "unit_name"
                let okpd2 = GetStringFromJtoken p "primary_classifier.code"
                let insertLotitem = sprintf "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, price = @price, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, okpd2_code = @okpd2_code" S.Settings.Pref
                let cmd19 = new MySqlCommand(insertLotitem, con)
                cmd19.Prepare()
                cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                cmd19.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
                cmd19.Parameters.AddWithValue("@name", poName) |> ignore
                cmd19.Parameters.AddWithValue("@sum", "") |> ignore
                cmd19.Parameters.AddWithValue("@price", "") |> ignore
                cmd19.Parameters.AddWithValue("@quantity_value", quant) |> ignore
                cmd19.Parameters.AddWithValue("@customer_quantity_value", quant) |> ignore
                cmd19.Parameters.AddWithValue("@okei", okei) |> ignore
                cmd19.Parameters.AddWithValue("@okpd2_code", okpd2) |> ignore
                cmd19.ExecuteNonQuery() |> ignore
                let delivPlace1 = GetStringFromJtoken p "delivery_address.locality"
                let delivPlace2 = GetStringFromJtoken p "delivery_address.region"
                let delivPlace3 = GetStringFromJtoken p "delivery_address.country_name"
                let delivPlace4 = GetStringFromJtoken p "delivery_address.street_address"
                let delivPlace = sprintf "%s %s %s %s" delivPlace1 delivPlace2 delivPlace3 delivPlace4
                let delivStart = p.StDDateTimeB "delivery_period.start"
                let delivEnd = p.StDDateTimeB "delivery_period.end"
                let delivTerm = sprintf "Start Delivery: %O End Delivery: %O" delivStart delivEnd
                if delivPlace <> "" || delivTerm <> "" then
                    let insertCustomerRequirement =
                        sprintf "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, application_guarantee_amount = @application_guarantee_amount, contract_guarantee_amount = @contract_guarantee_amount, delivery_term = @delivery_term" S.Settings.Pref
                    let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
                    cmd16.Prepare()
                    cmd16.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                    cmd16.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
                    cmd16.Parameters.AddWithValue("@delivery_place", delivPlace) |> ignore
                    cmd16.Parameters.AddWithValue("@application_guarantee_amount", "") |> ignore
                    cmd16.Parameters.AddWithValue("@contract_guarantee_amount", "") |> ignore
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm) |> ignore
                    cmd16.ExecuteNonQuery() |> ignore
                ()
            ()
      
      member private __.AddLotsExtends(con: MySqlConnection, idTender: int, item: JObject, idCustomer: int) =
          let lots = item.GetElements("data.lots")
          for l in lots do
              let lotId = GetStringFromJtoken l "id"
              try
                  let lo = __.ReturnPageLot(lotId)
                  __.Lot(con, idTender, lo, idCustomer)
                  ()
              with ex -> Log.logger ex
          ()
      
      member private __.Lot(con: MySqlConnection, idTender: int, item: JObject, idCustomer: int) =
          ()