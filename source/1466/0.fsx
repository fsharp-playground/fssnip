let makeCounter () =
    let c = ref 0
    fun () -> c := !c + 1; !c
        
let counter1 = makeCounter ()
let counter2 = makeCounter ()

[counter1(); counter1(); counter2(); counter1(); counter2()]
// Ergebnis: [1; 2; 1; 3; 2]