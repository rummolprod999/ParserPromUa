namespace PromUa

module P =

    let parserExec (p : Iparser) = p.Parsing()

    let parserPromUa (dir : string) =
        Logging.Log.logger "Начало парсинга"
        try
            parserExec (ParserPromUa(dir))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger (sprintf "Добавили тендеров %d" AbstractDocument.Add)
        Logging.Log.logger (sprintf "Обновили тендеров %d" AbstractDocument.Upd)
        ()