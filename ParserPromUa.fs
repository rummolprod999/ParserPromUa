namespace PromUa
open System

type ParserPromUa(dir : string) =
      inherit AbstractParser()
      interface Iparser with

            override __.Parsing() =
                  __.Login()
                  match dir with
                  | "daily" -> __.GetTendersList()
                  | "last" | _ -> ()

      
      member private __.GetTendersList() =
          let dateNow = String.Format("{0:dd.MM.yyyy}", DateTime.Now)
          let dateNowMinus3Days = String.Format("{0:dd.MM.yyyy}", DateTime.Now.AddDays(-3.))
          let url = String.Format("{0}{1}", AbstractParser.EndPoint, "/remote/api/v3/search")
          let data = ""
          ()