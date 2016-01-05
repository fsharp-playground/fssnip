#light
#time

open System
open System.IO
open System.Threading
open System.Threading.Tasks

type CList<'a> = System.Collections.Concurrent.ConcurrentBag<'a>

let private readFile (file:string) (result:CList<'a>) (f:byte[] -> 'a) =
  async {
    // Open stream
    let stream = File.OpenRead(file)

    // Async read, so we don't block on the thread pool
    let! content = stream.AsyncRead(int stream.Length)

    // Add to our concurrent bag
    result.Add(f content)

    // Close stream, we'r done
    stream.Close()

  } |> Async.StartAsTask

let rec private readDir (dir:string) (result:CList<'a>) (f:byte[] -> 'a) =
  seq {
    // Fire of one async read per file
    for file in Directory.GetFiles(dir) do
      yield readFile file result f

    // Spider down through the directory structure
    for dir in Directory.GetDirectories(dir) do
      yield! readDir dir result f
  }

let rec readFilesAsync<'a> (dir:string) (f:byte[] -> 'a) =
  let result = new CList<'a>()

  let tasks = 
    readDir dir result f 
    |> Seq.cast<Task> 
    |> Array.ofSeq

  // Wait for all tasks to complete
  Task.WaitAll(tasks)

  // Return result
  result :> seq<'a>

let result = 
  readFilesAsync<int> @"C:\Users\Fredrik\Projects\IronJS" (fun b -> b.Length)