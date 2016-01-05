// This snippet is based on fsharp-typeclasses
// http://code.google.com/p/fsharp-typeclasses/

// [snippet:Monad]
type Return() = 
    static member (?) (_:Return, _:'a option) = fun (x:'a) -> Some x
    static member (?) (_:Return, _:'a list)   = fun (x:'a) -> [x]
let inline return' x : ^R = Return() ? (Unchecked.defaultof< ^R>) x


type Bind() = 
    static member (?) (_:Bind, m:option<_>) = fun f -> Option.bind  f m
    static member (?) (_:Bind, m:list<_>  ) = fun f -> List.collect f m
let inline (>>=) m f : ^R = Bind() ? (m) f


type DoNotationBuilder() =
    member inline b.Return(x)    = return' x
    member inline b.Bind(p,rest) = p >>= rest
let do' = new DoNotationBuilder()


[4;5;6] >>= (fun x -> [x;x*10]) |> printfn "%A"
// [4; 40; 5; 50; 6; 60]
Some 7 >>= (fun x -> List.tryFind((=)x) [7;8;9]) |> printfn "%A"
// Some 7


do' {
  let! greeting = ["Good morning";"Good evening"]
  let! name = ["Alice";"Bob"]
  return printfn "%s, %s." greeting name
}
// Good morning, Alice.
// Good morning, Bob.
// Good evening, Alice.
// Good evening, Bob.


do' {
  let! id = Some "Carol123"
  let! password = Some "*****"
  return printfn "Hi, %s" id
}
// Hi, Carol123
//[/snippet]

//[snippet:Applicative]
type Fmap() = 
  static member (?) (_:Fmap , m:_ option) = fun f -> Option.map f m
  static member (?) (_:Fmap , m:_ list)   = fun f -> List.map    f m
let inline (<%>) f x = Fmap() ? (x) f


type Ap() = 
  static member (?) (_:Ap , mf:_ option) = fun (m:_ option) -> 
    match mf , m with Some f , Some x -> Some (f x) | _ -> None
  static member (?) (_:Ap , mf:_ list)   = fun (m:_ list) -> 
    [ for f in mf do for x in m -> f x]
let inline (<*>) mf m = Ap() ? (mf) m


(+) <%> ["a";"b"] <*> ["x";"y";"z"]  |> printfn "%A"
// ["ax"; "ay"; "az"; "bx"; "by"; "bz"]

(fun a b c -> a + b + c) <%> [100;200] <*> [10;20] <*> [1;2] |> printfn "%A"
// [111; 112; 121; 122; 211; 212; 221; 222]

let a = (printf "%s, %s") <%> Some "hello" <*> Some "applicative"
// hello, applicative
//[/snippet]