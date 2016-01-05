// Dynamic libray for F#.
// Syntax: obj?FieldOrProperty() => obj 
//         obj?Property1?SubProperty2?MethodName1(arg)?MethodName2()?FieldOrProperty() => obj
//         obj?MethodName(argument1,argement2,...) => obj or null
//         obj?FieldOrPropertyName<-Value => unit
open System
open System.Reflection
open System.Text.RegularExpressions

let rec (?) (ob:obj) name=
  let (|Tuple|_|) o=
    Regex.Match(o.GetType().Name,"^Tuple`(\d*)")|>function
      |m when m.Success->{1..m.Groups.[1].Value|>int}
                           |>Seq.map (fun x->((?) o ("Item"+string(x))(null)))
                           |>Seq.toArray|>Some
      |_->None
  let x=ob|>function
          | :? (obj->obj) as x->x()
          |x->x
  let tp=x.GetType()
  (fun (arg:obj) ->let ar=arg|>function
                           |null->[||]
                           |Tuple(arr)->arr
                           |_->[|arg|]
                   let fn2 bn=tp.InvokeMember(name,bn,null,x,ar)
                   try fn2 BindingFlags.GetProperty
                   with _ ->try fn2 BindingFlags.GetField
                            with _ -> fn2 BindingFlags.InvokeMethod)

let (?<-) (ob:obj) name arg=
  let x=ob|>function
          | :? (obj->obj) as x->x()
          |x->x
  let fn bn=x.GetType().InvokeMember(name,bn,null,x,[|arg|])|>ignore
  try fn BindingFlags.SetProperty
  with _ -> fn BindingFlags.SetField

// Example 1
//open System.Windows.Forms
//let f=new Form():>obj
//let tb=new TextBox():>obj
//let x=125:>obj
//x?MinValue() // !!!! Warning adding "()" symbols after last field or property
//f?Show()
//f?SetDesktopLocation(100,200)
//tb?Parent<-f
//f?Controls?Item(0)?Text<-"Check"
//tb?Text()

// Example 2
//let excel=Activator.CreateInstance(Type.GetTypeFromProgID("excel.application"))
//excel?Visible<-true
//let wb=excel?Workbooks?Add()
//let sheet=wb?Worksheets?Item(1)
//sheet?Range("a1")?Value<-3
//sheet?Range("b1")?Value<-4
//sheet?Range("c1")?Value<-"=a1+b1"
//sheet?Range("c1")?Text()
//excel?Quit()