open System

// Queue / Server is either Idle, 
// or Busy until a certain time, 
// with items queued for processing
type Status = Idle | Busy of DateTime * int

type State = 
    { Start: DateTime;
      Status: Status; 
      NextIn: DateTime }

let next arrival processing state =
   match state.Status with
   | Idle ->
      { Start = state.NextIn;
        NextIn = state.NextIn + arrival();
        Status = Busy(state.NextIn + processing(), 0) }
   | Busy(until, waiting) ->
      match (state.NextIn <= until) with
      | true -> 
            { Start = state.NextIn; 
              NextIn = state.NextIn + arrival();
              Status = Busy(until, waiting + 1) } 
      | false -> 
            match (waiting > 0) with
            | true -> 
               { Start = until; 
                 Status = Busy(until + processing(), waiting - 1); 
                 NextIn = state.NextIn }
            | false -> 
               { Start = until; 
                 Status = Idle; 
                 NextIn = state.NextIn }

let simulate startTime arr proc =
   let nextIn = startTime + arr()
   let state = 
      { Start = startTime; 
        Status = Idle;
        NextIn = nextIn }
   Seq.unfold (fun st -> 
      Some(st, next arr proc st)) state

let pretty state =
   let count =
      match state.Status with
      | Idle -> 0
      | Busy(_, waiting) -> 1 + waiting
   let nextOut =
      match state.Status with
      | Idle -> "Idle"
      | Busy(until, _) -> until.ToLongTimeString()
   let start = state.Start.ToLongTimeString()
   let nextIn = state.NextIn.ToLongTimeString()
   printfn "Start: %s, Count: %i, Next in: %s, Next out: %s" start count nextIn nextOut

let constantTime (interval: TimeSpan) = 
   let ticks = interval.Ticks
   fun () -> interval

let arrivalTime = new TimeSpan(0,0,10);
let processTime = new TimeSpan(0,0,5)

let simpleArr = constantTime arrivalTime
let simpleProc = constantTime processTime

let startTime = new DateTime(2010, 1, 1)
let constantCase = simulate startTime simpleArr simpleProc

printfn "Constant arrivals, Constant processing"
Seq.take 10 constantCase |> Seq.iter pretty;;

let uniformTime (seconds: int) = 
   let rng = new Random()
   fun () ->
      let t = rng.Next(seconds + 1) 
      new TimeSpan(0, 0, t)

let uniformArr = uniformTime 10
let uniformCase = simulate startTime uniformArr simpleProc

printfn "Uniform arrivals, Constant processing"
Seq.take 10 uniformCase |> Seq.iter pretty;;

let exponentialTime (seconds: float) =
   let lambda = 1.0 / seconds
   let rng = new Random()
   fun () ->
      let t = - Math.Log(rng.NextDouble()) / lambda
      let ticks = t * (float)TimeSpan.TicksPerSecond
      new TimeSpan((int64)ticks)

let expArr = exponentialTime 10.0
let expProc = exponentialTime 7.0
let exponentialCase = simulate startTime expArr expProc

printfn "Exponential arrivals, Exponential processing"
Seq.take 10 exponentialCase |> Seq.iter pretty;;

let averageCountIn (transitions: State seq) =
   // time spent in current state, in ticks
   let ticks current next =
      next.Start.Ticks - current.Start.Ticks
   // jobs in system in state
   let count state =
      match state.Status with
      | Idle -> (int64)0
      | Busy(until, c) -> (int64)c + (int64)1
   // update state = total time and total jobsxtime
   // between current and next queue state
   let update state pair =
      let current, next = pair
      let c = count current
      let t = ticks current next
      (fst state) + t, (snd state) + (c * t)     
   // accumulate updates from initial state
   let initial = (int64)0, (int64)0
   transitions
   |> Seq.pairwise
   |> Seq.scan (fun state pair -> update state pair) initial
   |> Seq.map (fun state -> (float)(snd state) / (float)(fst state))

let averageTimeIn  (transitions: State seq) =
   // time spent in current state, in ticks
   let ticks current next =
      next.Start.Ticks - current.Start.Ticks
   // jobs in system in state
   let count state =
      match state.Status with
      | Idle -> (int64)0
      | Busy(until, c) -> (int64)c + (int64)1
   // count arrivals
   let arrival current next =
      if count next > count current then (int64)1 else (int64)0
   // update state = total time and total arrivals
   // between current and next queue state
   let update state pair =
      let current, next = pair
      let c = count current
      let t = ticks current next
      let a = arrival current next
      (fst state) + a, (snd state) + (c * t)     
   // accumulate updates from initial state
   let initial = (int64)0, (int64)0
   transitions
   |> Seq.pairwise
   |> Seq.scan (fun state pair -> update state pair) initial
   |> Seq.map (fun state -> 
      let time = (float)(snd state) / (float)(fst state)
      new TimeSpan((int64)time))

// turnstiles admit 1 person / 4 seconds
let turnstileProc = exponentialTime 4.0
// passengers arrive randomly every 5s
let passengerArr = exponentialTime 5.0

let batchedTime seconds batches = 
   let counter = ref 0
   fun () ->
      counter := counter.Value + 1
      if counter.Value < batches
      then new TimeSpan(0, 0, 0)
      else 
         counter := 0
         new TimeSpan(0, 0, seconds)
// trains arrive every 30s with 5 passengers
let trainArr = batchedTime 30 6

// passengers arriving in station
let queueIn = simulate startTime passengerArr turnstileProc
// passengers leaving station
let queueOut = simulate startTime trainArr turnstileProc

let prettyWait (t:TimeSpan) = t.TotalSeconds

printfn "Turnstile to get in the Station"
averageCountIn queueIn |> Seq.nth 1000000 |> printfn "In line: %f"
averageTimeIn queueIn |> Seq.nth 1000000 |> prettyWait |> printfn "Wait in secs: %f"

printfn "Turnstile to get out of the Station"
averageCountIn queueOut |> Seq.nth 1000000 |> printfn "In line: %f"
averageTimeIn queueOut |> Seq.nth 1000000 |> prettyWait |> printfn "Wait in secs: %f"