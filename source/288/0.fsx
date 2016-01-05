// The Model

type Candidate = string
type Preference = int
type Vote = (Candidate * Preference) list
type Election = Vote list

// The Code

let partitionCandidates result =
    let _,losingPercentage = List.minBy (fun (c,p) -> p) result
    List.partition (fun (c,p) -> p = losingPercentage) result
    |> (fun (ls, ws) -> List.unzip ls |> fst, List.unzip ws |> fst)

let remove losers election =
    let isLosing candidate = List.exists (fun loser -> candidate = loser) losers
    List.map (List.filter (fun (c,_) -> not <| isLosing c)) election

let firstPreference vote =
    let min cp1 cp2 = if (snd cp2) < (snd cp1) then cp2 else cp1
    List.fold min ("", System.Int32.MaxValue) vote
    |> fst

let add candidate candidateTotals =
    let incr ct = if candidate = fst ct then candidate, snd ct + 1 else ct
    List.map incr candidateTotals

let isThereAWinner result =
    List.exists (fun (_, percentage) -> percentage >= 50.) result

let firstPreferenceResult candidates election =
    let totalNumberOfVotes = float <| List.length election
    let initialTotals = List.map (fun c -> c,0) candidates
    let toPercentage (candidate,total) = candidate,(float total)/totalNumberOfVotes*100.0
    let addVote candidateTotals vote = add (firstPreference vote) candidateTotals
    List.fold addVote initialTotals election
    |> List.map toPercentage

let rec electionResult candidates election =
    let result = firstPreferenceResult candidates election

    if isThereAWinner result then
        printfn "final %A" result
    else
        let losers, winners = partitionCandidates result
        printfn "-- %A\n    winners %A losers %A" result winners losers
        electionResult winners (remove losers election)

// The Election Results

let castVote candidates preferences =
    List.zip candidates preferences

let candidates = ["a"; "b"; "c"; "d"]
let election = [
    castVote candidates [1;2;3;4]
    castVote candidates [1;2;3;4]
    castVote candidates [1;2;3;4]
    castVote candidates [1;2;3;4]
    castVote candidates [4;3;2;1]
    castVote candidates [4;3;2;1]
    castVote candidates [3;4;1;2]
    castVote candidates [3;4;1;2]
    castVote candidates [4;1;3;2]
    castVote candidates [3;4;2;1]
]

electionResult candidates election