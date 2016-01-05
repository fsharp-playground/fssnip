let rec powerset = function
                   | [] -> [[]]
                   | h::t -> List.fold (fun xs t -> (h::t)::t::xs) [] (powerset t);