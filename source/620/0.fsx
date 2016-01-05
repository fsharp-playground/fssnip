// Learn more about F# at http://fsharprogram.net
#light
module BrainFuck
  open System
  open System.Collections.Generic 

  exception UnmatchedBrace of string

  let bf (program:string) (input: unit->char ) (output : char->unit) =
    if program.Length=0 then ()
    let mutable jmps = new Map<int,int>([])
    let mutable  stk = new Stack<char * int>()
    for i = 0 to (program.Length-1) do
      let c = program.[i]
      match c with
       | '[' -> stk.Push((c, i))
       | ']' -> if stk.Count>0 then
                  let (sym,pos)=stk.Peek() 
                  if sym ='[' then
                    jmps <-  jmps.Add(pos , i).Add(i,pos) 
                    ignore stk.Pop
                  else raise(UnmatchedBrace(String.Format(" at pos: {0} ", i)))
                else raise(UnmatchedBrace(String.Format(" at pos: {0} ", i)))
       | _ -> ()
    let mptr = ref 0
    let exit = ref true 
    let   ok = fun(pc)-> !exit && (pc>=0 && pc<program.Length)
    let mem : int[] = Array.zeroCreate 1000
    let rec bfi pc (j:Map<int,int>) =
      if ok pc then 
        match program.[pc] with
          | '[' -> if mem.[!mptr] = 0 then bfi j.[pc] j
          | ']' -> if mem.[!mptr] <> 0 then bfi j.[pc] j
          | '+' -> mem.[!mptr]<-(mem.[!mptr] + 1)
          | '-' -> mem.[!mptr]<-(mem.[!mptr] - 1)
          | '<' -> mptr := !mptr - 1
          | '>' -> mptr := !mptr + 1 
          | '.' -> output(char(mem.[!mptr]))
          | '#' -> mem.[!mptr] <- int( input()) 
          | _ -> ()
        if ok pc then bfi  (pc+1) j
        exit:= false     
    bfi 0 jmps


  let Main() =
    let bfcode = "++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>."
    bf  bfcode (fun ()  -> (char)( Console.Read()) ) (fun(x)->Console.Write((char)x)) 
    let x= Console.ReadKey(false)
    ()

  Main()