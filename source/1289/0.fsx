open System

let rand = Random()

let newOrderId () =
    rand.Next(1000000000, Int32.MaxValue)

let genPhoneNumber () =
    seq {
        yield "+"
        for i in 1 .. (rand.Next(6,10)) ->
             string <| rand.Next(10)
        } |> String.concat ""

let genDate () =
    DateTime.Now - TimeSpan(rand.Next(365), rand.Next(24), rand.Next(60), rand.Next(60))

let pickSample list =
    let rec inner soFar remainingList current =
        match remainingList with
        | [] ->
            current
        | h::t ->
            let next = if rand.Next(soFar + 1) > (soFar - 1) then h else current
            inner (soFar + 1) t next
    let h::t = list
    inner 1 t h