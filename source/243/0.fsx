(*

irc.freenode.org, channel ##fsharp

<LionMadeOfLions> scientists are baffled, though, it will be decades before we
                  begin to understand the lions made of lions

<ninegrid> challenge accepted
*)


type LionMadeOfLions = Lions | Lion of LionMadeOfLions * LionMadeOfLions
let lionMadeOfLions = Lion(Lion(Lion(Lions,Lions),Lions),Lions)

(*
           Lion
          /    \
         Lion  Lions
        /    \
      Lion  Lions
      /   \
    Lions Lions

*)

let rec y f x = f (y f) x
let understand x = printfn "%A" x

let lions (lions : LionMadeOfLions -> LionMadeOfLions) = function
  | Lion (x,y) -> lions x
  | Lions      -> Lions

y (lions >> fun f lion -> understand lion; f lion) lionMadeOfLions

(* output:

> 
Lion (Lion (Lion (Lions,Lions),Lions),Lions)
Lion (Lion (Lions,Lions),Lions)
Lion (Lions,Lions)
Lions
val it : LionMadeOfLions = Lions

*)
  