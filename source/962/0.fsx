module ChoAndGaines

open System

// Cho WKT, Gaines BJ (2007) Breaking the (Benford) law: Statistical fraud detection in campaign ﬁnance. 
// Amer Stat 61, 218–223

/// Observed frequency in a 'bin'.
let e i s =
    let n = s |> Seq.length |> float
    let startsi = s |> Seq.filter (fun x -> let digit = Int32.Parse(x.ToString().[0..0])
                                            digit = i)
                    |> Seq.length |> float
    startsi / n

/// Probability of appearing in a 'bin' according to Benford's law.
let b i =
    Math.Log10(1. + 1./(i |> float))
    
/// Cho and Gaines distance.
let d s =
    [1..9]
    |> Seq.map (fun i -> (b i - (e i s)) ** 2.)
    |> Seq.sum
    |> Math.Sqrt

/// Cho and Gaines distance times square root of data count. (d*)
let d' s =
    let n2 = s |> Seq.length |> float |> sqrt
    n2 * d s

/// Interpret the d* statistic based on critical values established by Morrow.
///
/// From http://www.johnmorrow.info/projects/benford/benfordMain.pdf
let interpret d' =
    let limits = 
        [
            "α 0.1", 1.212 
            "α 0.05", 1.330 
            "α 0.01", 1.569
        ]
    let below, above = 
        limits 
        |> Array.ofSeq 
        |> Array.partition (fun (_, t) -> d' < t)

    let belowStr, aboveStr =

        below 
        |> Array.map (fun (name, _) -> name)
        |> Array.fold (fun acc name -> sprintf "%s %s" acc name) "Below: ",

        above 
        |> Array.map (fun (name, _) -> name)
        |> Array.fold (fun acc name -> sprintf "%s %s" acc name) "Above: "

    belowStr, aboveStr

// Usage examples:

// ("Below: ", "Above:  α 0.1 α 0.05 α 0.01")
let randomTest =
    let r = new Random()
    [1..1000] 
    |> Seq.map (fun _ -> r.Next())
    |> d'
    |> interpret

// ("Below:  α 0.1 α 0.05 α 0.01", "Above: ")
let fileSizeTest =
    System.IO.Directory.EnumerateFiles(@"c:\windows", "*.*", System.IO.SearchOption.TopDirectoryOnly)
    |> Seq.map (fun name -> let f = new System.IO.FileInfo(name)
                            f.Length)
    |> d'
    |> interpret

// ("Below:  α 0.01", "Above:  α 0.1 α 0.05")
let welshLocalAuthorityPopulationsTest = 
    [
        69700
        121900
        115200
        93700
        152500
        134800
        133000
        75900
        122400
        183800
        239000
        139800
        139200
        126300
        346100
        234400
        58800
        178800
        69800
        91100
        91300
        145700
    ] 
    |> d'
    |> interpret
