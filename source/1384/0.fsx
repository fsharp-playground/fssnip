let echo x = "You have to work " + x.ToString() + " hours."

let ``working hours`` =
    let day = (int System.DateTime.Now.DayOfWeek)
    match day with
    | 0 | 6 -> None
    | _ -> Some(7.5)

let echoHours1 = 
    match ``working hours`` with
    | None -> ""
    | Some h -> echo h

let echoHours2 = 
    ``working hours`` 
    |> Option.map(fun h -> echo h)

let echoHours3 = 
    ``working hours`` 
    |> Option.map(echo)
