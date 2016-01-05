
// Type defs and helper methods - basic oo stuffs
type State = 
| State of string * State seq 
and User =
| User of string * int

let getContigs state =
  match state with 
  | State(_,contigs) -> contigs

let getName state = 
  match state with
  | State(name, _) -> name

let getUserName user=
  match user with
  | User(name, _) -> name

let stateTaken state takenStates =
  let matches s =
    match s with
    | (s, u) when s = state -> true
    | _                     -> false
  Seq.exists matches takenStates

let printUser user =
  match user with
  | User(name, weight) -> printfn "User: %s %d" name weight

printfn "Initializing"

// Set up the data
let rec mo = State("mo", seq { yield il; yield ia;yield ne; yield ks; yield ok; yield ar; yield tn;yield ky }) 
and ne = State("ne", seq {yield ia; yield ks; yield mo})
and ok = State("ok", seq {yield ks;yield ar;yield mo})
and ar = State("ar", seq {yield ok; yield mo; yield tn})
and tn = State("tn", seq {yield ar; yield mo;yield ky})
and ky = State("ky", seq { yield tn; yield il; yield mo})
and ks = State("ks", seq {yield ne; yield ok; yield mo})
and il = State("il", seq { yield mo; yield ia; yield ky }) 
and ia = State("ia", seq { yield mo; yield il; yield ne;yield ks })
let allStates = [mo; ne;ok;ar;tn;ky;ks;il; ia]

let users = [User("josh", 2); User("nick" , 3); User("mark", 2)]

// Implementation 

let rec placeNextUser (state:State) (users:User list) takenStates =
  match users with 
  | []           -> printStates takenStates
  | user :: remainingusers -> placeUserOnState user state remainingusers takenStates

and placeUserOnState user state remainingusers takenStates =
  let takenStates = (state, user) :: takenStates
  match user with
  | User(_, weight) when weight = 1 -> nextOpenState state remainingusers takenStates
  | User(name, weight)              -> nextOpenState state (User(name, weight-1) :: remainingusers) takenStates

and nextOpenState state (users:User list ) takenStates =

  let contigs = getContigs state
  let state = Seq.tryFind (fun state-> (stateTaken state takenStates) = false) contigs
  match state with
  | Some(state) -> placeNextUser state users takenStates
  | None -> let state = Seq.find (fun state -> (stateTaken state takenStates) = false) allStates
            placeNextUser state users takenStates

and printStates takenStates : unit = 
  match takenStates with
  | (state, user)::rest -> printfn "%s was taken by %s" (getName state) (getUserName user)
                           printStates rest
  | []                  -> ()
  



placeNextUser mo users []  