open Geo
open Geo.Measure
open Geo.Geometries

/// Liste mit id, latitude, longitude
let liste = [
    1, 46.6728308564052, 11.061115907505155
    2, 46.6728308564052, 11.061115907505195
    3, 46.6728308564052, 11.861115907505155
    4, 46.6728308564852, 11.061115907505155
    5, 56.6728308564052, 31.061115907505155
]

/// Die Ausgangskoordinate
let ausgangscoord = Coordinate(46.6728308564052, 11.061115907505155)
/// Der Radius in Meter
let radius = 1000.
/// Ein Kreis von einem Kilometer um die Ausgangskoordinate
let circle = Circle(ausgangscoord, radius)
/// Die äußere Umrandung
let circleBounds = circle.GetBounds()

/// Die gefilterten Koordinaten
let filtededCoords =
    liste
    // Umwandlung von (id, latitude, longitude) in (id, Coordinate(latitude, longitude))
    |> List.map    (fun (id, lat, lon) -> id, Coordinate(lat, lon))
    // Ist im Umkreis von einem Kilometer die jeweilige Koordinate enthalten?
    |> List.filter (snd >> circleBounds.Contains)

filtededCoords |> List.iter (printfn "%A")
