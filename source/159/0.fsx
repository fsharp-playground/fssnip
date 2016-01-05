// This takes something like [1;2;3;4] and returns 
// [4][4; 3][4; 3; 2][4; 3; 2; 1][4; 3; 1][4; 2][4; 2; 1]
// [4; 1][3][3; 2][3; 2; 1][3; 1][2][2; 1][1] 

let allCombinations lst =
    let rec comb accLst elemLst =
        match elemLst with
        | h::t ->
            let next = [h]::List.map (fun el -> h::el) accLst @ accLst
            comb next t
        | _ -> accLst
    comb [] lst

// The output is in reverse order of creation of the list, 
// in order to avoid a second list concatenation. This can be
// changed in the function itself, or you can sort the result
// with the following.

let sortListList ls =
    let rec cmp lstA lstB =
        match lstA, lstB with
        | hA::tA, hB::tB when hA=hB -> cmp tA tB
        | hA::tA, hB::tB when hA<hB -> -1
        | hA::tA, hB::tB when hA>hB -> 1
        | [], [] -> 0
        | [], _ -> -1
        | _ -> 1
    List.map (fun l -> List.sort l) ls
    |> List.sortWith (fun a b -> cmp a b)
