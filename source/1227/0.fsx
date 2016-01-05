open System.Text
open System
open System.Linq

let input = "elektriba"

let rec remove i l =
    match i, l with
    | 0, h::t -> t
    | i, h::t-> h::remove (i - 1) t
    | _, [] -> failwith "index out of range"

let rnd = new Random()

let shuffle (input:string) =
    let _shuffle (inp:string) =
        let mutable lst = List.ofSeq inp
        let result = Array.create inp.Length '\000'
        for i = 0 to result.Length - 1 do
            let ix = rnd.Next(lst.Length - 1)
            result.[i] <- lst.[ix]
            lst <- remove ix lst
        List.ofArray result

    match input.Length with
    | l when l < 2 -> input
    | _ -> input.[0] :: (_shuffle input.[1 .. input.Length - 2]) @ [input.[input.Length - 1]]
            |> List.map (fun (c:char) -> c.ToString())
            |> List.reduce (+)

shuffle input
