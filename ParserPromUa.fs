namespace PromUa

type ParserPromUa(dir : string) =
      inherit AbstractParser()
      interface Iparser with

            override __.Parsing() =
                  __.Login()
                  