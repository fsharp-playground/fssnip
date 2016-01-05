open System
open System.Threading 
open System.Collections.Generic 
open Microsoft.FSharp.Core.Operators.Unchecked 

// [snippet:Memoize Sample]
let createDic (key:'a) (value:'b) = Dictionary<'a, 'b> ()
let collateArg (arg: 'TArg) (f : 'TArg -> 'TResult) = fun a -> f a

[<CompiledName("Memoize")>]
let memoize0 f = 
  let value = ref defaultof<'TResult>
  let hasValue = ref false
  fun () -> if not !hasValue then  hasValue := true
                                   value := f ()
            !value

[<CompiledName("Memoize")>]
let memoize1 f = 
  let dic = createDic  defaultof<'TArg1> defaultof<'TResult>
  fun x -> match dic.TryGetValue(x) with 
           | true, r -> r
           | _       -> dic.[x] <- f x
                        dic.[x]

type args<'TArg1,'TArg2> = {item1:'TArg1; item2:'TArg2}
[<CompiledName("Memoize")>]
let memoize2 (f : 'TArg1 -> 'TArg2 -> 'TResult) = 
  let f' = collateArg { item1 = defaultof<'TArg1>
                        item2 = defaultof<'TArg2> } (fun a -> f a.item1 a.item2) |> memoize1
  fun a b -> f' { item1 = a 
                  item2 = b}


type args<'TArg1,'TArg2,'TArg3> ={item1:'TArg1; item2:'TArg2; item3:'TArg3}
[<CompiledName("Memoize")>]
let memoize3 (f : 'TArg1 -> 'TArg2 -> 'TArg3 -> 'TResult) = 
  let f' = collateArg { item1 = defaultof<'TArg1>
                        item2 = defaultof<'TArg2>
                        item3 = defaultof<'TArg3> } (fun a -> f a.item1 a.item2 a.item3) |> memoize1
  fun a b c -> f' { item1 = a 
                    item2 = b
                    item3 = c}
// [/snippet]

// [snippet:Memoize Tail Recursion Sample]
[<CompiledName("MemoizeTailRecursion")>]
let memoizeTailRecursion f =
  let dic = createDic  defaultof<'TArg1> defaultof<'TResult>
  let rec f' a k = 
    match dic.TryGetValue(a) with
    | true, r -> k r
    | _ ->  f a (fun r -> dic.[a] <- r
                          k r) f'
  (fun a -> f' a id)

[<CompiledName("MemoizeTailRecursion")>]
let memoizeTailRecursion2 f =
  let dic = createDic (defaultof<'TArg1> 
                     , defaultof<'TArg2>) defaultof<'TResult>
  let rec f' a b k = 
    match dic.TryGetValue((a,b)) with
    | true, r -> k r
    | _ ->  f a (fun r -> dic.[(a,b)] <- r
                          k r) f'
  (fun a b -> f' a b id)

[<CompiledName("MemoizeTailRecursion")>]
let memoizeTailRecursion3 f =
  let dic = createDic (defaultof<'TArg1> 
                     , defaultof<'TArg2>
                     , defaultof<'TArg3>) defaultof<'TResult>
  let rec f' a b c k = 
    match dic.TryGetValue((a,b,c)) with
    | true, r -> k r
    | _ ->  f a (fun r -> dic.[(a,b,c)] <- r
                          k r) f'
  (fun a b c -> f' a b c id)
// [/snippet]

// [snippet:Main]
let fibtrc n k m =
    if n = 0 then k 1
    else m (n - 1) (fun r1 -> let r = r1 * n in k r) 

let Heviy0 () =
  Thread.Sleep 3000
  1

let Heviy i = 
  Thread.Sleep 1000
  i + 1

let Heviy2 i j = 
  Thread.Sleep 1000
  i + j + 1

let Heviy3 i j k = 
  Thread.Sleep 1000
  i + j + k + 1
 
let Main =
  printfn "%s" "memoize0"
  let memofunc0 = memoize0 (fun () -> Heviy0 ())
  for i=0 to 4 do Console.WriteLine(memofunc0 ())
  for i=0 to 4 do Console.WriteLine(memofunc0 ())
  printfn "%s" "memoize1"
  let memofunc1 = memoize1 (fun x -> Heviy x)
  for i=0 to 4 do Console.WriteLine(memofunc1 i)
  for i=0 to 4 do Console.WriteLine(memofunc1 i)
  printfn "%s" "memoize2"
  let memofunc2 = memoize2 (fun a b -> Heviy2 a b)
  for i=0 to 4 do Console.WriteLine(memofunc2 i i)
  for i=0 to 4 do Console.WriteLine(memofunc2 i i)
  printfn "%s" "memoize3"
  let memofunc3 = memoize3 (fun a b c -> Heviy3 a b c)
  for i=0 to 4 do Console.WriteLine(memofunc3 i i i)
  for i=0 to 4 do Console.WriteLine(memofunc3 i i i)

let fibtrcmem = memoizeTailRecursion fibtrc
fibtrcmem 5 |> printfn "%d" 

Console.WriteLine ()
  |> fun _ -> Console.ReadLine () |> ignore
// [/snippet]


type Y<'T> = Rec of (Y<'T> -> 'T)
// Y Combinator
let y mk =
  let f' (Rec f as g) = mk (fun y -> f g y)
  f' (Rec f')
  
// Memoized Y Combinator... No.
let yMem mk = 
  let dic = createDic defaultof<'a> defaultof<'b>
  let f' (Rec f as g) = mk (fun y -> f g y)
  let f'' = f' (Rec f')
  fun x -> if dic.ContainsKey(x) then
             dic.[x]
           else
             let answer = f'' x
             dic.[x] <- answer
             answer
