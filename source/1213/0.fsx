namespace Distance

[<AutoOpen>]
module Units =

    open System

    [<Measure>] type km
    [<Measure>] type rad
    [<Measure>] type deg

    let degToRad (degrees : float<deg>) =
        degrees * Math.PI / 180.<deg/rad>

[<AutoOpen>]
module Constants =

    let earthRadius = 6371.<km>
    let marsRadius = 3397.<km>

module GreatCircle = 

    open System

    /// Calculates the great-circle distance between two Latitude/Longitude positions on a sphere of given radius.
    let DistanceBetween (radius:float<km>) lat1 long1 lat2 long2 =
        let lat1r, lat2r, long1r, long2r = lat1 |> degToRad, 
                                           lat2 |> degToRad,
                                           long1 |> degToRad,
                                           long2 |> degToRad
        let deltaLat = lat2r - lat1r
        let deltaLong = long2r - long1r

        let a = Math.Sin(deltaLat/2.<rad>) ** 2. +
                (Math.Sin(deltaLong/2.<rad>) ** 2. * Math.Cos((double)lat1r) * Math.Cos((double)lat2r))

        let c = 2. * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.-a))

        radius * c

    /// Calculate DistanceBetween for Earth.
    let DistanceBetweenEarth = DistanceBetween earthRadius

    /// Calculate DistanceBetween for Mars.
    let DistanceBetweenMars = DistanceBetween marsRadius

module GreatCircleTests =

    open NUnit.Framework
    open FsUnit

    [<TestFixture>]
    type ``Given the DistanceBetween function for Earth``() =

        // Error margin for non-sphericality of Earth:
        let ErrorMargin = 0.003; // 0.3%

        // Travel no distance:
        [<TestCase(0., 0., 0., 0., 0.)>]
        // Travel along the equator eastwards for 90 degrees:
        [<TestCase(0., 0., 0., 90., 10018.79)>]
        // Travel along the equator westwards for 90 degrees:
        [<TestCase(0., 0., 0., -90., 10018.79)>]
        // Travel along the equator eastwards for 180 degrees:
        [<TestCase(0., 0., 0., 180., 20037.58)>]
        // Travel along the equator westwards for 180 degrees:
        [<TestCase(0., 0., 0., -180., 20037.58)>]
        // Travel along the meridian northwards 90 degrees:
        [<TestCase(0., 0., 90., 0., 10018.79)>]
        // Travel along the meridian soutwards 90 degrees:
        [<TestCase(0., 0., -90., 0., 10018.79)>]
        // Travel from Farnham to Reigate:
        [<TestCase(51.214, -0.799, 51.230, -0.188, 42.5)>]
        // Travel from London to Sidney Australia:
        [<TestCase(51.51, -0.13, -33.86, 151.21, 16998.)>]
        
        member t.``the function returns the right result``(lat1, long1, lat2, long2, expected:float<km>) =
            let actual = GreatCircle.DistanceBetweenEarth lat1 long1 lat2 long2
            let error = expected * ErrorMargin
            actual |> should (equalWithin error) expected

    [<TestFixture>]
    type ``Given the DistanceBetween function for Mars``() =

        // Error margin for non-sphericality of Mars:
        let ErrorMargin = 0.003; // 0.3%

        // Travel from Olympus Mons to Pavonis Mons:
        [<TestCase(18.65, 226.2, 1.48, 247.04, 1582.)>]
        
        member t.``the function returns the right result``(lat1, long1, lat2, long2, expected:float<km>) =
            let actual = GreatCircle.DistanceBetweenMars lat1 long1 lat2 long2
            let error = expected * ErrorMargin
            actual |> should (equalWithin error) expected
