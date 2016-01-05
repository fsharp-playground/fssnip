    open System

    [<Measure>] type deg
    [<Measure>] type rad
    [<Measure>] type x
    [<Measure>] type y
    type lon = float<x deg>
    type lat = float<y deg>
    let inline degToRad (d:float<'u deg>) = d*0.0174532925199433<rad/deg>
    let cos (d:float<'u rad>) = Math.Cos(float d)
    let sin (d:float<'u rad>) = Math.Sin(float d)
    
    let project (centerlon:lon) (centerlat:lat) (lon:lon) (lat:lat) =
        // http://mathworld.wolfram.com/AzimuthalEquidistantProjection.html
        // http://www.radicalcartography.net/?projectionref
        let t = degToRad lat
        let l = degToRad lon
        let t1 = degToRad centerlat // latitude center of projection
        let l0 = degToRad centerlon // longitude center of projection
        let c = Math.Acos ((sin t1) * (sin t) + (cos t1) * (cos t) * (cos (l-l0)))
        let k = c / (sin c)
        let x:float<x> = k * (cos t) * (sin (l-l0)) 
                         |> LanguagePrimitives.FloatWithMeasure
        let y:float<y> = k * (cos t1) * (sin t) - (sin t1) * (cos t) * (cos (l-l0)) 
                         |> LanguagePrimitives.FloatWithMeasure
        (x, y)