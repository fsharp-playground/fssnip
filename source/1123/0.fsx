let count = List.fold (fun acc _ -> acc+1) 0
let reduce func (hd::tl) = List.fold func hd tl
let rev  (hd::tl) = List.fold (fun acc next -> next::acc) (hd::[]) tl
let map func (hd::tl) = 
    List.fold (fun acc next -> (func next)::acc) (func hd::[]) tl 
    |> List.rev