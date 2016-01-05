type Token = END
type Status = ACCEPT | ERROR

type ParseState = Shifting | Reducing

let parse gettoken shift reduce () =
    let rec read() =
        let tok = gettoken()

        if tok = END then
            ACCEPT
        else
            update tok Shifting

    and update tok state = 
        match state with
        | Shifting ->
            if shift tok then
                read()
            else
                update tok Reducing

        | Reducing ->
            if reduce tok then
                update tok Shifting
            else
                ERROR
    
    read()
