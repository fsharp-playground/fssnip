type t = (unit -> unit) * (unit -> unit)

type Binder<'a> = ('a -> t)*(t -> 'a option)

let embed<'a> (): Binder<'a> =
    let r = ref None
    let put x = (fun () -> r := Some x), (fun () -> r := None) 
    let get (f, g) = f (); let res = !r in g (); res in
    put, get

let binder<'a> : Binder<'a> = embed ()

let (of_int, to_int) = binder<int>
let (of_string, to_string) = binder<string>
let (of_float, to_float) = binder<float>

let heterogenous_list = [ of_int 3; of_float 4. ; of_int 5; of_int 7 ; of_string "F#" ]
    
let just_floats = List.choose to_float heterogenous_list
let just_ints =  List.choose to_int heterogenous_list