/// Calculate the Jaro-Winkler distance of s1 and s2
let jaroWinkler s1 s2 = 
    let jaroScore = jaro s1 s2
    // Accumulate the number of matching initial characters
    let maxLength = (min s1.Length s2.Length) - 1
    let rec calcL i acc =
        if i > maxLength || s1.[i] <> s2.[i] then acc
        else calcL (i + 1) (acc + 1.0)
    let l = min (calcL 0 0.0) 4.0
    // Calculate the JW distance
    let p = 0.1
    let result = jaroScore + (l * p * (1.0 - jaroScore))
    // This isn't strictly necessary as we can't divide by zero
    // but it makes me feel better 
    if Double.IsNaN result then 0.0 else result

[<Fact>]
let ``Jaro-Winkler identity test`` () = 
    let result = jaroWinkler "RICK" "RICK"
    Assert.Equal("1.000", String.Format("{0:0.000}", result))

[<Fact>]
let ``Jaro-Winkler martha test`` () = 
    let result = jaroWinkler "MARTHA" "MARHTA"
    Assert.Equal("0.961", String.Format("{0:0.000}", result))

[<Fact>]
let ``Jaro-Winkler dwayne test`` () = 
    let result = jaroWinkler "DWAYNE" "DUANE"
    Assert.Equal("0.840", String.Format("{0:0.000}", result))

[<Fact>]
let ``Jaro-Winkler dixon test`` () =
    let result = jaroWinkler "DIXON" "DICKSONX"
    Assert.Equal("0.813", String.Format("{0:0.000}", result))
