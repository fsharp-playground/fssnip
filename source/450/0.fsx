type IFoobar =
  member ConvertToBar : string -> unit

type MyFoobar () =
  let converter = fun s -> ()
  interface IFoobar with
    member this.ConvertToBar = converter