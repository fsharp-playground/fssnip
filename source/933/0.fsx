module PhantomUnit2 =
    type [<Measure>] b
 
    type Vec<'a, [<Measure>] 'n> = 
        | V of List<'a>
 
    let nil<'a> : Vec<'a, 1> =
        V [] 
 
    let cons (x : 'a) (V xs : Vec<'a,'n>) : Vec<'a,'n b> =
        V (x :: xs)
 
    let append (V xs : Vec<'a,'n>) (V ys : Vec<'a,'m>) : Vec<'a,'n 'm> =
        V (xs @ ys)
 
    let zip (V xs : Vec<'a,'n>) (V ys : Vec<'b,'n>) : Vec<'a * 'b,'n> =
        V (List.zip xs ys)
 
    let sum (V xs : Vec<int,'n>) : int = 
        List.sum xs
 
    let l = cons 1 (cons 2 (cons 3 nil))
    let g = zip l l

    (*
    error FS0001: Type mismatch. Expecting a
        Vec<int,1>    
    but given a
        Vec<int,b ^ 3>    
    The unit of measure '1' does not match the unit of measure 'b ^ 3' 
    *)
    //let h = zip l (append l l)
 
    printfn "%A" l
    printfn "%A" g