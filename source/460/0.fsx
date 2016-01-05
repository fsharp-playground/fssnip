type IA = 
  abstract Action : unit -> unit

type Type = 
  | TypeA 
  | TypeB

let factory = function
  | TypeA -> { new IA with 
                   member this.Action() = printfn "type A" }
  | TypeB -> { new IA with 
                  member this.Action() = printfn "type B" }
