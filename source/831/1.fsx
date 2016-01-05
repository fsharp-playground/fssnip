#if INTERACTIVE
#r "System.Xml"
#r "System.Xml.Linq"
#endif

open System
open System.IO
open System.Xml.Linq
open System.Text.RegularExpressions

let rec findsolutiondir (p:DirectoryInfo) = 
      if (p.GetFiles("*.sln") |> Array.length > 0) 
      then p
      else findsolutiondir p.Parent


let root = findsolutiondir (DirectoryInfo(__SOURCE_DIRECTORY__))

let getprojectsdir (root:DirectoryInfo) = 
   let rec getdirs (root:DirectoryInfo) = seq {
      yield! root.GetDirectories() |> Array.filter(fun f -> f.GetFiles("*.fsproj") |> Array.length > 0 ) 
      yield! root.GetDirectories() |> Array.map(fun d -> getdirs d) |> Seq.concat}
   getdirs root   


   
getprojectsdir root |> Seq.iter (fun d ->    let p = new System.Diagnostics.Process();
                                             printfn "installing %A"d.Name
                                             p.StartInfo.WorkingDirectory <- root.FullName
                                             p.StartInfo.FileName <- "powershell.exe";
                                             p.StartInfo.Arguments <- ("/c nuget i " + d.Name + "\\packages.config -o Packages")
                                             p.StartInfo.RedirectStandardOutput <- true
                                             p.StartInfo.UseShellExecute <- false
                                             p.Start() |> ignore
                                             printfn "result ?"
                                             printfn "result %A" (p.StandardOutput.ReadToEnd())
                                             printfn "done"
                                 )