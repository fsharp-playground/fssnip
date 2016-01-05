let run (program:string) =
    let b = Array.create 30000 0uy
    let p, pc = ref 0, ref 0
    let rec execute () =
        match program.[!pc] with
        | '>' -> incr p; incr pc
        | '<' -> decr p; incr pc
        | '+' -> b.[!p] <- b.[!p]+1uy; incr pc
        | '-' -> b.[!p] <- b.[!p]-1uy; incr pc
        | '.' -> b.[!p] |> char |> printf "%O"; incr pc
        | ',' -> b.[!p] <- System.Console.ReadKey().KeyChar |> byte; incr pc
        | '[' -> if b.[!p] = 0uy then findend -1 else incr pc
        | ']' -> if b.[!p] <> 0uy then findstart -1 else incr pc        
        | _ -> incr pc
    and findend count =
        match program.[!pc] with
        | ']' -> if count > 0 then incr pc; findend (count-1)        
        | '[' -> incr pc; findend (count+1)
        | _ -> incr pc; findend count          
    and findstart count =
        match program.[!pc] with
        | '[' -> if count > 0 then decr pc; findstart (count-1)        
        | ']' -> decr pc; findstart (count+1)
        | _ -> decr pc; findstart count      
    while !pc < program.Length do execute ()

run "++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>."