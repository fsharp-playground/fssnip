let inline parse< ^T when ^T : (static member TryParse : string * byref< ^T > -> bool) and  ^T : (new : unit -> ^T) > valueToParse =
    let mutable output = new ^T()
    let parsed = ( ^T : (static member TryParse  : string * byref< ^T > -> bool ) (valueToParse, &output) )
    match parsed with
    | true -> output |> Some
    | _ -> None