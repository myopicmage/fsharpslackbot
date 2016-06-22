open System.Net.Http
open WebSocketSharp
open System
open Types
open Newtonsoft.Json
open System.Text
open System.Linq

let messageParse arg (users : User list) (ws : WebSocket) = 
    let message = JsonConvert.DeserializeObject<SlackMessage>(arg)
    
    let getUser user = 
        let item = users.Single(fun x -> x.id = user)
        item.name

    match message.``type`` with
    | "message" -> 
        match message.text.StartsWith("@myopicm") with
        | true -> printfn "%A WANTS YOUR ATTENTION: %A" (getUser message.user) message.text
        | false -> printfn "%A: %A" (getUser message.user) message.text
    | "user_typing" -> ()
    | "channel_marked" -> ()
    | "presence_change" -> ()
    | "reconnect_url" -> ()
    | _ -> printfn "%A: %A" message.user message.``type``

[<EntryPoint>]
let main argv = 
    use client = new HttpClient()
    client.BaseAddress <- Uri("https://slack.com/")
    let api = "api/rtm.start?token=nooooooope&no_unreads=true&pretty=1"
    let responseTask = client.PostAsync(api, new StringContent(""))
    responseTask.Wait()

    let response = responseTask.Result
    let r = response.Content.ReadAsStringAsync()
    r.Wait()

    let sr = JsonConvert.DeserializeObject<SlackBotResponse>(r.Result)

    use ws = new WebSocket(sr.url)
    ws.Connect()
    ws.OnMessage.Add(fun x -> (messageParse x.Data sr.users ws))

    Console.ReadKey(true) |> ignore
    0
