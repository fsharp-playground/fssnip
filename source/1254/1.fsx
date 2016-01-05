open Geo
open Geo.Measure
open Geo.Geometries

/// Liste mit id, latitude, longitude
let liste = [
    1, 46.672830, 11.061115
    2, 46.673184, 11.048070
    3, 46.640194, 11.189862
    4, 46.684255, 11.050816
    5, 46.674576, 11.057681
    6, 46.672765, 11.060170
    7, 46.661640, 11.159306
    8, 46.689201, 11.107121
    9, 46.657398, 11.071416
    10, 46.672851, 11.060893
    11, 46.701102, 11.178753
    12, 46.684147, 11.154034
    13, 46.640793, 11.071636
]

/// Die Ausgangskoordinate
let ausgangscoord = Coordinate(46.6728308564052, 11.061115907505155)
/// Der Radius in Meter
let radius = 15000.
/// Ein Kreis von x Kilometer um die Ausgangskoordinate
let circle = Circle(ausgangscoord, radius)
/// Die äußere Umrandung
let circleBounds = circle.GetBounds()

let getDistance (coord1: Coordinate) (coord2: Coordinate) =
    abs(coord1.GetBounds().GetLength().Value - coord2.GetBounds().GetLength().Value)
//    abs << int <| (coord1.GetBounds().GetLength().Value - coord2.GetBounds().GetLength().Value) / 1000.    // m?

/// Die gefilterten Koordinaten
let filtededCoords =
    liste
    // Umwandlung von (id, latitude, longitude) in (id, Coordinate(latitude, longitude))
    |> List.map    (fun (id, lat, lon) -> id, Coordinate(lat, lon))
    // Ist im Umkreis von x Kilometer die jeweilige Koordinate enthalten?
    |> List.filter (snd >> circleBounds.Contains)
    // Berechne die Distanz zum Ausgangspunkt
    |> List.map    (fun (id, coord) -> id, getDistance ausgangscoord coord)
    // Sortiere nach den am nächsten liegenden
    |> List.sortBy snd

filtededCoords |> List.iter (printfn "%A")

