// [snippet: measures and type declerations]
[<Measure>] type rad
[<Measure>] type deg
[<Measure>] type km
type Location = { Latitude : float<deg>; Longitude : float<deg> }
// [/snippet]

// [snippet: calculation with haversine-formula]
let GreatCircleDistance<[<Measure>] 'u> (R : float<'u>) (p1 : Location) (p2 : Location) =
    let degToRad (x : float<deg>) = System.Math.PI * x / 180.0<deg/rad>

    let sq x = x * x
    // take the sin of the half and square the result
    let sinSqHf (a : float<rad>) = (System.Math.Sin >> sq) (a / 2.0<rad>)
    let cos (a : float<deg>) = System.Math.Cos (degToRad a / 1.0<rad>)

    let dLat = (p2.Latitude - p1.Latitude) |> degToRad
    let dLon = (p2.Longitude - p1.Longitude) |> degToRad

    let a = sinSqHf dLat + cos p1.Latitude * cos p2.Latitude * sinSqHf dLon
    let c = 2.0 * System.Math.Atan2(System.Math.Sqrt(a), System.Math.Sqrt(1.0-a))

    R * c
// [/snippet]

// [snippet: using the mean-earth-radius]
let GreatCircleDistanceOnEarth = GreatCircleDistance 6371.0<km>
// [/snippet]

// [snippet: example]
let p1 = { Latitude = 53.147222222222222222222222222222<deg>; Longitude = 0.96666666666666666666666666666667<deg> }

let p2 = { Latitude = 52.204444444444444444444444444444<deg>; Longitude = 0.14055555555555555555555555555556<deg> }

GreatCircleDistanceOnEarth p1 p2
// [/snippet]