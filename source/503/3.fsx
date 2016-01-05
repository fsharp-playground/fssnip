//Pack consecutive duplicates of list elements into sublists.
// https://sites.google.com/site/prologsite/prolog-problems/1
let packDups l = 
       let result =  List.foldBack (
                            fun ele (flist,(mainlist:List<List<_>>)) -> 
                                    match (flist,mainlist) with
                                    |([],_)   ->  (ele::flist,mainlist) 
                                    |(h::_,_) when ele = h -> ((ele::flist),mainlist)
                                    |(_::_,s) when s.Head = List.empty  -> ([ele], [flist])
                                    |(_::_,_) -> ([ele],flist::mainlist)
                                    ) l ([], [[]] )  
       fst(result)::snd(result) 

packDups [1;1;2;2;1;1;3;3;4] 
