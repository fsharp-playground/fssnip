let run (program:string) =
    let rand = System.Random()
    let stack = System.Collections.Generic.Stack()
    let grid = Array2D.create 80 25 ' '
    let lines = program.Split('\n')
    lines 
    |> Seq.truncate 25
    |> Seq.iteri (fun y line ->
        let length = min 80 line.Length
        for x = 0 to length-1 do grid.[x,y] <- line.[x]
    )
    let push c = stack.Push c
    let pop () = stack.Pop()
    let running = ref true
    let string_mode = ref false
    let v = ref (1,0)
    let pc = ref (0,0)
    let (+.) (a,b) (c,d) = a+c, b+d
    let instruction c =
        match c with
        | c  when System.Char.IsDigit(c) -> 
            let digit = c |> byte 
            digit - byte '0' |> push
        | '+' -> pop () + pop () |> push
        | '-' -> let a, b = pop (), pop () in b - a |> push
        | '*' -> pop () * pop () |> push
        | '/' -> let a, b = pop (), pop () in b / a |> push
        | '%' -> let a, b = pop (), pop () in b % a |> push
        | '!' -> (if pop() = 0uy then 1uy else 0uy) |> push
        | ''' -> let a, b = pop (), pop() in (if b > a then 1uy else 0uy) |> push
        | '>' -> v := (1,0)
        | '<' -> v := (-1,0)
        | '^' -> v := (0,-1)
        | 'v' -> v := (0,1)
        | '?' -> v := [-1,0;1,0;0,-1;0,1].[rand.Next(4)]
        | '_' -> v := if pop() = 0uy then (1,0) else (-1,0) 
        | '|' -> v := if pop() = 0uy then (0,1) else (0,-1)
        | '"' -> string_mode := true
        | ':' -> let top = pop() in push top; push top
        | '\\' -> let a,b = pop(),pop() in push b; push a
        | '$' -> pop () |> ignore
        | '.' -> pop () |> int |> printf "%d"
        | ',' -> pop () |> char |> printf "%O"
        | '#' -> pc := !pc +. !v
        | 'p' -> let y,x,v = pop(), pop(), pop() in grid.[int y,int x] <- char v
        | 'g' -> let y,x = pop(), pop() in grid.[int y, int x] |> byte |> push
        | '&' -> System.Console.Read() |> byte |> push
        | '~' -> System.Console.Read() |> byte |> push
        | '@' -> running := false
        |  _  -> ()
    let execute c =
        if !string_mode then
            if c = '"' then string_mode := false
            else c |> byte |> push
        else
            instruction c
    push 0uy    
    while !running do
        let x,y = !pc
        let x,y = (x+80)%80, (y+25)%25
        let c = grid.[x,y]
        execute c
        pc := !pc +. !v

let program=">              v
v  ,,,,,\"Hello\"<
>48*,          v
v,,,,,,\"World!\"<
>25*,@"

run program