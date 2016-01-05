type Country =
    | Albania
    | Montenegro
    | Romania
    | Austria
    | Ukraine
    | Belarus
    | Belgium
    | Azerbaijan
    | Malta
    | SanMarino
    | France
    | UnitedKingdom
    | Turkey
    | Greece
    | BosniaHerzogovina
    | Moldova
    | Bulgaria
    | Switzerland
    | Slovenia
    | Cyprus
    | Croatia
    | Solvakia
    | Macedonia
    | Netherlands
    | Portugal
    | Iceland
    | Sweden
    | Norway
    | Lithuania
    | Estonia
    | Denmark
    | Ireland
    | Latvia
    | Spain
    | Finland
    | Georgia
    | Italy
    | Serbia
    | Germany
    | Russia
    | Hungary
    | Israel

type Awards = {Giver : Country; Recipients : List<Country>}

type Received = {Recipient : Country; Score : int}

let AwardPoints (results : List<Awards>) =
    results
    |> List.map (fun result -> result.Recipients
                               |> List.mapi (fun i recipient -> {Recipient = recipient; Score = 12-i}))
    |> List.concat
    |> Seq.ofList
    |> Seq.groupBy (fun received -> received.Recipient)
    |> Seq.map (fun (country, awards) -> {Recipient = country;
                                          Score = awards |> Seq.sumBy (fun award -> award.Score)})
    |> List.ofSeq

let resultsAwarded =
    [
        {Giver = Albania; Recipients = [Spain; France; Sweden]}
        {Giver = Montenegro; Recipients = [Sweden; Greece; Albania]}
        {Giver = Romania; Recipients = [Germany; Russia; Serbia]}
        {Giver = Austria; Recipients = [Georgia; Spain; Sweden]}
        {Giver = Ukraine; Recipients = [Finland; Latvia; Lithuania]}
        {Giver = Belarus; Recipients = [Iceland; Portugal; Netherlands]}
        {Giver = Belgium; Recipients = [Iceland; France; Israel]}
        {Giver = Azerbaijan; Recipients = [Azerbaijan; France; Georgia]}
        {Giver = Malta; Recipients = [Albania; Portugal; Cyprus]}
        // etc etc.
    ]

let Outcomes =
    AwardPoints resultsAwarded
    |> List.sortBy (fun received -> -received.Score)

