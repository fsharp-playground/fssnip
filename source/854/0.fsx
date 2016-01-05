open System

let getdisposable () = {new IDisposable with 
                           member x.Dispose() = printfn "I am disposed"}

type Atype () =
   let d = getdisposable ()
   interface IDisposable with
      member x.Dispose() = d.Dispose()

                        
let f ()= 
   use l = getdisposable ()
   printfn "hello"

f()//hello -> I am disposed


let f2 ()= 
   use a = new Atype()
   printfn "hello"

f2()//hello -> I am disposed
