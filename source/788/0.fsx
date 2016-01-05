module Mandelbrot

open System

let countIters x0 y0 maxIter =
    let rec iters x y i =
        if (x*x + y*y < 4.0) && (i < maxIter) then
            let x' = x*x - y*y + x0
            let y' = 2.*x*y + y0
            let i' = i+1
            iters x' y' i'
        else
            i
    iters 0. 0. 0

let mandelMap xMin xMax yMin yMax stepSize maxIter =
    let xCount, yCount = (xMax - xMin) / stepSize |> int, (yMax - yMin) / stepSize |> int
    Array2D.init xCount yCount (fun x y -> countIters (float(x)*stepSize+xMin) (float(y)*stepSize+yMin) maxIter)
    
let printMap (map : int[,]) =
    let charMap = [|'.'; ','; '\''; '-'; ':'; '/'; '('; '*'; '|'; '$'; '#'; '@'; '%'; '~'|]
    let charIndex rawIndex =
        rawIndex % (charMap |> Array.length)
    map
    |> Array2D.iteri (fun x y elem -> let char = charMap.[charIndex elem]
                                      if y = 0 then printfn ""
                                      printf "%c" char)
                                      
// Example
mandelMap -2.0 2.0 -2.0 2.0 0.1 1000 |> printMap

//    ,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,
//    ,,,,,,,,,,,,,,''--::(::--'',,,,,,,,,,,,,
//    ,,,,,,,,,,,,''---:::(:::---'',,,,,,,,,,,
//    ,,,,,,,,,,''-----::(((::-----'',,,,,,,,,
//    ,,,,,,,,,''------:/(((/:------'',,,,,,,,
//    ,,,,,,,''''-----://*(*//:-----'''',,,,,,
//    ,,,,,,''''------//(%(%(//------'''',,,,,
//    ,,,,,''''------:**|.(.|**:------'''',,,,
//    ,,,,,''''------/*':(((:'*/------'''',,,,
//    ,,,,''''------:/*%(((((%*/:------'''',,,
//    ,,,'''''-----::/**(((((**/::-----''''',,
//    ,,,'''''----:://*@(((((@*//::----''''',,
//    ,,''''''---:::/(*$,(((,$*(/:::---'''''',
//    ,,'''''----::/(|@#(((((#@|(/::----''''',
//    ,''''''---:::*%%(((((((((%%*:::---''''''
//    ,''''''--:::/|(((((((((((((|/:::--''''''
//    ,''''''--::/(|%(((((((((((%|(/::--''''''
//    ,''''''--:/(|#(((((((((((((#|(/:--''''''
//    ,'''''''-/*$((((((((((((((((($*/-'''''''
//    ,'''''''-/$#(((((((((((((((((#$/-'''''''
//    ,'''''''-:(|:~(((((((((((((~:|(:-'''''''
//    ,'''''''-::/($((((((((((((($(/::-'''''''
//    ,''''''''-::/(%(((((((((((%(/::-''''''''
//    ,''''''''--:/(,(((((%(((((,(/:--''''''''
//    ,'''''''''--:/,*$,-|*|-,$*,/:--'''''''''
//    ,''''''''''--:://(/////(//::--''''''''''
//    ,''''''''''-----:::::::::-----''''''''''
//    ,,''''''''''-----------------'''''''''',
//    ,,''''''''''''-------------'''''''''''',
//    ,,,'''''''''''''---------''''''''''''',,
//    ,,,''''''''''''''''''''''''''''''''''',,
//    ,,,,''''''''''''''''''''''''''''''''',,,
//    ,,,,,''''''''''''''''''''''''''''''',,,,
//    ,,,,,''''''''''''''''''''''''''''''',,,,
//    ,,,,,,''''''''''''''''''''''''''''',,,,,
//    ,,,,,,,''''''''''''''''''''''''''',,,,,,
//    ,,,,,,,,,''''''''''''''''''''''',,,,,,,,
//    ,,,,,,,,,,''''''''''''''''''''',,,,,,,,,
//    ,,,,,,,,,,,,''''''''''''''''',,,,,,,,,,,
//    ,,,,,,,,,,,,,,''''''''''''',,,,,,,,,,,,,