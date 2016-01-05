open System

let min (xs : double list) = Seq.fold (fun (acc : double) x -> Math.Min(acc,x)) Double.MaxValue xs
let max (xs : double list) = Seq.fold (fun (acc : double) x -> Math.Max(acc,x)) Double.MinValue xs

type Interval = 
    { a:double; b:double }
    static member (*) (x : Interval, y : Interval) = 
        let list = [x.a * y.a; x.a * y.b; x.b * y.a; x.b * y.b]
        { a =  min list; b = max list}
    static member (/) (x : Interval, y : Interval) = { a = x.a / y.b; b = x.b / y.a}
    static member (+) (x : Interval, y : Interval) = { a = x.a + y.a; b = x.b + y.b}
    static member (-) (x : Interval, y : Interval) = { a = x.a - y.b; b = x.b - y.a}
    static member (*) (x : Interval, y : double) = { a = x.a * y; b = x.b * y}
    static member (/) (x : Interval, y : double) = { a = x.a / y; b = x.b / y}
    static member (+) (x : Interval, y : double) = { a = x.a + y; b = x.b + y}
    static member (-) (x : Interval, y : double) = { a = x.a - y; b = x.b - y}
    static member (*) (x : double, y : Interval) = { a = x * y.a; b = x * y.b}
    static member (/) (x : double, y : Interval) = { a = x / y.b; b = x / y.a}
    static member (+) (x : double, y : Interval) = { a = x + y.a; b = x + y.b}
    static member (-) (x : double, y : Interval) = { a = x - y.b; b = x - y.a}
    static member pow (x : Interval, p : double) = { a =  Math.Pow(x.a, p); b = Math.Pow(x.b, p)}
let zeroLengthInterval(x : double) = {a=x; b=x}

type Fuzzy =
    { Top : Interval; Bottom : Interval }
    static member (*) (x : Fuzzy, y : Fuzzy) = { Top = x.Top * y.Top; Bottom = x.Bottom * y.Bottom }
    static member (/) (x : Fuzzy, y : Fuzzy) = { Top = x.Top / y.Top; Bottom = x.Bottom / y.Bottom }
    static member (+) (x : Fuzzy, y : Fuzzy) = { Top = x.Top + y.Top; Bottom = x.Bottom + y.Bottom }
    static member (-) (x : Fuzzy, y : Fuzzy) = { Top = x.Top - y.Top; Bottom = x.Bottom - y.Bottom }
    static member (*) (x : Fuzzy, y : double) = { Top = x.Top * y; Bottom = x.Bottom * y }
    static member (/) (x : Fuzzy, y : double) = { Top = x.Top / y; Bottom = x.Bottom / y }
    static member (+) (x : Fuzzy, y : double) = { Top = x.Top + y; Bottom = x.Bottom + y }
    static member (-) (x : Fuzzy, y : double) = { Top = x.Top - y; Bottom = x.Bottom - y }
    static member (*) (x : double, y : Fuzzy) = { Top = x * y.Top; Bottom = x * y.Bottom }
    static member (/) (x : double, y : Fuzzy) = { Top = x / y.Top; Bottom = x / y.Bottom }
    static member (+) (x : double, y : Fuzzy) = { Top = x + y.Top; Bottom = x + y.Bottom }
    static member (-) (x : double, y : Fuzzy) = { Top = x - y.Top; Bottom = x - y.Bottom }
    static member pow (x : Fuzzy, p : double) = 
      { 
        Top = Interval.pow(x.Top, p)
        Bottom = Interval.pow(x.Bottom, p)
      }
let number(a,b,c) = { Top = zeroLengthInterval(b); Bottom = { a=a; b=c } }        
//Example - fuzzy bond valuation
let i1 = number(0.11,0.12,0.14)
let i2 = number(0.08,0.11,0.16)
let M = 1000.
let couponRate = 0.1

let coupon = M * couponRate
let presentValue = coupon/(1.+i1)+(coupon + M)/Fuzzy.pow(1.+i2, 2.)