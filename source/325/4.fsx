#light
#time

open System
open System.IO
open System.Threading
open System.Threading.Tasks

type CList<'a> = System.Collections.Concurrent.ConcurrentBag<'a>

let private readFile (file:string) (f:byte[] -> 'a) =
  async {
    // Open stream
    let stream = File.OpenRead(file)

    // Async read, so we don't block on the thread pool
    let! content = stream.AsyncRead(int stream.Length)

    // Close stream, we'r done
    stream.Close()

    // Apply operation on data
    return f content

  } |> Async.StartAsTask

let rec private readDir (dir:string) (f:byte[] -> 'a) =
  seq {
    // Fire of one async read per file
    for file in Directory.GetFiles(dir) do
      yield readFile file f

    // Spider down through the directory structure
    for dir in Directory.GetDirectories(dir) do
      yield! readDir dir f
  }

let readFilesAsync<'a> (dir:string) (f:byte[] -> 'a) =
  let tasks = 
    readDir dir f 
    |> Seq.cast<Task> 
    |> Array.ofSeq

  // Wait for all tasks to complete
  Task.WaitAll(tasks)

  // Return result as a seq
  seq { 
    for task in tasks do 
      yield (task :?> Task<'a>).Result 
  }

let result = 
  readFilesAsync<int> @"C:\Users\Fredrik\Projects\IronJS" (fun b -> b.Length)