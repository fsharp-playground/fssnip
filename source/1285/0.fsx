open System.Collections.Generic

type CodeTree<'T> = Leaf of 'T * int | Fork of CodeTree<'T> * CodeTree<'T> * 'T list * int

let chars  = function | Leaf(c,_) -> [c] | Fork(_,_,l,_) -> l
let weight = function | Leaf(c,w) ->  w  | Fork(_,_,_,w) -> w
let mergeCT ct1 ct2 = Fork(ct1, ct2, chars ct1 @ chars ct2, weight ct1 + weight ct2)
let foldCodeTree fNode (fLeaf:'T -> 'a) (x:'T CodeTree) = 
         let rec f x = match x with | Leaf(e,w) -> fLeaf(e) | Fork(l,r,cs,w) -> fNode(f l)(f r) 
         f x 
  
module List =
    let span f xs = 
        let l,r = xs |> List.fold(fun (l,r: _ list) e -> if f e then e::l, r.Tail else l,r ) ([],xs)
        l |> List.rev, r

module Huffman = 
   let buildFreq (sample : 'T list) = 
       if sample.Length = 0 then []
       else let sample = sample |> List.sort
            let rec count (sample: _ list) = [ if sample.Length <> 0 then 
                                                let left, right = sample |> List.span ((=) sample.Head)
                                                yield sample.Head,left.Length
                                                yield! count right  ]
            count sample |> List.sortBy snd        

   let buildFromFreq(distrib:('T*int) list) : CodeTree<'T> =
       let rec loop = function 
       | [] ->  failwith "not possible"
       | [r] -> r
       | f::s::xs ->    let newCT = mergeCT f s
                        let left, right = List.span (fun e -> weight e < weight newCT) xs
                        loop(left @ (newCT:: right))
       loop (distrib  |> List.sortBy(snd) |> List.map(Leaf))

   let decode (codeTree:_ CodeTree) (bits : int list) =
       let o = bits |> List.fold(fun ((Fork(l,r,_,_)), decMsg) b ->  let branch = if b = 0 then l else r
                                                                     match branch with | Leaf(c,_) -> (codeTree, c::decMsg) 
                                                                                       | _         -> (branch  , decMsg))
                                     (codeTree, [])
       snd o |> List.rev

   let makeEncoderDic (tree: 'T CodeTree) = 
      foldCodeTree (fun (lMap:Map<'T,int list>) rMap -> lMap |> Map.fold(fun m k v -> m.Add(k, 0::v)) (rMap |> Map.fold(fun m k v -> m.Add(k, 1::v)) Map.empty)) 
                   (fun c -> Map.empty.Add(c,[])) 
                   tree
   
   let encode (dic:Map<'T,int list>) (text : 'T list) = text |> List.fold(fun s e -> s@dic.[e]) ([])


   let secret = [0;0;1;1;1;0;1;0;1;1;1;0;0;1;1;0;1;0;0;1;1;0;1;0;1;1;0;0;1;1;1;1;1;0;1;0;1;1;0;0;0;0;1;0;1;1;1;0;0;1;0;0;1;0;0;0;1;0;0;0;1;0;1]
   let frenchCode = Fork(Fork(Fork(Leaf('s',121895),Fork(Leaf('d',56269),Fork(Fork(Fork(Leaf('x',5928),Leaf('j',8351),['x';'j'],14279),Leaf('f',16351),['x';'j';'f'],30630),Fork(Fork(Fork(Fork(Leaf('z',2093),Fork(Leaf('k',745),Leaf('w',1747),['k';'w'],2492),['z';'k';'w'],4585),Leaf('y',4725),['z';'k';'w';'y'],9310),Leaf('h',11298),['z';'k';'w';'y';'h'],20608),Leaf('q',20889),['z';'k';'w';'y';'h';'q'],41497),['x';'j';'f';'z';'k';'w';'y';'h';'q'],72127),['d';'x';'j';'f';'z';'k';'w';'y';'h';'q'],128396),['s';'d';'x';'j';'f';'z';'k';'w';'y';'h';'q'],250291),Fork(Fork(Leaf('o',82762),Leaf('l',83668),['o';'l'],166430),Fork(Fork(Leaf('m',45521),Leaf('p',46335),['m';'p'],91856),Leaf('u',96785),['m';'p';'u'],188641),['o';'l';'m';'p';'u'],355071),['s';'d';'x';'j';'f';'z';'k';'w';'y';'h';'q';'o';'l';'m';'p';'u'],605362),Fork(Fork(Fork(Leaf('r',100500),Fork(Leaf('c',50003),Fork(Leaf('v',24975),Fork(Leaf('g',13288),Leaf('b',13822),['g';'b'],27110),['v';'g';'b'],52085),['c';'v';'g';'b'],102088),['r';'c';'v';'g';'b'],202588),Fork(Leaf('n',108812),Leaf('t',111103),['n';'t'],219915),['r';'c';'v';'g';'b';'n';'t'],422503),Fork(Leaf('e',225947),Fork(Leaf('i',115465),Leaf('a',117110),['i';'a'],232575),['e';'i';'a'],458522),['r';'c';'v';'g';'b';'n';'t';'e';'i';'a'],881025),['s';'d';'x';'j';'f';'z';'k';'w';'y';'h';'q';'o';'l';'m';'p';'u';'r';'c';'v';'g';'b';'n';'t';'e';'i';'a'],1486387)

   let r = decode frenchCode secret

   let r' = decode frenchCode (encode (makeEncoderDic frenchCode) r)
