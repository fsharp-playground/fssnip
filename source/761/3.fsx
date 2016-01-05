open System

// given a roll between 0 and 1
// and a distribution D of 
// probabilities to end up in each state
// returns the index of the state
let state (D: float[]) roll =
   let rec index cumul current =
      let cumul = cumul + D.[current]
      match (roll <= cumul) with
      | true -> current
      | false -> index cumul (current + 1)
   index 0.0 0

// given the transition matrix P
// the index of the current state
// and a random generator,
// simulates what the next state is
let nextState (P: float[][]) current (rng: Random) =
   let dist = P.[current]
   let roll = rng.NextDouble()
   state dist roll

// given a transition matrix P
// the index i of the initial state
// and a random generator
// produces a sequence of states visited
let simulate (P: float[][]) i (rng: Random) =
   Seq.unfold (fun s -> Some(s, nextState P s rng)) i

// Vector dot product
let dot (V1: float[]) (V2: float[]) =
   Array.zip V1 V2
   |> Array.map(fun (v1, v2) -> v1 * v2)
   |> Array.sum

// Extracts the jth column vector of matrix M 
let column (M: float[][]) (j: int) =
   M |> Array.map (fun v -> v.[j])

// Given a row-vector S describing the probability
// of each state and a transition matrix P, compute
// the next state distribution
let nextDist S P =
   P 
   |> Array.mapi (fun j v -> column P j)
   |> Array.map(fun v -> dot v S)

// Euclidean distance between 2 vectors
let dist (V1: float[]) V2 =
   Array.zip V1 V2
   |> Array.map(fun (v1, v2) -> (v1 - v2) * (v1 - v2))
   |> Array.sum
   
// Evaluate stationary distribution
// by searching for a fixed point
// under tolerance epsilon
let stationary (P: float[][]) epsilon =
   let states = P.[0] |> Array.length
   [| for s in 1 .. states -> 1.0 / (float)states |] // initial
   |> Seq.unfold (fun s -> Some((s, (nextDist s P)), (nextDist s P)))
   |> Seq.map (fun (s, s') -> (s', dist s s'))
   |> Seq.find (fun (s, d) -> d < epsilon)

// Illustration

// Our Markov chain models plane delays
// 0 = early, 1 = on-time, 2 = delayed
// more comments at 
// http://clear-lines.com/blog/post/Simple-Markov-chains-in-FSharp.aspx

// Transition matrix:
// each row corresponds to a state,
// and contains an array of probabilities
// to transition to each of the states.
let P = 
   [| 
      [| 0.10; 0.85; 0.05 |];
      [| 0.10; 0.75; 0.15 |];
      [| 0.05; 0.60; 0.35 |]
   |]

let rng = new Random()

// how many delays in a sequence of 1000 flights?
let flights = simulate P 1 rng

let delays = 
   Seq.take 1000 flights 
   |> Seq.filter (fun i -> i = 2) 
   |> Seq.length

// stationary distribution
let longterm = stationary P 0.0001

// impact of matrix modification

// improve delays after delays
let strat1 = 
   [| 
      [| 0.10; 0.85; 0.05 |];
      [| 0.10; 0.75; 0.15 |];
      [| 0.05; 0.61; 0.34 |]
   |]

// improve delays after on-time
let strat2 = 
   [| 
      [| 0.10; 0.85; 0.05 |];
      [| 0.10; 0.76; 0.14 |];
      [| 0.05; 0.60; 0.35 |]
   |]

let impact1 = stationary strat1 0.0001
let impact2 = stationary strat2 0.0001