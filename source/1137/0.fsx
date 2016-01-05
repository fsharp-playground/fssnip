open System

let stringList (str:String) = List.filter(fun c -> c <> ' ') (Array.toList (str.ToCharArray()))

let headsMatch list1 list2 = 
    match list1 with 
        | h::t ->
            match list2 with
                | h1::t1 -> h = h1
                | [] -> false
        | [] -> List.isEmpty list2

let rec palindrome forward backward = 
    if headsMatch forward backward then
        if List.isEmpty forward then 
            true
        else
            palindrome (List.tail forward) (List.tail backward)
    else
        false

let isPal str1 = palindrome (stringList str1) (List.rev (stringList str1))

let x = isPal "abc ba"