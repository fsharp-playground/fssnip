// [snippet:Printing None results in &lt;null&gt;]
None |> printfn "Value: %A"
// Value: <null>
// [/snippet]

// [snippet:Custom implementation of Option&lt;'T&gt; - Printing None results in None]
type MyOption<'T> =
| Some of 'T
| None

MyOption.None |> printfn "Value: %A"
// Value: None
// [/snippet]

// [snippet:Revised MyOption&lt;'T&gt; - Printing None results in &lt;null&gt;]
// Here, MyOption2<'T> is decorated with CompilationRepresentationAttribute and
// the UseNullAsTrueValue flag is set just like it is for Option<'T> in the F#
// library. This is how None is represented in the compiled code.
// See http://bit.ly/1cA0QfS for more info about CompilationRepresentation
[<CompilationRepresentation(CompilationRepresentationFlags.UseNullAsTrueValue)>]
type MyOption2<'T> =
| Some of 'T
| None

MyOption2.None |> printfn "Value: %A"
// Value: <null>
// [/snippet]