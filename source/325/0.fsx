#light
#time

open System
open System.IO
open System.Threading
open System.Threading.Tasks

type CList<'a> = System.Collections.Concurrent.ConcurrentBag<'a>

let readFile (file:string) (result:CList<'a>) (f:byte[] -> CList<'a> -> unit) =
  async {
    let stream = File.OpenRead(file)
    let! content = stream.AsyncRead(int stream.Length)
    f content result
    stream.Close()

  } |> Async.StartAsTask

let rec readDir (dir:string) (result:CList<'a>) (f:byte[] -> CList<'a> -> unit) =
  seq {
    for file in Directory.GetFiles(dir) do
      yield readFile file result f

    for dir in Directory.GetDirectories(dir) do
      yield! readDir dir result f
  }


let rec collectFiles<'a> (dir:string) (f:byte[] -> CList<'a> -> unit) =
  let result = new CList<'a>()
  let tasks = readDir dir result f |> Seq.cast<Task> |> Array.ofSeq
  Task.WaitAll(tasks)
  result.ToArray()

let result = 
  (collectFiles<int> @"C:\Users\Fredrik\Projects\IronJS" (fun b r -> 
    r.Add(b.Length)
  )).Length