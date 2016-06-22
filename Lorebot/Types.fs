module Types

open System.Collections.Generic
open System

type Self = {
    id : string
    name : string
    prefs : Dictionary<Object, Object>
    created : string
}

type Team = {
    id : string
    name : string
    email_domain : string
    domain : string
    icon : Dictionary<Object, Object>
    msg_edit_window_mins : int
    over_storage_limit : bool
    prefs : Dictionary<Object, Object>
    plan : string
}

type User = {
    id : string
    name : string
}

type Channel = {
    id : string
    name : string
}


type SlackBotResponse = {
    ok : bool
    url : string
    users : User list
    channels : Channel list
}

type Webhook = {
    url : string
    channel : string
    configuration_url : string
}

type Bot = {
    bot_user_id : string
    bot_access_token : string
}

type SlackAuthResponse = {
    access_token : string
    scope : string
    team_name : string
    team_id : string
    incoming_webhook : Webhook
    bot : Bot
}

type SlackAuthRequest = {
    token : string
    no_unreads : bool
}

type SlackMessage = {
    text : string
    channel : string
    user : string
    ``type`` : string
}