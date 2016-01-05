/// Test Anything Protocol (TAP) NUnit runner
module Tap

open System
open System.Collections
open System.Reflection
open NUnit.Framework

type Args = obj[]
type ExpectedResult = obj option
type ExceptionType = Type option
type Test = Test of MethodInfo * Args * ExpectedResult * ExceptionType

let internal getCustomAttribute<'TAttribute when 'TAttribute :> Attribute> (mi:MethodInfo) = 
   mi.GetCustomAttribute(typeof<'TAttribute>, true) :?> 'TAttribute

module internal SourceData =

   let (|SourceProperty|_|) (name,t:Type) =
      let pi = t.GetProperty(name)
      if pi <> null then Some(pi.GetGetMethod()) else None

   let (|SourceMethod|_|) (name,t:Type) =
      let mi = t.GetMethod(name)
      if mi <> null then Some(mi) else None

   let getSourceData (instance:obj, instanceType) (sourceName,sourceType:Type) =
      match (sourceName,sourceType) with
      | SourceProperty mi | SourceMethod mi->
         let instance = 
            if instanceType <> sourceType 
            then Activator.CreateInstance(sourceType) 
            else instance
         let result = mi.Invoke(instance, [||]) 
         result :?> IEnumerable
      | _ -> invalidOp "Expecting property or method"

module internal ParameterData =

   open SourceData

   module internal List =
       let rec combinations = function
       | [] -> [[]]
       | hs :: tss ->
           [ for h in hs do
               for ts in combinations tss ->
                   h :: ts ]

   let tryGetCustomAttribute<'TAttribute when 'TAttribute :> Attribute> (pi:ParameterInfo) =
      match pi.GetCustomAttribute(typeof<'TAttribute>, true) with
      | :? 'TAttribute as attr -> Some attr
      | _ -> None

   let (|Range|_|) = tryGetCustomAttribute<RangeAttribute>
   let (|Values|_|) = tryGetCustomAttribute<ValuesAttribute>
   let (|ValueSource|_|) = tryGetCustomAttribute<ValueSourceAttribute>

   let getParameterData instance (pi:ParameterInfo) =
      match pi with
      | Range range -> [for x in range.GetData(pi) -> x]
      | Values values -> [for x in values.GetData(pi) -> x]
      | ValueSource source ->
         let data = getSourceData instance (source.SourceName, source.SourceType)
         [for x in data -> x]
      | _ -> invalidOp "Expecting values"

module internal TestGeneration =

   open SourceData
   open ParameterData

   let (|Ignore|_|) (mi:MethodInfo) =
      if getCustomAttribute<IgnoreAttribute>(mi) <> null then Some() else None

   let (|TestCases|_|) (mi:MethodInfo) =
      let cases = mi.GetCustomAttributes(typeof<TestCaseAttribute>, true)
      if cases.Length > 0 then Some(cases |> Seq.cast<TestCaseAttribute>)
      else None

   let (|TestCaseSource|_|) (mi:MethodInfo) =
      let source = getCustomAttribute<TestCaseSourceAttribute>(mi)
      if source <> null then
         let sourceType = 
            if source.SourceType <> null then source.SourceType else mi.DeclaringType
         Some(source.SourceName, sourceType)
      else None

   let (|VanillaTest|_|) (mi:MethodInfo) =
      if getCustomAttribute<TestAttribute>(mi) <> null then Some() else None

   let toExpectedException (mi:MethodInfo) =
      let attr = getCustomAttribute<ExpectedExceptionAttribute>(mi)
      if attr <> null then Some attr.ExpectedException else None

   let fromCases (mi:MethodInfo) (cases:TestCaseAttribute seq) =
      let ex = toExpectedException(mi)
      [|for case in cases ->
         let expected = if case.HasExpectedResult then Some case.ExpectedResult else None
         Test(mi, case.Arguments, expected, ex)
      |]

   let fromData instance (mi:MethodInfo) (data:IEnumerable) =
      let ex = toExpectedException mi
      [|for item in data ->
         match item with
         | :? TestCaseData as case ->
            let expected = if case.HasExpectedResult then Some(case.Result) else None
            Test(mi, case.Arguments, expected, ex) 
         | :? (obj[]) as args -> Test(mi, args, None, ex)
         | arg -> Test(mi, [|arg|], None, ex)
      |]

   let fromValues instance (mi:MethodInfo) =
      [| let ps = mi.GetParameters()
         let argValues = [for pi in ps -> getParameterData instance pi]
         let ex = toExpectedException mi
         match List.combinations argValues with
         | [] -> yield Test(mi, [||], None, ex)
         | xs -> yield! [for args in xs -> Test(mi, List.toArray args, None, ex)]
      |]

   let generateTests instance (mi:MethodInfo) =
      let instance = instance, mi.DeclaringType
      match mi with
      | Ignore -> [||]
      | TestCases cases -> fromCases mi cases
      | TestCaseSource source -> getSourceData instance source |> fromData instance mi
      | VanillaTest -> fromValues instance mi
      | _ -> [||]

module internal TestRunner =

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

   let runTests instance (setUp,tearDown) (tests:Test[]) =
      printfn "1..%d" tests.Length
      tests |> Array.iteri (fun i test ->
         let result =
            try setUp (); runTest instance test
            finally tearDown ()
         result |> showResult (i+1) test 
      )

let Run (testType:Type) =
   let constr = testType.GetConstructor([||])
   let instance = if constr <> null then constr.Invoke([||]) else null
   let methods = testType.GetMethods()
   let tests = [|for mi in methods do yield! TestGeneration.generateTests instance mi|]
  
   let methodsWithAttribute attr =
      methods |> Array.filter (fun mi -> mi.GetCustomAttribute(attr, true) <> null)

   let runMethods (methods:MethodInfo[]) = 
      methods |> Array.iter (fun mi -> mi.Invoke(instance,[||]) |> ignore)

   let setUps = methodsWithAttribute typeof<SetUpAttribute>
   let tearDowns = methodsWithAttribute typeof<SetUpAttribute>
   let setUp () = setUps |> runMethods
   let tearDown () = tearDowns |> runMethods

   methodsWithAttribute typeof<TestFixtureSetUpAttribute> |> runMethods
   TestRunner.runTests instance (setUp, tearDown) tests
   methodsWithAttribute typeof<TestFixtureTearDownAttribute> |> runMethods