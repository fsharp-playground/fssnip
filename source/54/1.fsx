// compose a list of functions fs into a single function
let compose fs = List.reduce (>>) fs

// apply list of functions to an initial arg
let fs = [(*) 2; (+) 7; (*) 3; (+) 3]
compose fs 3
// = 3 |> ((*)2 >> (+)7 >> (*)3 >> (+) 3)
// = 3 |> (*)2 |> (+)7 |> (*)3 |> (+) 3
// = (((3 * 2) + 7) * 3) + 3
// = 42