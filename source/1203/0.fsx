open System

module Earth =
    type Point(lon,lat) = 
        let toRad deg = deg * (Math.PI / 180.0)
        
        member this.Lon = lon
        member this.Lat = lat
        
        member this.LonRad = toRad lon
        member this.LatRad = toRad lat

    
    let greatCircleDistance (p1:Point) (p2:Point) =
        // code adapted from 
        // http://www.codeproject.com/Articles/12269/Distance-between-locations-using-latitude-and-long
        (*
            The Haversine formula according to Dr. Math.
            http://mathforum.org/library/drmath/view/51879.html
                
            dlon = lon2 - lon1
            dlat = lat2 - lat1
            a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
            c = 2 * atan2(sqrt(a), sqrt(1-a)) 
            d = R * c
                
            Where
                * dlon is the change in longitude
                * dlat is the change in latitude
                * c is the great circle distance in Radians.
                * R is the radius of a spherical Earth.
                * The locations of the two points in 
                    spherical coordinates (longitude and 
                    latitude) are lon1,lat1 and lon2, lat2.
        *)
        
        let dlon = p2.LonRad - p1.LonRad;
        let dlat = p2.LatRad - p1.LatRad;

        // Intermediate result a.
        let a = (sin (dlat / 2.0)) ** 2.0 + ((cos p1.LatRad) * (cos p2.LatRad) * (sin (dlon / 2.0)) ** 2.0);

        // Intermediate result c (great circle distance in Radians).
        let c = 2.0 * (asin (sqrt a));

        // Distance.
        let earthRadiusKms = 6371.0;
        let distance = earthRadiusKms * c;

        distance

    let test = 
        let d = greatCircleDistance (new Point(5.0, -32.0)) (new Point(-3.0, 4.0))
        printfn "%f" d // 4091 km