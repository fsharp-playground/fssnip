(* we store our instructions in the binary tree for faster search *)
type 'a Tree when 'a : comparison = 
     | Nil 
     | Node of 'a * ('a Tree) * ('a Tree)

let Leaf x = Node(x, Nil, Nil)

type State = int
type Base = char
(* tape is represented by a zipper *)
type Ribbon = (Base list)*(Base list) 
type OP = L | R | U | H
type Instr = State * Base * Base * State * OP
type Prog = Instr Tree
type TM = Prog * State * Ribbon


let insert x T =
    let rec insert' x T cont = 
        match T with
        | Nil           -> cont (Leaf x)
        | Node(a, l, r) ->
          if (x > a) then
              insert' x r (fun e -> cont <| Node(a, l, e))
          else
              insert' x l (fun e -> cont <| Node(a, e, r))
    insert' x T id


let flip f x y = f y x
let curry f x y = f (x, y)
let uncurry f (x, y) = f x y

(* creating a tape structure from a string *)
let mkrib (s : string) = ([' '], s.ToCharArray() |> List.ofArray)

(* equivalent to List.append << List.rev *)
let rec rev_append l1 l2  =
    match l1 with
    | []   -> l2
    | h::t -> rev_append t (h::l2)
    
(* move the head according to the operation *)
let proc rib op =
    match op,rib with
    | U,_         -> rib
    | L,([],r)    -> ([], ' '::r)
    | L,(h::t, r) -> (t, h::r)
    | R,(l,h::[]) -> (h::l, [' '])
    | R,(l, h::t) -> (h::l, t)
    | H,(l, r)    -> ([],rev_append l r)

(* Find an instruction in the instructions tree *)
let rec find fSt fSym = function
    | Nil -> failwith "Not found"
    | Node((st,sym,nsym,nst,op), l, r) ->
        if (fSt,fSym) = (st,sym) then (st,sym,nsym,nst,op)
        elif (fSt,fSym) <= (st,sym) then find fSt fSym l
        else find fSt fSym r

let findInstr st sym (P : Prog) = find st sym P

(* simulate a step of the machine *)
let step ((P, st, (ribL, sym::ribR)) : TM) =
  let (_,_,nsym,nst,op) = findInstr st sym P
  let newrib = proc (ribL, nsym::ribR) op
  (P, nst, newrib)


(* Print the tape *)
let print (P, st, (ribL,ribR)) =
    for c in (List.rev ribL) do printf "%c " c
    printf "[%c] " <| List.head ribR
    for c in (List.tail ribR) do printf "%c " c
    printf "\n"


let rec run ((P, st, rib) : TM) =
    match rib with
    (* if the left part of the ribbon is nil, then halt *)
    | ([],_) -> (P, st, rib)
    | (l, r) ->
        print (P, st, rib)
        let n = step (P,st,rib)
        run n

(* example machine: binary addition *)

let mtadd = 
    (
    [(0,'0','0',0,R);
    (0,'1','1',0,R);
    (0,' ',' ',1,R);
    (1,'0','0',1,R);
    (1,'1','1',1,R);
    (1,' ',' ',2,L);
    (2,'1','0',3,L);
    (2,'0','1',2,L);
    (2,' ',' ',5,R);
    (3,'1','1',3,L);
    (3,'0','0',3,L);
    (3,' ',' ',4,L);
    (4,'1','0',4,L);
    (4,'0','1',0,R);
    (4,' ',' ',0,R);
    (5,'1',' ',5,R);
    (5,' ',' ',5,H)] |> List.fold (flip insert) Nil,
    0, mkrib "001110 101")

run mtadd
