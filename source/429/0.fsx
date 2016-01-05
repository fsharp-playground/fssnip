// [snippet:Tuple overloads]
type TupleUtils =
    static member Item1(t) = let (x,_) = t in x
    static member Item1(t) = let (x,_,_) = t in x
    static member Item1(t) = let (x,_,_,_) = t in x
    static member Item1(t) = let (x,_,_,_,_) = t in x
    static member Item1(t) = let (x,_,_,_,_,_) = t in x
    static member Item1(t) = let (x,_,_,_,_,_,_) = t in x
    
    static member Item2(t) = let (_,x) = t in x
    static member Item2(t) = let (_,x,_) = t in x
    static member Item2(t) = let (_,x,_,_) = t in x
    static member Item2(t) = let (_,x,_,_,_) = t in x
    static member Item2(t) = let (_,x,_,_,_,_) = t in x
    static member Item2(t) = let (_,x,_,_,_,_,_) = t in x

    static member Item3(t) = let (_,_,x) = t in x
    static member Item3(t) = let (_,_,x,_) = t in x
    static member Item3(t) = let (_,_,x,_,_) = t in x
    static member Item3(t) = let (_,_,x,_,_,_) = t in x
    static member Item3(t) = let (_,_,x,_,_,_,_) = t in x

    static member Item4(t) = let (_,_,_,x) = t in x
    static member Item4(t) = let (_,_,_,x,_) = t in x
    static member Item4(t) = let (_,_,_,x,_,_) = t in x
    static member Item4(t) = let (_,_,_,x,_,_,_) = t in x

    static member Item5(t) = let (_,_,_,_,x) = t in x
    static member Item5(t) = let (_,_,_,_,x,_) = t in x
    static member Item5(t) = let (_,_,_,_,x,_,_) = t in x

    static member Item6(t) = let (_,_,_,_,_,x) = t in x
    static member Item6(t) = let (_,_,_,_,_,x,_) = t in x

    static member Item7(t) = let (_,_,_,_,_,_,x) = t in x    
// [/snippet]

// [snippet:Example]
//f is infered as 'a * 'b -> 'a * 'a
let f t = let x,_ = t in x,x

//without the type annotation, you will get the error:
//"A unique overload for method 'Item1' could not be determined 
//based on type information prior to this program point"
let f' (t:_*_) = let x = TupleUtils.Item1(t) in x, x
// [/snippet]