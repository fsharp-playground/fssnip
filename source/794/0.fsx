// **********************************************************************
// 
//          Airplane passenger unloading simulator.
//             Author: Max Galkin (yacoder.net)
//                        June, 2012            
//
//    INPUT: see airplane model parameters in the 'main' function.
//    OUTPUT: tabulated average unload time and stress statistics.
//
// **********************************************************************


open Microsoft.FSharp.Collections;

// Our airplane model is a rectangular grid.
type Coordinate = { row : int; column : int }

// Airplane is filled with people, who try to reach the exit as soon as possible.
// When they first step into the corridor they need to reach for their luggage and so they block way for others.
type PersonState =
    | Idle              // Person waiting for an empty position to move into.
    | Busy of int       // Person busy for the given amount of turns, blocks way for other people.

type Person = { state : PersonState; id : int }

// Each position in the airplane model grid is one of these kinds.
type PositionKind =
    | Seat
    | Corridor

type Position = { 
        kind : PositionKind;        // See above.

        exit : Option<Coordinate>;  // Coordinate of the next position on the way to the exit.
                                    // If it is None -- the person has reached the airplane's exit. Limbo. You win.

        person: Option<Person>      // Person occupying this position. Can be None.
    }

// The airplane model type -- a matrix of positions defined above.
type AirplaneModel = Position[,]

// A record defining behavior of people in the airplane.
//   p -- probability of giving way to a person standing in the corridor behind your seat row.
//   N -- a person reaching for his luggage blocks the corridor for randomly chosen number of zero to N turns.
type Behavior = { p: double; N: int }

// For the given Behavior each turn of our simulation will output:
//   model -- airplane model with updated passenger position information;
//   stress -- amount of stress generated in that turn (a person is stressed if it s(he) is idle and not moving towards the exit this turn);
//   finalTurn -- true if the last passenger has left the airplane.
type TransformationResult = { model: AirplaneModel; stress : int; finalTurn: bool }

// *********************************************************************************************************
// A group of functions generating random numbers.
// We ask for an external random generator instead of using a global one, because Random is not thread-safe.
// Main algorithm will take care of creating one Random instance per thread.

// Random double in interval [0.0 ; 1.0]
let randomZeroToOne (random : System.Random) = random.NextDouble();
// Random integer in interval [0 ; N]
let randomZeroToN (random : System.Random) N = random.Next(N+1);
// Random choice from two given values
let randomChoice (random : System.Random) A B = if (randomZeroToOne random < 0.5) then A else B
// Choose first value from tuple with probability p, second value with probability (1-p)
let fstWithProbabilityP (random : System.Random) (A, B) p = if (randomZeroToOne random < p) then A else B

// *********************************************************************************************************

// Simulate next turn of the given current model for the given Behavior variables and return the upated airplane model.
// The transformation is performed according to the following rules:
//   * idle persons try to step into empty positions moving toward the exit, they generate stress if they can't move;
//   * persons stepping into the corridor become busy for a random number of turns (reaching for their freaking luggage);
//   * busy persons in the corridors block way;
//   * an idle person in the exit position goes to the limbo.
//
// Note that we make some assumptions about the topology of the airplane grid:
//  * we expect only one corridor and 2 sides with equal number of seats on each side.
let transform (random : System.Random) (behavior : Behavior) (model : AirplaneModel) =
    let rows = Array2D.length1 model
    let columns = Array2D.length2 model
    let seatsOnEachSide = columns / 2;
    
    // **************************************************************************************************************************************************
    // Determine who will step into an empty corridor position, one of the passengers from the seat row, or a person in the corridor behind the seat row.
    //
    //  ###I###
    //  ##I.I##
    //
    // A person in the corridor behind (if he is Idle) will step in with probability p.
    // Otherwise there is an equal chance that one of the two row seat passengers will step in.
    let getNewOccupant p N corridorCandidate (rowCandidate1 : Option<Person>) (rowCandidate2 : Option<Person>) =
        // make sure corridor candidate is Idle
        let corridorCandidateEffective = Option.bind(fun person -> if (person.state = Idle) then Some(person) else None) corridorCandidate
        
        // row candidates will become busy if they step into corridor
        let makeCandidateBusy = Option.bind(fun person -> Some({ person with state = Busy(randomZeroToN random N)}))
        let rowCandidate1Effective = makeCandidateBusy rowCandidate1
        let rowCandidate2Effective = makeCandidateBusy rowCandidate2

        match (corridorCandidateEffective, rowCandidate1Effective, rowCandidate2Effective) with
            | (None, None, None) -> None
            | (None, None, Some(person)) | (None, Some(person), None) -> Some(person)
            | (None, Some(person1), Some(person2)) -> Some(randomChoice random person1 person2)
            | (Some(personC), None, None) -> Some(personC)
            | (Some(personC), None, Some(person)) -> Some(fstWithProbabilityP random (personC, person) p)
            | (Some(personC), Some(person), None) -> Some(fstWithProbabilityP random (personC, person) p)
            | (Some(personC), Some(person1), Some(person2)) -> Some(fstWithProbabilityP random (personC, randomChoice random person1 person2) p)
    //
    // **************************************************************************************************************************************************

    // Look through empty positions and determine who is stepping into them.
    // We need to pre-compute this information to create the upated model.
    // We use the getNewOccupant function defined above for empty corridor positions.
    let newOccupant = Array2D.init rows columns (fun i j ->
        match model.[i,j] with
            | { kind = Corridor; person = None } when i=0 -> getNewOccupant behavior.p behavior.N None model.[i, j-1].person model.[i, j+1].person
            | { kind = Corridor; person = None } -> getNewOccupant behavior.p behavior.N model.[i-1, j].person model.[i, j-1].person model.[i, j+1].person
            | { person = None } when (0 < j) && (j < seatsOnEachSide) -> model.[i,j-1].person
            | { person = None } when (seatsOnEachSide < j) && (j < columns-1) -> model.[i,j+1].person
            | _ -> None
    )

    // Now we have everything to create the updated model.
    let newModel = Array2D.init rows columns (fun i j ->
        match model.[i,j] with
            // a busy person keeps blocking the way
            | { person = Some({ state = Busy(0) }) } -> { model.[i,j] with person = Some({ model.[i,j].person.Value with state = Idle }) }
            | { person = Some({ state = Busy(turns) }) } -> { model.[i,j] with person = Some({ model.[i,j].person.Value with state = Busy(turns-1) }) }

            // an Idle person in the exit position goes to limbo
            | { exit = None; person = Some({ state = Idle }) } -> { model.[i,j] with person = None }  // Honey, I'm home!
            
            // all other cases involving an idle person
            | { exit = Some(coordinate); person = Some({ state = Idle; id = id }) } ->
                match model.[coordinate.row, coordinate.column].kind with
                    // an Idle person will always make a move into an empty seat
                    | Seat when model.[coordinate.row, coordinate.column].person.IsNone -> { model.[i,j] with person = None }
                    // an Idle person will not move into a taken seat position (no, never!)
                    | Seat -> model.[i,j]
                    // for corridor positions use the precomputed newOccupant value to determine who is stepping into it
                    | Corridor -> 
                        if (Option.map (fun el -> el.id) newOccupant.[coordinate.row, coordinate.column]) = Some(id) then
                            // an Idle person may make a move if newOccupant id matches with its id
                            { model.[i,j] with person = None }
                        else
                            // an Idle person stays in the same position
                            model.[i,j]
            
            // an empty position may become taken or stay empty (use precomputed newOccupant)
            | { person = None } -> { model.[i,j] with person = newOccupant.[i,j] }
    )

    let enumeratePositions (model : AirplaneModel) = seq { for i in 0..rows-1 do for j in 0..columns-1 do yield model.[i,j] }
    let positionWithAPerson (position : Position) = position.person.IsSome
    let isStressedPosition (p1 : Position) (p2 : Position) =
        match (p1.person, p2.person) with
            | Some( { state = Idle; id = id1} ), Some( { state = Idle; id = id2} ) when id1 = id2 -> 1
            | _ -> 0

    // calculate how many Idle people stayed in their position producing stress
    let stress = Seq.map2 (isStressedPosition) (enumeratePositions model) (enumeratePositions newModel) |> Seq.sum
    let isAnyoneLeftOnBoard = enumeratePositions newModel |> Seq.exists positionWithAPerson 
    { model = newModel; stress = stress; finalTurn = not isAnyoneLeftOnBoard }



// Generate initial airplane model filled with people.
// Airplane exit is at the bottom row, seating rows are horizontal.
//
// ###.###    row 0
// ###.###    row 1
// ###.###    row 2
// ###.###    row 3
//    |
//   exit
//
let oneCorridorAirplaneModel numberOfRows seatsOnEachSide =
    let isCorridor column = (column = seatsOnEachSide)
    let rowSize = seatsOnEachSide*2+1
    Array2D.init numberOfRows rowSize (fun i j ->
        {   kind = if (isCorridor j) then Corridor else Seat;
            exit = match j with
                    // it is a corridor position, we need to move down unless it is the last row 
                    | column when isCorridor column -> 
                        if(i < numberOfRows-1) then Some({ row = i+1; column = j }) else None
                    // we need to move left or right depending on the side of the corridor
                    | _ -> if(j < seatsOnEachSide) then Some({ row = i; column = j+1 }) else Some({ row = i; column = j-1 });
            person = if (isCorridor j) then None else Some({ state = Idle; id = rowSize*i + j })
        }
    )

// Outputs airplane model into the console.
let debugPrintAirplaneModel model =
    let positionToChar (position : Position) =
        match (position.kind, position.person) with
            | (Seat, None) -> '#'                                           // empty seat
            | (Seat, _) -> 'I'                                              // idle person in a seat
            | (Corridor, None) -> '.'                                       // empty corridor
            | (Corridor, Some(person)) when person.state = Idle -> 'I'      // idle person in the corridor
            | (Corridor, Some(person)) -> 'B'                               // busy person in the corridor
    let rows = Array2D.length1 model
    let columns = Array2D.length2 model
    for row in 0 .. rows-1 do
        for column in 0 .. columns-1 do
            printf "%c" (positionToChar model.[row, column])
        printfn ""

// Runs one full simulation.
// Unfolds transformations until the airplane is unloaded, then folds the sequence back to collect aggregated stress and count number of turns.
let runSimulationOnce transformation airplaneModel =
    let stressSequnceUnfolder = Seq.unfold (fun state -> 
        match state with
            | { finalTurn = true } -> None      // stops unfolding
            | { model = model } ->              // new turn
                let result = transformation model
                // printfn "New turn"
                // debugPrintAirplaneModel result.model
                Some(result.stress, result) ) 
            
    stressSequnceUnfolder { model = airplaneModel; stress = 0; finalTurn = false; } 
            |> Seq.fold (fun (turn, stress) el -> (turn + 1, stress + el)) (0,0)


// Prints final results of multiple simulation runs.
// Outputs raw data -- results for each run, then outputs data grouped by p and N with averaged stress and durations.
// Finally outputs 2 tables for duration and stress ready to be pasted into Excel to create a 3D chart of stress and duration.
let printResults results (pInterval : float[]) nInterval =
    printfn "All runs"
    printfn "p\tN\tDuration\tStress"
    results |> Seq.iter (Seq.iter (fun el -> printfn "%f\t%d\t%d\t%d" (fst (fst el)) (snd (fst el)) (fst (snd el)) (snd (snd el))))

    printfn "Grouped average"
    printfn "p\tN\tAvg(D)\tAvg(S)"
    results |> Seq.iter (fun el -> printfn "%f\t%d\t%f\t%f" (fst (fst (el |> Seq.head))) (snd (fst (el |> Seq.head))) (el |> Seq.averageBy (snd >> fst >> float)) (el |> Seq.averageBy (snd >> snd >> float)) )

    let Nk = nInterval |> Array.length;

    printfn ""
    printfn "2D duration table"
    for N in nInterval do printf "\t%d" N

    results 
        |> Seq.iteri (fun i el -> 
                if ((i % Nk) = 0) then
                    printfn ""
                    printf "%f\t" pInterval.[i/Nk]

                printf "%f\t" (el |> Seq.averageBy (snd >> fst >> float))
            )

    printfn ""

    printfn "2D stress table"
    for N in nInterval do printf "\t%d" N

    results 
        |> Seq.iteri (fun i el -> 
                if ((i % Nk) = 0) then
                    printfn ""
                    printf "%f\t" pInterval.[i/Nk]

                printf "%f\t" (el |> Seq.averageBy (snd >> snd >> float))
            )
    



[<EntryPoint>]
let main argv = 

  
    //*****************************************
    //*            MODEL PARAMETERS           *
    //*****************************************
    
    let airplaneModel = oneCorridorAirplaneModel 40 3;      // Airplane model: 40 rows with 3 seats on each side of the central corridor
    let pInterval = [| 0.0 .. 0.05 .. 1.0 |]                // p -- probability of giving way to a person behind you in corridor
    let NInterval = [| 0 .. 5 .. 100 |]                     // N -- after stepping into the corridor a person blocks way for 0 to N turns
    let kIterations = 30                                    // number of iterations of simulations with the same p and N value to calculate averages

    // ****************************************

    let stopwatch = new System.Diagnostics.Stopwatch();
    stopwatch.Start();

    let results = 
        seq { 
            for p in pInterval do
                for N in NInterval do
                    yield async { 
                        let random = new System.Random()
                        let transformation = transform random { p = p; N = N }
                        printfn "Calculations for [ p=%f   N=%d ] started" p N
                        return seq { 
                            for i in 1 .. kIterations do yield ((p, N), runSimulationOnce transformation airplaneModel) 
                            } |> Seq.toList 
                    } 
            }
            |> Async.Parallel 
            |> Async.RunSynchronously

    printResults results pInterval NInterval

    stopwatch.Stop();
    
    printfn ""
    printfn "Elapsed %d ms" stopwatch.ElapsedMilliseconds
    
    0 // return an integer exit code
