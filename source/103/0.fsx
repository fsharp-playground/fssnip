open System

type 'a Tree = 
    | Node of ('a Tree list * 'a)

let myTree = Node([
                    Node(
                          [Node([],21); 
                           Node([],22) 
                          ]
                        ,2);
                    Node([],3); 
                    Node([ 
                          Node([],41); 
                          Node([],42) 
                         ]
                        ,4)
                   ]
                  ,1)

let rec findTree (cont:unit -> int) (find:int)  (t:int Tree list)= 
    match t with
    | [] -> cont()
    | (h::t) -> match h with
                | Node(child,value) ->  if value = find then 1 
                                        else t |> findTree (fun _ -> findTree cont find child) find
    

[myTree] |> findTree (fun _ -> 0) 22 |> Console.WriteLine 
//0 for not found, 1 for found