namespace PromUa

open Logging
open System
open System.IO
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks

module Download =
    
    exception EmptyString of string
    
    type TimedWebClientPromUa() =
        inherit WebClient()
        override this.GetWebRequest(address: Uri) =
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 60000
            wr.Headers.Add("Api-Agent", "zk_integration")
            wr.Headers.Add("Content-Type", "application/json")
            wr :> WebRequest

    type HttpClientPromUa(url: string, data: string, typereq: string) =
         member public __.ReturnString(): string =
            use client = new HttpClient()
            client.Timeout <- new TimeSpan(0, 0, 60)
            let request = new HttpRequestMessage()
            request.RequestUri <- new Uri(url)
            match typereq with
            | "post" -> request.Method <- HttpMethod.Post
            | _ -> request.Method <- HttpMethod.Get
            
            request.Headers.Add("Api-Agent", "zk_integration")
            if S.Token <> "" then
                request.Headers.Add("Acc-Token", S.Token)
            if data <> "" then
                request.Content <- new StringContent(data, Encoding.UTF8, "application/json")
            let task = client.SendAsync(request).Result
            if task.StatusCode = HttpStatusCode.NotFound then failwith "404"
            let res = task.Content.ReadAsStringAsync().Result
            if res = "" then raise (EmptyString(sprintf "empty string at url %s" url))
            res

         static member DownloadSringPromUa(url: string, ?postdata: string, ?typereq: string) =
             let dt = defaultArg postdata ""
             let pst = defaultArg typereq "get"
             let mutable s = null
             let count = ref 0
             let mutable continueLooping = true
             while continueLooping do
                 try
                     let task = Task.Run(fun () -> (new HttpClientPromUa(url, dt, pst)).ReturnString())
                     if task.Wait(TimeSpan.FromSeconds(100.)) then
                         s <- task.Result
                         continueLooping <- false
                     else raise <| new TimeoutException()
                 with e ->
                     //Console.WriteLine(e)
                     if !count >= 2 then
                         Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url (!count + 1))
                         Log.logger e
                         continueLooping <- false
                     else incr count
                     Thread.Sleep(5000)
             s

    let DownloadStringPromUa url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true
        while continueLooping do
            try
                let task = Task.Run(fun () -> (new TimedWebClientPromUa()).DownloadString(url: string))
                if task.Wait(TimeSpan.FromSeconds(100.)) then
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with _ ->
                if !count >= 3 then
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s

