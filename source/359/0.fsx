let (|Apply|) f input = f input

let addFunc = fun a -> a+5

let add5 (Apply addFunc res) = res

let add5' (Apply ((+) 5) res) = res

//alas: let add5'' (Apply (fun -> a + 5) res) = res 