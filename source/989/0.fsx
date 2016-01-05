open System
open System.Collections.Generic

module LII =
    type InputData =
        | Start
        | End
        | Word of string

    let data = Dictionary<InputData,InputData list>()

    let rnd = Random(DateTime.Now.Millisecond)

    let generateResponse() =
        let startWords = data.[Start]
        Seq.unfold( fun current -> let nextWords = data.[current]
                                   match nextWords.[rnd.Next(nextWords.Length)] with  
                                   | End -> None
                                   | Word(x) as i -> Some(x,i)
                                   | _ -> failwith "impossible") Start
        |> fun s -> String.Join(" ",s)

    let handleInput (text:string) =
        let text = text.Replace(",","").ToLower()
        seq { for s in text.Split('.') do
                yield Start
                for x in s.Split(' ') -> Word x
                yield End } 
        |> Seq.pairwise
        |> Seq.iter( fun (prev,next) -> if data.ContainsKey(prev) = false then data.Add(prev,[])
                                        let current = data.[prev]
                                        data.[prev] <- next::current)
        generateResponse()