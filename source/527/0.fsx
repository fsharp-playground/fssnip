(*
http://www.4clojure.com/problem/53

Given a vector of integers, find the longest consecutive sub-sequence of increasing numbers. 
If two sub-sequences have the same length, use the one that occurs first. 
An increasing sub-sequence must have a length of 2 or greater to qualify.
	
(= (__ [1 0 1 2 3 0 4 5]) [0 1 2 3])
	
(= (__ [5 6 1 3 2 7]) [5 6])
	
(= (__ [2 3 3 4 5]) [3 4 5])
	
(= (__ [7 6 5 4]) [])

*)
let increasesubseq l = 
    let rec increasesubseqinner l intermediateresult accuresult =
        match (l,intermediateresult,accuresult) with
        |([],[_],_) -> accuresult // when the intermediateresult contains only one item
        // cons the intermediateresult with the accuresult
        |([],_,_) ->  (intermediateresult::accuresult) 
        |(h::t,[],_) -> increasesubseqinner t [h] accuresult // first item in the list
        |(h::t,h1::_,_) -> 
            if h1+1 = h then increasesubseqinner t (h::intermediateresult) accuresult
            else increasesubseqinner t [h] (intermediateresult::accuresult)
        // when there is only 1 item 
        |(_::t,_::t1,_) when t1.IsEmpty ->  increasesubseqinner t [] accuresult 
        // when the accresult is [[]]
        |(_::t,_,h::_) when h.IsEmpty ->  increasesubseqinner t [] [intermediateresult] 
    let n = increasesubseqinner l [] [[]] 
            |> List.maxBy(fun x -> List.length(x))
            |> List.rev
    if n.Tail.IsEmpty then [] else n


increasesubseq [5;6;1;3;2;7] 
increasesubseq [1;0;1;2;3;0;4;5]
increasesubseq [2;3;3;4;5]
increasesubseq [7;6;5;4]