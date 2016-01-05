// [snippet: Processing chunks of input with agents]
type Agent<'a> = MailboxProcessor<'a>

type State<'a> =
  | Continue of ('a -> State<'a>)
  | Done of 'a

type Message<'a> =
  | Result of 'a
  | NeedInput
  | Error of string

let pong f = Agent<string * (Message<_> -> unit)>.Start(fun inbox ->
  let rec loop f = async {
    let! msg = inbox.Receive()
    match msg with
    | m, cont ->
        match f m with
        | Done x -> cont <| Result x
        | Continue f' ->
            cont NeedInput
            return! loop f'
  }
  loop f )

let rec ping (target1: Agent<_>) (target2: Agent<_>) = Agent<Message<_>>.Start(fun inbox ->
  let target = ref target1
  let state = ref ""
  async {
    for x = 1 to 10 do
      (!target).Post(x.ToString(), inbox.Post)
      let! msg = inbox.Receive()
      match msg with
      | Result v ->
          target := target2
          state := v
      | Error e -> System.Console.WriteLine e
      | _ -> ()
      System.Console.WriteLine msg

    System.Console.WriteLine "Sending \"\""
    (!target).Post("", inbox.Post)
    let! result = inbox.Receive()
    System.Console.WriteLine result
    match result with
    | Result x ->
        System.Console.WriteLine !state
        System.Console.WriteLine x
    | Error x -> System.Console.WriteLine x
    | _ -> System.Console.WriteLine "Something went wrong"
  })

let take n =
  let rec step count state (str: string) =
    System.Console.WriteLine("Received " + str)
    if str = "" then
      Done state
    elif count < n then
      Continue <| step (count + 1) (state + str)
    else Done (state + str)
  if n <= 0 then
    fun _ -> Done "" // Effectively skip the input
  else step 0 ""
// [/snippet]

// [snippet: Usage]
let f1 = pong <| take 2
let f2 = pong <| take 10
ping f1 f2
// Received 1
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 2
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 3
// FSI_0085+Message`1+Result[System.String]
// Received 4
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 5
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 6
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 7
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 8
// // FSI_0085+Message`1+_NeedInput[System.String]
// Received 9
// FSI_0085+Message`1+_NeedInput[System.String]
// Received 10
// FSI_0085+Message`1+_NeedInput[System.String]
// Sending ""
// Received 
// FSI_0085+Message`1+Result[System.String]
// 123
// 45678910
// [/snippet]