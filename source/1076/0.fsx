// [snippet: Definition]
open System.Reflection

let printw width =
    let formatCode = sprintf "%%%ds" -width
    printf <| Printf.TextWriterFormat<string -> unit, unit>(formatCode)
    
let printMembers header column (ms:string list) =
    let printItems f =
        let hasValue = not << System.String.IsNullOrWhiteSpace
        if hasValue header then printfn "[%s]" header

        List.iteri f ms
        printf "\n\n"

    match ms with
    | [] -> ()
    | _ when ms.Length <= column -> printItems (fun _ -> printf "%s ")

    | otherwize ->
        let longest = ms |> List.maxBy (fun s -> s.Length)
        let width = longest.Length + 1
        printItems (fun i item ->
            if i % column = 0 && i <> 0 then printf "\n" ;
            printw width item)

let collectNames (ms:seq<#MemberInfo>) =
    ms
    |> Seq.map (fun m -> m.Name)
    |> Set.ofSeq
    |> Set.toList
    |> List.sort

let dirbase x =
    let t = x.GetType()
    t.GetMembers() |> collectNames

let dir x =
    let print header = collectNames >> printMembers header 3
    let t = x.GetType()
    t.GetMethods()    |> print "Methods"
    t.GetProperties() |> print "Properties"
    t.GetFields()     |> print "Fields"
// [/snippet]

// [snippet: Usage]
// > dir 1;;
// [Methods]
// CompareTo   Equals      GetHashCode 
// GetType     GetTypeCode Parse       
// ToString    TryParse    
//
// [Fields]
// MaxValue MinValue 
//
// val it : unit = ()
//
// > dir [] ;;
// [Methods]
// CompareTo         Cons              Equals            
// GetHashCode       GetType           ToString          
// get_Empty         get_Head          get_HeadOrDefault 
// get_IsCons        get_IsEmpty       get_Item          
// get_Length        get_Tag           get_Tail          
// get_TailOrNull    
//
// [Properties]
// Empty         Head          HeadOrDefault 
// IsCons        IsEmpty       Item          
// Length        Tag           Tail          
// TailOrNull    
//
// val it : unit = ()
// [/snippet]