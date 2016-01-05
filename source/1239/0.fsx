let str o = o.ToString ()

type Propagation<'s, 'c> =
    | Propagation of 's * 'c list
    override this.ToString () =
        match this with Propagation (state, changes) -> "State: " + str state + " Changes: " + List.joinBy str " " changes

let propagate state =
    Propagation (state, [])

let inline ( >>. ) propagation (setter, recorder) =
    match propagation with
    | Propagation (state, recordings) ->
        let newState = setter state
        Propagation (newState, recorder newState :: recordings)

let inline ( >. ) propagation setter =
    ( >>. ) propagation (setter, id)
;;


type Propagation<'s,'c> =
  | Propagation of 's * 'c list
  with
    override ToString : unit -> string
  end
val propagate : state:'a -> Propagation<'a,'b>
val inline ( >>. ) : propagation:Propagation<'a,'b> -> setter:('a -> 'c) * recorder:('c -> 'b) -> Propagation<'c,'b>
val inline ( >. ) : propagation:Propagation<'a,'b> -> setter:('a -> 'b) -> Propagation<'b,'b>