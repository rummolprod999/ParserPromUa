namespace PromUa

open System
open System.IO
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks

module Download =
    type TimedWebClientPromUa() =
        inherit WebClient()
        override this.GetWebRequest(address: Uri) =
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 60000
            wr.Headers.Add("Api-Agent", "zk_integration")
            wr.Headers.Add("Content-Type", "application/json")
            wr :> WebRequest
            
    type HttpClientPromUa(url: string, data: string)  =
         member public __.ReturnString() : string =
            use client = new HttpClient()
            client.Timeout <- new TimeSpan(0, 0, 60)
            let request = new HttpRequestMessage()
            request.RequestUri <- new Uri(url)
            request.Method <- HttpMethod.Post
            request.Headers.Add("Api-Agent", "zk_integration")
            request.Content <- new StringContent(data, Encoding.UTF8, "application/json")
            let task = client.SendAsync(request).Result
            let res = task.Content.ReadAsStringAsync().Result
            res
            
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
    
    let DownloadSringPromUa url postdata =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true
        while continueLooping do
            try
                let task = Task.Run(fun () -> (new HttpClientPromUa(url, postdata)).ReturnString())
                if task.Wait(TimeSpan.FromSeconds(100.)) then
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with e ->
                Console.WriteLine(e)
                if !count >= 3 then
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url (!count+1))
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s