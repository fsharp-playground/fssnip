let a = ["ccc";"aaaz";"d";"bb"]
let b = ["Cameron",31; "Rachel",29; "Nicole",10; "Katheryn",8; "Charlotte",1]

let sorted a b = List.sortBy b a

(* Sorting the list a by the last character of each string in the list *)
sorted a (fun s -> s.[s.Length - 1])

(* Sorting the list b by the age of each tuple in the list *)
sorted b (fun (name,age) -> age)