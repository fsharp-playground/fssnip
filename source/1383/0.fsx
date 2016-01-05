let MyFunction1 a =
   if a = "A" then 1
   elif a = "B" then 2
   else 3

let MyFunction2 a = 
   match a with
   | "A" -> 1
   | "B" -> 2
   | _ -> 3

let MyFunction3 = fun a -> 
   match a with
   | "A" -> 1
   | "B" -> 2
   | _ -> 3

let MyFunction4 = function
   | "A" -> 1
   | "B" -> 2
   | _ -> 3

let MyFunction5 a = 
   match a with
   | x when x = "A" -> 1
   | x when x = "B" -> 2
   | x -> 3


let MyFunction6 a = 

    let (|FirstVowel|FirstConsonant|Other|) p =
        match p with
        | "A" -> FirstVowel
        | "B" -> FirstConsonant
        | _ -> Other

    match a with
    | FirstVowel -> 1
    | FirstConsonant -> 2
    | Other -> 3
