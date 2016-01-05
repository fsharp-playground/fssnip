let removeDuplicates(m_list : 'a list) : 'a list =
    let reversedList = m_list |> List.rev // filter elements in original order
    let rec filterOutDuplicate i acc = 
        if i < 0 then 
                    acc |> List.rev // result returned in original order
                 else
            if List.exists(fun y -> reversedList.[i] = y) acc then
                    filterOutDuplicate (i - 1) (acc)
                else // function starts at last element of list
                    filterOutDuplicate (i - 1) (reversedList.[i] :: acc)
    filterOutDuplicate (m_list.Length - 1) List.empty

let testlist = ["e"; "j"; "f"; "h"; "d"; "i"; "k"; "l"; "g"; "a"; "b"; "c"; "d"; "e"; "f"; "g" ]
let removeDupL9 = removeDuplicates(testlist)