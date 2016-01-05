(* c:\example.txt content:

== MyItem 1 ==
...some content...
Total: 10
Success

== MyItem 2 ==
...some content...
Total: 2
Failed

== MyItem 3 ==
...some content...
Total: 14
Success

== MyItem 4 ==
...some content...
Total: 7
Failed

*)


#if INTERACTIVE
  ;;
#else
module myParser
#endif

let file = @"c:\example.txt"
let lines = System.IO.File.ReadAllLines(file)

let countFailed =
    let rec readLines (myLines:string list) resultItem resultdata =
        match myLines with
        | h::t when h.Contains("==") -> 
            let myItem = h.Substring(3, h.Length-6)
            readLines t (myItem::resultItem) resultdata
        | total::state::t when total.Contains("Total:") && state.StartsWith("Failed") -> 
            let info = (List.head resultItem), total.Substring(7) //, state ...etc information
            readLines t resultItem (info::resultdata)
        | h::t  -> readLines t resultItem resultdata
        | [] -> resultdata
    readLines (lines |> Seq.toList) [] []

// val countFailed : (string * string) list = [("MyItem 4", "7"); ("MyItem 2", "2")]

