type Vector<'T>(x : 'T) = class
    member o.X = x
    static member inline (+) (l : Vector<_>, r : Vector<_>) = 
        Vector(l.X + r.X)
end
 
let v1 = Vector(2)
printfn "%d" (v1 + v1).X
 
let v2 = Vector(2.0)
printfn "%f" (v2 + v2).X