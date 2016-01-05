// [snippet:Declaration of the using combinator]
open System

module Event = 
  /// Generates new values using the specified function 'f' (just like
  /// 'Event.map'), but automatically diposes of the previous value when 
  /// a new value is generated.
  let using f evt = 
    evt |> Event.scan (fun st inp ->
      // Dipose of the previous value if it is not 'null'
      if st <> Unchecked.defaultof<_> then 
        (st :> IDisposable).Dispose()
      f inp ) Unchecked.defaultof<_>
// [/snippet]

// [snippet:Windows Forms example]
open System.Drawing
open System.Windows.Forms

let colors = (*[omit:(...)]*)failwith "Not implemented" : IEvent<Color>(*[/omit]*)
let frm = (*[omit:(...)]*)failwith "Not implemented" : Form(*[/omit]*)

// Turns colors into brushes, but automatically disposes 
// of the previous brush when creating a new one
colors 
  |> Event.using (fun clr -> new SolidBrush(clr))
  |> Event.add (fun br -> frm.BackBrush <- br)
// [/snippet]