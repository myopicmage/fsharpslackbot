open System.Net.Http
open WebSocketSharp
open System
open Types
open Newtonsoft.Json
open System.Text
open System.Linq

let mutable messageId = 1

let messageParse (arg : MessageEventArgs) (users : User list) (ws : WebSocket) (channels : Channel list) = 
    let message = JsonConvert.DeserializeObject<SlackMessage>(arg.Data) |> makeOpt

    let sendMessage toSend =
        ws.Send(JsonConvert.SerializeObject(toSend))
        messageId <- messageId + 1

    let getUser user = 
        let item = users.SingleOrDefault(fun x -> x.id = user) |> makeOpt

        match item with
        | Some(item) -> item.name
        | None -> user

    let getChannel channel =
        let item = channels.SingleOrDefault(fun x -> x.id = channel) |> makeOpt

        match item with
        | Some(item) -> item.name
        | None -> channel

    let handleMessage (m : SlackMessage) =
        let mOpt = m.text |> makeOpt

        match mOpt with
        | Some(x) -> printfn "[%A] %A: %A" (getChannel m.channel) (getUser m.user) x
        | None -> printfn "[System] %A" m

    let handleReaction m =
        let user = getUser m.user

        printfn "[System] %A added a reaction" user
    
    let badMessage (m : MessageEventArgs) = 
        match m.IsPing with
        | true -> 
            sendMessage { ``type`` = "ping"; id = messageId; time = DateTime.Now } 
            printfn "[System] Ping?"
        | false -> printfn "[Error] %A" m

    let goodMessage (m : SlackMessage) = 
        match m.``type`` with
        | "hello" -> printfn "[System] Hello, you're now connected."
        | "pong" -> printfn "[System] Pong!"
        | "message" -> handleMessage m
        | "user_typing" -> ()
        | "channel_marked" -> ()
        | "presence_change" -> ()
        | "reconnect_url" -> ()
        | "reaction_added" -> handleReaction m
        | "pref_change" -> printfn "[System] Your preference has been changed: %A to %A" m.name m.value
        | _ -> printfn "%A" arg

    match message with
    | None -> badMessage arg
    | Some(m) -> goodMessage m

let runExit (ws : WebSocket) =
    ws.Close()

    Environment.Exit(0)

let awaitMessage (ws : WebSocket) (channels : Channel list) (users : User list) =
    let getChannel channel =
        let item = channels.SingleOrDefault(fun x -> x.name = channel) |> makeOpt

        match item with
        | Some(item) -> item.id
        | None -> channel

    let command = Console.ReadLine()

    match command with
    | "exit" -> runExit ws
    | _ ->
        let channel = getChannel "programming"
        JsonConvert.SerializeObject({ Msg.``type`` = "message"; Msg.text = command; Msg.id = messageId; Msg.channel = channel;}) |> ws.Send
        messageId <- messageId + 1

[<EntryPoint>]
let main argv = 
    use client = new HttpClient()
    client.BaseAddress <- Uri("https://slack.com/")
    let api = ""
    let responseTask = client.PostAsync(api, new StringContent(""))
    responseTask.Wait()

    let response = responseTask.Result
    let r = response.Content.ReadAsStringAsync()
    r.Wait()

    let sr = JsonConvert.DeserializeObject<SlackBotResponse>(r.Result)

    use ws = new WebSocket(sr.url)
    ws.Connect()
    ws.EmitOnPing <- true
    ws.OnMessage.Add(fun x -> (messageParse x sr.users ws sr.channels))

    while true do
        awaitMessage ws sr.channels sr.users

    0