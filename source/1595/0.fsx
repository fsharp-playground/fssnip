open Microsoft.FSharp.Reflection

type TagUnionCase<'TTag> =  
  { CaseInfo: UnionCaseInfo
  ; Value: 'TTag
  }

type TagUnionCaseReflection<'TTag> =
    
    static member construct caseInfo =
      FSharpValue.MakeUnion(caseInfo, [||]) :?> 'TTag

    static member GetAll() = 
     
      FSharpType.GetUnionCases(typeof<'TagUnionType>)
      |> Array.map (fun c -> { CaseInfo = c; Value = TagUnionCaseReflection<'TTag>.construct c})

type Foos = Foo | Bar | Baz

TagUnionCaseReflection<Foos>.GetAll()