let run (program:string) =
    let b = Array.create 30000 0uy
    let p, pc = ref 0, ref 0
    let execute () =
        match program.[!pc] with
        | '>' -> incr p; incr pc
        | '<' -> decr p; incr pc
        | '+' -> b.[!p] <- b.[!p]+1uy; incr pc
        | '-' -> b.[!p] <- b.[!p]-1uy; incr pc
        | '.' -> b.[!p] |> char |> printf "%O"; incr pc
        | ',' -> b.[!p] <- System.Console.Read() |> byte; incr pc
        | '[' -> 
            if b.[!p] = 0uy then while program.[!pc] <> ']' do incr pc
            else incr pc
        | ']' ->
            if b.[!p] <> 0uy then while program.[!pc] <> '[' do decr pc
            else incr pc
        | _ -> incr pc
    while !pc < program.Length do execute ()
run "++++++++++[>+++++++>++++++++++>+++>+<<<<-]>++.>+.+++++++..+++.>++.<<+++++++++++++++.>.+++.------.--------.>+.>."