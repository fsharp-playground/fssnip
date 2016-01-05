open Chiron
open Chiron.Operators

type Suit with
    static member ToJson (suit : Suit) =
        match suit with
        | Punch -> ToJsonDefaults.ToJson "punch"
        | Kick -> ToJsonDefaults.ToJson "kick"
        | Throw -> ToJsonDefaults.ToJson "throw"
        | Defend -> ToJsonDefaults.ToJson "defend"
    static member FromJson (_ : Suit) : Json<Suit> =
        fun json ->
            match json with
            | String "punch" -> Value Punch, json
            | String "kick" -> Value Kick, json
            | String "throw" -> Value Throw, json
            | String "defend" -> Value Defend, json
            | s -> Error (sprintf "Expected suit, found %A" s), json

type Card with
    static member private createMega name speed damage =
        MegaAttack (name, speed, damage)

    static member private createBasic suit value =
        Basic (suit, value)

    static member ToJson (card : Card) =
        match card with
        | KnockDown ->
            Json.write "type" "knockdown"
        | Combo (speed, extra) -> 
            Json.write "type" "combo"
            *> Json.write "speed" speed
            *> Json.write "extra" extra
        | MegaAttack (name, speed, damage) ->
            Json.write "type" "mega"
            *> Json.write "name" name
            *> Json.write "speed" speed
            *> Json.write "damage" damage
        | Basic (suit, v) ->
            Json.write "type" "basic"
            *> Json.write "suit" suit
            *> Json.write "value" v
    static member FromJson (_ : Card) =
        json {
            let! t = Json.read "type"

            return!
                match t with
                | "knockdown" -> Json.init KnockDown
                | "combo" ->
                        fun spd ext -> Combo(spd, ext)
                    <!> (Json.read "speed") <*> (Json.read "extra")
                | "mega" ->
                        fun name spd dam -> MegaAttack (name, spd, dam)
                    <!> (Json.read "name") <*> (Json.read "speed") <*> (Json.read "damage")
                | "basic" -> Card.createBasic <!> (Json.read "suit") <*> (Json.read "value")
                | _ -> Json.error (sprintf "Expected card")
        }

type PlayerName with
    static member ToJson (PlayerName p) =
        ToJsonDefaults.ToJson p
    static member FromJson (_ : PlayerName) : Json<PlayerName> =
        fun json ->
            match json with
            | String s -> Value <| PlayerName s, json
            | x -> Error (sprintf "Expected player name, found %A" x), json

type DeckName with
    static member ToJson (DeckName d) =
        ToJsonDefaults.ToJson d
    static member FromJson (_ : DeckName) =
        fun json ->
            match json with
            | String s -> Value <| DeckName s, json
            | x -> Error (sprintf "Expected deck name, found %A" x), json
