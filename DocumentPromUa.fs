namespace PromUa

type DocumentPcontr223() =
      
     
      inherit AbstractDocument()
      interface IDocument with
        override __.Worker() =
            __.WorkerEntity()
      
      member __.WorkerEntity() = ()