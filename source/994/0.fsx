//Declare some types
type Country = 
    |Germany    |Italy      |England    |Spain
    |France     |Portugal   |Turkey     |Scotland
    |Ukraine

type Group = |A|B|C|D|E|F|G|H

type Place = |First |Second

type Team = {
    name : string;
    country : Country;
    group : Group;
    place: Place;
}

//Declare all teams
let PSG = {name ="Paris Saint-Germain"; country=France; group=A;place=First}
let Porto = {name ="FC Porto"; country=Portugal; group=A; place=Second}

let Schalke = {name ="FC Schalke 04"; country=Germany; group=B;place=First}
let Arsenal = {name ="Arcenal FC"; country=England; group=B; place=Second}

let Malaga = {name ="Malaga CF"; country=Spain; group=C;place=First}
let Milan = {name ="AC Miland"; country=Italy; group=C; place=Second}

let Dortmund = {name ="Borussia Dortmund"; country=Germany; group=D;place=First}
let Real = {name ="Real Madrid CF"; country=Spain; group=D; place=Second}

let Juventus  = {name ="Juventus"; country=Italy; group=E;place=First}
let Shakhtar = {name ="FC Shakhtar Donetsk"; country=Ukraine; group=E; place=Second}

let Bayern  = {name ="FC Bayern München"; country=Germany; group=F;place=First}
let Valencia = {name ="Valemcia CF"; country=Spain; group=F; place=Second}

let Barcelona  = {name ="FC Barcelona"; country=Spain; group=G;place=First}
let Celtic = {name ="Celtic FC"; country=Scotland; group=G; place=Second}

let ManchesterU  = {name ="Manchester United FC"; country=England; group=H;place=First}
let Galatasaray = {name ="Galatasaray AŞ"; country=Turkey; group=H; place=Second}

//And put them in a list
let allTeams = [PSG; Porto; Schalke; Arsenal; Malaga; Milan; Dortmund; Real; Juventus;
    Shakhtar; Bayern; Valencia; Barcelona;Celtic;ManchesterU; Galatasaray;]
//Partition the list in group winners and runner ups
let groupWinners, runnerUps = 
    let isGroupWinner team  = (team.place = First)
    List.partition isGroupWinner allTeams;
//I stole this function from stack overflow to generate all permutaions of a list
let rec perms = 
    let distrib e L =
        let rec aux pre post = 
            seq {
                match post with
                | [] -> yield (L @ [e])
                | h::t -> yield (List.rev pre @ [e] @ post)
                          yield! aux (h::pre) t 
            }
        aux [] L
    function 
    | [] -> Seq.singleton []
    | h::t -> Seq.collect (distrib h) (perms t)

//Create all draws by using all permutations of the runner ups and pairing
//them with an unchanged list of gruop winners
let getDraws runnerUps  groupWinners = 
    [for perm in (perms runnerUps) do
        yield List.zip perm groupWinners
    ]

let draws = getDraws runnerUps  groupWinners

//fnction to check if a match is valid
let isValidMatch (footballMatch: Team*Team) = 
    match footballMatch with
    |(teamA,teamB) when teamA.group = teamB.group -> false
    |(teamA,teamB) when teamA.country = teamB.country  ->false
    |_ -> true
//checks if all matches in a draw are valid
let isValidDraw (draw: (Team*Team) list)  = 
    draw
    |>List.map isValidMatch 
    |>List.fold (&&) true
//filter the draws so we only keep the valid ones
let validDraws = List.filter isValidDraw draws
//Calculates the probability of a match by looking how many of the draws
//contain the match and dividing by the total amount of matches
let matchProbability (footballMatch:Team*Team) =  
    let occurences = 
        validDraws 
        |> List.filter (List.exists (fun (x:Team*Team) -> x= footballMatch)) 
        |> List.length

    (float occurences)*100.0/(float <| List.length validDraws)
//Gets a list of tupples where each tuple consist of the match and it respective probability           
let getProbabilities runnerUps groupWinners = 
    [for runnerUp in runnerUps do
        for adversary in groupWinners do
        let footbalMatch = (runnerUp,adversary)
        yield (footbalMatch, matchProbability footbalMatch)
    ]

let probabilities = getProbabilities runnerUps groupWinners

//a function to print a probability
let printProbability probabilities = 
    match probabilities with
    |((teamA,teamB),probability) -> printfn "%s  \t  %s   \t  %F%% " teamA.name teamB.name probability
//print all probabilities
List.iter printProbability probabilities