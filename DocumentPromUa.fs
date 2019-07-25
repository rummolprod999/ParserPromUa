namespace PromUa
open System

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
      inherit AbstractDocument()
      interface IDocument with
        override __.Worker() =
            __.WorkerEntity()

      new(id, hasLots, status, publishDate, endDate, createDate, biddingDate, amount, descr, purObj, orgName, currency) as this = DocumentPromUa()
                                                                                                                                  then this.id <- id
                                                                                                                                       this.hasLots <- hasLots
                                                                                                                                       this.status <- status
                                                                                                                                       this.publishDate <- publishDate
                                                                                                                                       this.endDate <- endDate
                                                                                                                                       this.createDate <- createDate
                                                                                                                                       this.biddingDate <- biddingDate
                                                                                                                                       this.amount <- amount
                                                                                                                                       this.descr <- descr
                                                                                                                                       this.purObj <- purObj
                                                                                                                                       this.orgName <- orgName
                                                                                                                                       this.currency <- currency
      member __.WorkerEntity() = ()
