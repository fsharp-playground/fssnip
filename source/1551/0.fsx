open Hopac
open Hopac.Infixes
open Hopac.Alt.Infixes
open Hopac.Job.Infixes

let putString = mb()
let putInt = mb()
let get = ch()

Job.foreverServer (
    Alt.choose [ putString >>=? fun v -> get <-- sprintf "Echo %s" v
                 putInt >>=? fun v -> get <-- sprintf "Echo %d" v ])
|> run

Job.foreverServer (get |>> printfn "GOT: %s") |> run

// Put 5 values to 'putString' and 5 values to 'putInt'
job {
    for i in 1 .. 5 do
        do! putString <<-+ "Hello!"
        do! putInt <<-+ i 
} |> run
