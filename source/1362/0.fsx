/// When called with a single argument, returns a function 
/// that counts the number of times it has been called
let createCounter initial =
  let state = ref initial
  (fun () -> 
     let current = !state
     state := current + 1
     current)

// Create a counter and call it a few times
let counter = createCounter 0
let n0 = counter ()
let n1 = counter ()
let n2 = counter ()
