(*[omit:(open declarations)]*)
open Hopac
open Hopac.Infixes
open Hopac.Job.Infixes
open Hopac.Alt.Infixes
(*[/omit]*)

let put, get, empty, contains = ch(), ch(), ch(), ch()

// Initially, the buffer is empty
run (empty <-+ ())

Job.foreverServer (
    Alt.choose [ empty >>.? (put >>= Ch.send contains)
                 contains >>=? fun v -> Ch.send get v >>. Ch.send empty () ])
|> run

// Repeatedly try to put value into the buffer
job { do! Async.Sleep 1000
      for i in 0 .. 10 do
          printfn "putting: %d" i
          do! put <-- string i
          do! Async.Sleep 500 }
|> start

// Repeatedly read values from the buffer and print them
job { while true do 
          do! Async.Sleep 250
          let! v = get
          printfn "got: %s" v }
|> start
