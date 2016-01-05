open System.IO

let start = System.DateTime.Now

let alphas =
    ['а';'б';'в';'г';'д';'е';'ё';'ж';'з';'и';
     'й';'к';'л';'м';'н';'о';'п';'р';'с';'т';
     'у';'ф';'х';'ц';'ч';'ш';'щ';'ъ';'ы';'ь';
     'э';'ю';'я']

let nums = [0..32]

let ht = System.Collections.Generic.Dictionary<char,int>()

List.zip alphas nums |> List.iter (fun (k,v) -> ht.[k] <- v)

let encodeFromString (s: string) =
    let rec f i n =
        if i = 4 then n
        else
            let c = ht.[s.[i]]
            f (i+1) (n*33+c)
    f 0 0

let decodeToString (v: int) =
    let a = Array.create<char> 4 ' '
    let rec f i n =
        if i = -1 then System.String(a)
        else
            a.[i] <- List.nth alphas (n%33)
            f (i-1) (n/33)
    f 3 v

let reader =
    seq { use reader = StreamReader(File.OpenRead @"I:\Lang\Ocaml\ssp\Vocabulary-4.txt")
          while not reader.EndOfStream do
              let s = reader.ReadLine()
              if s.Length = 4 then
                let r = encodeFromString s
                if (decodeToString r) <> s then
                  failwith (sprintf "%s <> %s" (decodeToString r) s)
                yield encodeFromString s }

//reader |> Seq.iter (fun v -> printfn "%d" v);;

let words = Array.ofSeq reader

let wordWithout w = function
  |0 -> w%(33*33*33)
  |1 -> (w/33/33/33)*33*33*33+w%(33*33)
  |2 -> (w/33/33)*33*33+w%33
  |_ -> (w/33)*33

let ht0 = System.Collections.Generic.Dictionary<int,Set<int>>()
let ht1 = System.Collections.Generic.Dictionary<int,Set<int>>()
let ht2 = System.Collections.Generic.Dictionary<int,Set<int>>()
let ht3 = System.Collections.Generic.Dictionary<int,Set<int>>()

let hs = [| ht0;ht1;ht2;ht3 |]

let initWWO v =
  for i in 0..3 do
    let k = wordWithout v i
    let h = hs.[i]
    if h.ContainsKey k then h.[k] <- h.[k].Add(v)
    else h.Add(k, Set.singleton v)
 
words |> Array.iter (fun w -> initWWO w)

//printfn "0: %d 1: %d 2: %d 3: %d" ht0.Count ht1.Count ht2.Count ht3.Count

exception Found of string*int
exception NotFound

let find maxSteps srcWord dstWord =
  let steps = ref 0
  let srcNum = encodeFromString srcWord
  let dstNum = encodeFromString dstWord
  let prevSrc = ref Set.empty<int>
  let curSrc = ref (Set.singleton srcNum)
  try
    let f = ref true
    while !steps <> words.Length && (Set.count !prevSrc) <> (Set.count !curSrc) do
      incr steps
      prevSrc := !curSrc
      curSrc := Set.empty<int>
      for v in !prevSrc do
        for i in 0..3 do
          let k = wordWithout v i
          let h = hs.[i]
          if h.ContainsKey k then
            if Set.contains dstNum h.[k] then
              let prevWord = decodeToString v
              raise (Found(prevWord,i))
            curSrc := Set.union !curSrc (Set.difference h.[k] !prevSrc)
      curSrc := Set.union !prevSrc !curSrc
    raise NotFound
  with
  | Found(prev,k) ->
    //printfn "found %s at %d step over %s where any is %d" (decodeToString dstNum) !steps prev k
    prev,!steps
  | NotFound ->
    // failwith (sprintf "not found for an %d steps and %d words" !steps (Set.count !prevSrc))
    "",-1


let paths src dst =
  let word, steps = find words.Length src dst
  if steps = -1 then
    printfn "%s -> %s (нет шагов): -" src dst
  else
    let results = ref [word; dst]
    let maxSteps = ref steps
    while src <> (List.head !results) do
      let word, steps = find !maxSteps src (List.head !results)
      results := word::!results
      maxSteps := steps

    printf "%s -> %s (%d шагов): " src dst (List.length !results)
    !results |> List.iter (fun v -> printf "%s " v)
    printf "\n"


let srcList = ["муха";"день";"снег";"отец";"рука";"зима";"свет";"липа"]
let dstList = ["слон";"ночь";"вода";"мать";"нога";"лето";"тьма";"клён"]

List.zip srcList dstList |> List.iter (fun (src,dst) -> paths src dst)

let stop = System.DateTime.Now
let ts = stop-start
printfn "at %A" ts