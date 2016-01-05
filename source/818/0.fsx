module EarthSimilarityIndex

// A planet:
type Planet = {
                mass : float; 
                radius : float; 
                density : float; 
                g : float; 
                ve : float; 
                a : float; 
                Tsurf : float; 
                Teq : float
              }

// Some basic planetary numbers:
let earth = {
                mass = 1.0; 
                radius = 1.0; 
                density = 1.0; 
                g = 1.0; 
                ve = 1.0; 
                a = 1.0; 
                Tsurf = 288.; 
                Teq = 254.
             }

let mars = {
                mass = 0.107; 
                radius = 0.53; 
                density = 0.71; 
                g = 0.38; 
                ve = 0.45; 
                a = 1.52; 
                Tsurf = 227.; 
                Teq = 210.
            }

let mercury = {
                mass = 0.0553; 
                radius = 0.38; 
                density = 0.98; 
                g = 0.38; 
                ve = 0.38; 
                a = 0.39; 
                Tsurf = 440.; 
                Teq = 434.
              }

// Same numbers but as arrays:
let earthStats = [|earth.mass; earth.radius; earth.density; earth.g; earth.ve; earth.a; earth.Tsurf; earth.Teq|]
let marsStats = [|mars.mass; mars.radius; mars.density; mars.g; mars.ve; mars.a; mars.Tsurf; mars.Teq|]
let mercuryStats = [|mercury.mass; mercury.radius; mercury.density; mercury.g; mercury.ve; mercury.a; mercury.Tsurf; mercury.Teq|]

// Weight all numbers equally:
let weights = [|1.; 1.; 1.; 1.; 1.; 1.; 1.; 1.|]

// As Array.fold but applying the given function to each element, and starting with the initial value x:
let foldBy f x a =
    a |> Array.fold (fun acc elem -> f acc elem) x

// Multiply up the values in an array:
let pi a = a |> foldBy (*) 1.

// Calculate the similarity index of two planets:
let similarityIndex p1 p2 w = 
    let n = p1 |> Array.length |> float
    Array.zip3 p1 p2 w
    |> Array.map (fun (p1Val, p2Val, wVal) -> 
                    (1. - abs((p1Val - p2Val) / (p1Val + p2Val)) ) ** (wVal / n))
    |> pi 

// Calculate the Earth Similarity Index of any planet:
let ESI = similarityIndex earthStats

// Some similarity indices within the Solar System:
let earthEarth = ESI earthStats weights // 1.0
let earthMars = ESI marsStats weights // 0.6276230757
let earthMercury = ESI mercuryStats weights // 0.5239652331
