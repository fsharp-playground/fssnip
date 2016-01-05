module SearchTree = 

  open System

  let inline private ($) a b = b a

  type T<'a> = {
    Map : Map<char, T<'a>> 
    Values : 'a list
  }

  type TO<'a> = T<'a> option

  let empty<'a> : TO<'a> = None
  let insert (s:string) value (t:TO<_>) =
    
    let rec insert (i:int) (t:TO<_>) =
      if i < s.Length then
        
        let c = s.[i]

        match t with
        | None -> 
          let subNode = None $ insert (i+1)
          let map = Map.empty $ Map.add c subNode
          {Map=map; Values=[value]}

        | Some node ->
          match node.Map $ Map.tryFind c with
          | None ->
            let subNode = None $ insert (i+1)
            let map = node.Map $ Map.add c subNode
            {Map=map; Values=value::node.Values}

          | Some subNode ->
            let subNode = Some subNode $ insert (i+1)
            let map = node.Map $ Map.add c subNode
            {Map=map; Values=value::node.Values}

      else
        {Map=Map.empty; Values=[value]}

    t $ insert 0 $ Some

  let find (s:string) (t:TO<_>) =
    
    let rec find (i:int) (t:TO<_>) =
      
      if i < s.Length then
        let c = s.[i]

        match t with
        | None -> []
        | Some node ->
          node.Map $ Map.tryFind c $ find (i+1)
        
      else
        match t with
        | None -> []
        | Some node -> node.Values
        
    t $ find 0

    
//Example
open System

let rnd = Random()
let randomChar () =
  Math.Floor((rnd.NextDouble() * 26.0) + 65.0) |> char

let randomString () =
  let sb = new Text.StringBuilder()
  for i = 0 to 20 do
    sb.Append(randomChar()) |> ignore
  sb.ToString()

let mutable tree = 
  SearchTree.empty<int>

for i = 0 to 1000000 do
  tree <- tree |> SearchTree.insert (randomString()) i 
