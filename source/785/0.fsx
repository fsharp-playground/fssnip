let n = 10000000
let arr = Array.init n (fun _ -> 0)

let rec buildList n acc i = if i = n then acc else buildList n (0 :: acc) (i + 1)
let lst = buildList n [] 0

let rezArr = new ResizeArray<int>()
rezArr.AddRange(arr)

printfn "Array has [%d] elements" arr.Length
printfn "List has [%d] elements" lst.Length
printfn "ResizeArray has [%d]" rezArr.Count

#time

let doNothing _ = ()

arr |> Array.iter doNothing
lst |> List.iter doNothing
rezArr |> Seq.iter doNothing

let incr x = x + 1

arr |> Array.map incr
lst |> List.map incr
new ResizeArray<int>(rezArr |> Seq.map incr)