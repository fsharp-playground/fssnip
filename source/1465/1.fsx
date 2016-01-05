/// Test Anything Protocol (TAP) NUnit runner
module Tap

open System
open System.Reflection
open NUnit.Framework

module internal List =
    let rec combinations = function
    | [] -> [[]]
    | hs :: tss ->
        [ for h in hs do
            for ts in combinations tss ->
                h :: ts ]

module internal GenerateTests =

   type Args = obj[]
   type ExpectedResult = obj option
   type ExceptionType = Type option
   type Test = Test of MethodInfo * Args * ExpectedResult * ExceptionType

   let getCustomAttribute<'TAttribute when 'TAttribute :> Attribute> (mi:MethodInfo) = 
      mi.GetCustomAttribute(typeof<'TAttribute>, true) :?> 'TAttribute

   let toExpectedException (mi:MethodInfo) =
      let attr = getCustomAttribute<ExpectedExceptionAttribute>(mi)
      if attr <> null then Some attr.ExpectedException else None
   
   let (|Ignore|_|) (mi:MethodInfo) =
      if getCustomAttribute<IgnoreAttribute>(mi) <> null then Some() else None

   let (|TestCases|_|) (mi:MethodInfo) =
      let cases = mi.GetCustomAttributes(typeof<TestCaseAttribute>, true)
      if cases.Length > 0 then Some(cases |> Seq.cast<TestCaseAttribute>)
      else None

   let (|TestCaseSource|_|) (mi:MethodInfo) =
      let source = getCustomAttribute<TestCaseSourceAttribute>(mi)
      if source <> null then Some(source.SourceName, source.SourceType)
      else None

   let (|UnitTest|_|) (mi:MethodInfo) =
      if getCustomAttribute<TestAttribute>(mi) <> null then Some() else None

   let fromCases (mi:MethodInfo) (cases:TestCaseAttribute seq) =
      let ex = toExpectedException(mi)
      [|for case in cases ->
         let expected = if case.HasExpectedResult then Some case.ExpectedResult else None
         Test(mi, case.Arguments, expected, ex)
      |]

   let fromValues (mi:MethodInfo) =
      [| let ps = mi.GetParameters()
         let argValues =
            [for pi in ps ->
               match pi.GetCustomAttribute(typeof<ValuesAttribute>, true) with
               | :? ValuesAttribute as attr -> [for x in attr.GetData(pi) -> x]
               | _ -> invalidOp "Expecting values"]
         let ex = toExpectedException mi
         match List.combinations argValues with
         | [] -> yield Test(mi, [||], None, ex)
         | xs -> yield! [for args in xs -> Test(mi, List.toArray args, None, ex)]
      |]

   let fromSource instance (mi:MethodInfo) (sourceName,sourceType:Type) =
      let pi = mi.DeclaringType.GetProperty(sourceName)
      let getter = pi.GetGetMethod()
      let instance = if sourceType <> null then Activator.CreateInstance(sourceType) else instance
      let data = getter.Invoke(instance, [||]) :?> System.Collections.IEnumerable
      let ex = toExpectedException mi
      [|for item in data ->
         match item with
         | :? TestCaseData as case ->
            let expected = if case.HasExpectedResult then Some(case.Result) else None
            Test(mi, case.Arguments, expected, ex) 
         | :? (obj[]) as args -> Test(mi, args, None, ex)
         | arg -> Test(mi, [|arg|], None, ex)
      |]

   let generateTests instance (mi:MethodInfo) =
      match mi with
      | Ignore -> [||]
      | TestCases cases -> fromCases mi cases
      | TestCaseSource(data) -> fromSource instance mi data
      | UnitTest -> fromValues mi
      | _ -> [||]

   let runTest instance (Test(mi,args,expected,exType)) = 
      try
         let actual = mi.Invoke(instance,args)
         match expected with
         | Some expected -> Assert.AreEqual(expected, actual)
         | None -> ()
         None
      with ex -> 
         match exType with
         | Some t when t = ex.GetType() -> None
         | _ -> Some ex

   let color c =
      let previous = Console.ForegroundColor
      Console.ForegroundColor <- c
      { new System.IDisposable with 
         member __.Dispose() = Console.ForegroundColor <- previous
      }

   let showResult number (Test(mi,args,_,_)) error =
      let name =
         mi.Name + 
            if args.Length > 0 then "(" + String.Join(",", args) + ")"
            else ""
      match error with
      | None ->
         using (color ConsoleColor.Green) <| fun _ ->
            printfn "ok %d - %s" number name
      | Some e ->
         using (color ConsoleColor.Red) <| fun _ ->
            printfn "not ok %d - %s" number name
            printfn "  %A" e

   let runTests instance (tests:Test[]) =
      printfn "1..%d" tests.Length
      tests |> Array.iteri (fun i test ->
         runTest instance test |> showResult (i+1) test 
      )

open GenerateTests

let Run (testType:System.Type) =
   let constr = testType.GetConstructor([||])
   let instance = if constr <> null then constr.Invoke([||]) else null
   let methods = testType.GetMethods()
   let tests = [|for mi in methods do yield! generateTests instance mi|]
  
   let runMethodsWithAttribute attr = 
      methods 
      |> Array.filter (fun mi -> mi.GetCustomAttribute(attr, true) <> null)
      |> Array.iter (fun mi -> mi.Invoke(instance,[||]) |> ignore)

   runMethodsWithAttribute typeof<SetUpAttribute>  
   runTests instance tests
   runMethodsWithAttribute typeof<TearDownAttribute>