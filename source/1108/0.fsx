#nowarn "42"

let inline ret (x : ^T) = (# "ret" x : ^S #)

let test () : int =
    if false then ret "hello" // your type checker is useless here!
    
    for i = 0 to 100 do
        if i = 42 then ret i
    
    -1

test()