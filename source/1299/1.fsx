namespace FSharp.Fuzzy

open System

type Interval = 
    {
        a:decimal
        b:decimal 
    }
    member this.Middle = (this.a+this.b)/2m
    static member operation f (x : Interval, y : Interval) = 
        let list = [f x.a y.a; f x.a y.b; f x.b y.a; f x.b y.b]
        let min (xs : decimal seq) = 
            Seq.fold (fun (acc : decimal) x -> Math.Min(acc,x)) Decimal.MaxValue xs
        let max (xs : decimal seq) = Seq.fold (fun (acc : decimal) x -> Math.Max(acc,x)) Decimal.MinValue xs
        { a =  min list; b = max list}
    static member (*) (x : Interval, y : Interval) = Interval.operation (fun x y-> x*y) (x,y)
    static member zeroLength (x : decimal) = {a=x; b=x}
    static member Zero =  Interval.zeroLength 0m
    static member (/) (x : Interval, y : Interval) = 
        if y.a < 0m && y.b > 0m then failwith "Divider cannot contain zero."
        Interval.operation (fun x y-> x/y) (x,y)
    static member (+) (x : Interval, y : Interval) = { a = x.a + y.a; b = x.b + y.b}
    static member (-) (x : Interval, y : Interval) = { a = x.a - y.b; b = x.b - y.a}
    static member (*) (x : Interval, y : decimal) = x * Interval.zeroLength(y)
    static member (/) (x : Interval, y : decimal) = x / Interval.zeroLength(y)
    static member (+) (x : Interval, y : decimal) = x + Interval.zeroLength(y)
    static member (-) (x : Interval, y : decimal) = x - Interval.zeroLength(y)
    static member (*) (x : decimal, y : Interval) = Interval.zeroLength(x) * y
    static member (/) (x : decimal, y : Interval) = Interval.zeroLength(x) / y
    static member (+) (x : decimal, y : Interval) = Interval.zeroLength(x) + y
    static member (-) (x : decimal, y : Interval) = Interval.zeroLength(x) - y
    static member pow (x : Interval, p : double) = { a =  double x.a ** p |> decimal; b = double x.b ** p |> decimal }
    static member distance (x : Interval, y : Interval) = Math.Abs(y.Middle - x.Middle)

[<StructuredFormatDisplayAttribute("{alphaCuts}")>]
type Fuzzy(a : Interval seq) = 
    let alphas = a |> Array.ofSeq

    member this.alphaCuts with get() = alphas |> Array.ofSeq
    member this.Bottom with get() = alphas.[0]
    member this.Top with get() = alphas.[alphas.Length - 1]
    
    override this.ToString() = sprintf "%A" alphas

    override x.Equals(yobj) =
        match yobj with
        | :? Fuzzy as y -> x.alphaCuts = y.alphaCuts
        | _ -> false
 
    override x.GetHashCode() = hash x.alphaCuts
    
    interface System.IComparable with
      member x.CompareTo yobj =
          match yobj with
          | :? Fuzzy as y -> compare x.alphaCuts y.alphaCuts
          | _ -> invalidArg "yobj" "cannot compare values of different types"

    static member Zero =  Fuzzy(Array.create 11 Interval.Zero)
    
    static member operation f (a:Fuzzy) (b:Fuzzy) =  Fuzzy(Seq.map2 f a.alphaCuts b.alphaCuts )
    static member map f (a:Fuzzy) =  Fuzzy(Seq.map f a.alphaCuts )
    
    static member (*) (x : Fuzzy, y : Fuzzy) = Fuzzy.operation (fun a b-> a*b) x y
    static member (/) (x : Fuzzy, y : Fuzzy) = Fuzzy.operation (fun a b-> a/b) x y
    static member (+) (x : Fuzzy, y : Fuzzy) = Fuzzy.operation (fun a b-> a+b) x y
    static member (-) (x : Fuzzy, y : Fuzzy) = Fuzzy.operation (fun a b-> a-b) x y
    static member (*) (x : Fuzzy, y : decimal) = Fuzzy.map (fun a-> a*y) x
    static member (/) (x : Fuzzy, y : decimal) = Fuzzy.map (fun a-> a/y) x
    static member (+) (x : Fuzzy, y : decimal) = Fuzzy.map (fun a-> a+y) x
    static member (-) (x : Fuzzy, y : decimal) = Fuzzy.map (fun a-> a-y) x
    static member (*) (x : decimal, y : Fuzzy) = y * x
    static member (/) (x : decimal, y : Fuzzy) = Fuzzy.map (fun a-> x/a) y
    static member (+) (x : decimal, y : Fuzzy) = y + x
    static member (-) (x : decimal, y : Fuzzy) = Fuzzy.map (fun a-> x-a) y
    static member pow (x : Fuzzy, p : double) = Fuzzy.map (fun a-> Interval.pow(a, p)) x

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<AutoOpen>]
module Fuzzy =
    let alpha total level = if level = 0 then 0m else 1.m / decimal (total - 1) * decimal level

    let trapezoid levels (a,b,c,d) = 
        if a>b || b>c || c>d then failwith "expected a>=b>=c>=d"
        let maxIndex = levels - 1
        let step = 1.m / (decimal maxIndex)
        Fuzzy(seq { for i in 0..maxIndex -> { a = a+(b-a)*step*decimal i; b = c+(d-c)*step*decimal (maxIndex-i) } } )    
   
    let interval(a,b,c,d) = trapezoid 11 (a,b,c,d)
    let number(a,b,c) = interval(a,b,b,c)
    let point(a) = number(a,a,a)   
    
    let binary f (a: Fuzzy) (b: Fuzzy) = 
        assert (a.alphaCuts.Length = b.alphaCuts.Length)
        let length = a.alphaCuts.Length
        let result = 
            Seq.zip a.alphaCuts b.alphaCuts 
            |> Seq.mapi (fun i pair -> alpha a.alphaCuts.Length i * f pair ) 
            |> Seq.sum
        result / (decimal a.alphaCuts.Length / 2m)

    let unary f (a: Fuzzy) = 
        let result = 
            a.alphaCuts 
            |> Seq.mapi (fun i b -> alpha a.alphaCuts.Length i * f b ) 
            |> Seq.sum
        result / (decimal a.alphaCuts.Length / 2m)
    
    let distance a b = binary Interval.distance a b
    let width a = unary (fun i->i.b - i.a) a
    let risk a = unary (fun i->2m * (i.b - i.a)/(i.a+i.b)) a

    let plot (a : Fuzzy) = 
        let length =  a.alphaCuts.Length - 1
        seq { for i in 0..length -> a.alphaCuts.[i].a, alpha a.alphaCuts.Length i 
              for i in length .. -1 .. 0 -> a.alphaCuts.[i].b, alpha a.alphaCuts.Length i  } |> Array.ofSeq
