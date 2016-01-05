open System.Reflection
open System.Reflection.Emit
open System.Collections.Generic

// [snippet:Simple exception analysis]
open Mono.Reflection
    
/// check for the throw opcode preceded with newobj 
let getThrownException (instr: Instruction) =
    let prev = instr.Previous
    if instr.OpCode = OpCodes.Throw && prev <> null && prev.OpCode = OpCodes.Newobj then 
        let newType = (prev.Operand :?> ConstructorInfo).ReflectedType
        if typeof<System.Exception>.IsAssignableFrom newType then
            Some (instr, newType.FullName)
        else None
    else None


let inline instructionInfo (m: MethodBase) (i, t) comment = 
    sprintf "%s->%s: %A%s" m.ReflectedType.FullName m.Name t comment

let inline hasCallOpcode (i: Instruction) =
    i.OpCode = OpCodes.Call || i.OpCode = OpCodes.Callvirt || i.OpCode = OpCodes.Calli

/// add some additional information about the exceptions
/// e.g. the value of message parameter for the standard ArgumentException 
(*[omit:(get exception resource)]*)
let types = System.Reflection.Assembly.GetAssembly(typeof<System.ArgumentException>).GetTypes()
let tryGetExceptionResource (prm: obj) = 
    types  
    |> Seq.tryFind (fun t -> t.FullName = "System.ExceptionResource")
    |> Option.bind (fun e ->Some (System.Enum.ToObject(e, prm)))
(*[/omit]*)

let addComment (i: Instruction) =
    let comment = 
        match i.Operand with
        | :? MethodBase as mi when i.OpCode=OpCodes.Call && mi.Name="ThrowArgumentException" 
                                                         && mi.GetParameters().Length>0 ->
            let msg = tryGetExceptionResource i.Previous.Operand 
            if Option.isSome msg then
                sprintf " (param: %A)" msg.Value 
            else ""
        | _ -> ""
    i, comment
    
/// add exceptions to dictionary
let inline (<~) (dict: Dictionary<_, HashSet<_>>) (m, items) = (*[omit:(...)]*)
    let set = if dict.ContainsKey m then dict.[m] 
              else
                let s = HashSet<_>() in dict.Add(m, s); s
    Seq.iter (set.Add >> ignore) items(*[/omit]*)

/// perform analysis for a method with given detalization parameters 
let analyzeExceptions (m: MethodBase) (detalization, maxDepth) =
    printfn ">> Analyze %s (%s):" m.Name m.ReflectedType.FullName
    let exnsFound = Dictionary<_,HashSet<_>>()

    let rec check d (m: MethodBase) comment = 
        if m = null || m.GetMethodBody() = null then HashSet<_>()
        else
            let tab = String.replicate d " "
            let inline printDetails() = (*[omit:(...)]*)
                printfn "%s>> %s (%s)" tab m.Name m.ReflectedType.FullName 
                let detExns = exnsFound.[m]
                if detExns.Count > 0 then detExns |> Seq.iter (printfn "\t%s%s" tab)(*[/omit]*)

            if exnsFound.ContainsKey m then printDetails()
            else
                let instructions = m.GetInstructions()
                // exceptions in the method body
                let exns = 
                    instructions 
                    |> Seq.map getThrownException 
                    |> Seq.filter Option.isSome 
                    |> Seq.map (fun e -> instructionInfo m e.Value comment)
            
                exnsFound <~ (m, exns)
                if d < detalization - 1 then printDetails()     

                // exceptions in inner calls
                if d < maxDepth then
                    let innerExceptions = 
                        instructions 
                        |> Seq.filter hasCallOpcode
                        |> Seq.map addComment
                        |> Seq.collect (fun (i, c) -> check (d+1) (i.Operand :?> MethodBase) c)
                    exnsFound <~ (m, innerExceptions)
                if d = detalization - 1 then printDetails()
            exnsFound.[m]

    check 0 m "" |> (fun i -> printfn "Summary:"; i) |> Seq.iter (printfn "  %s")

(*[omit:(Example: get a couple of methods to analyze)]*)
open System.Linq
type T() =
    let dict = Dictionary<_,_>() 
    member x.ToDictionary (arr: _[]) = arr.ToDictionary(id, string)
    member x.ThrowException() = failwith "exception here"

let methods = typeof<T>.GetMethods() |> Seq.filter (fun m -> m.DeclaringType = typeof<T> && m.GetMethodBody() <> null) 
                                     |> Seq.map (fun m -> m.Name, m) |> Map.ofSeq
 (*[/omit]*)  
analyzeExceptions methods.["ToDictionary"] (2, 5) (*[omit:(output)]*)
>> Analyze ToDictionary (FSI_0003+T):
>> ToDictionary (FSI_0003+T)
   >> ArgumentNull (System.Linq.Error)
   >> ArgumentNull (System.Linq.Error)
 >> ToDictionary (System.Linq.Enumerable)
	 System.ThrowHelper->ThrowArgumentNullException: "System.ArgumentNullException"
	 System.ThrowHelper->ThrowArgumentException: "System.ArgumentException" (param: Argument_AddingDuplicate)
Summary:
  System.ThrowHelper->ThrowArgumentNullException: "System.ArgumentNullException"
  System.ThrowHelper->ThrowArgumentException: "System.ArgumentException" (param: Argument_AddingDuplicate)(*[/omit]*)  
analyzeExceptions methods.["ThrowException"] (0, 5) (*[omit:(output)]*)
>> Analyze ThrowException (FSI_0003+T):
Summary:
  FSI_0003+T->ThrowException: "System.Exception"(*[/omit]*)
// [/snippet]