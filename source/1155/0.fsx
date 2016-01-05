//LAB3

type 'a Tree = Empty | Branch of 'a * 'a Tree * 'a Tree

let balancedBinaryTree n = 
    let rec loop l cont =
        if l <= n then
            loop (2*l) (fun lt -> loop (2*l+1) (fun rt -> cont <| Branch ('x', lt, rt)))
        else
            cont Empty
    loop 1 id

let rez = balancedBinaryTree 6