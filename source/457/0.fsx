type Command = { Redo: unit->unit; Undo: unit->unit }

let result = ref 7

let add n = { 
    Redo = (fun _ -> result:= !result + n); 
    Undo = (fun _ -> result := !result - n) }

let minus n = { 
    Redo = (fun _ -> result:= !result - n); 
    Undo = (fun _ -> result := !result + n) }

let cmd = (add 3)
printfn "current state = %d" !result

cmd.Redo()
printfn "after redo: %d" !result

cmd.Undo()
printfn "after undo: %d" !result
