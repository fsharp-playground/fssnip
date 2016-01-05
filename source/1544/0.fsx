#if INTERACTIVE
#r @"..\packages\Microsoft.Kinect.2.0.1410.19000\lib\net45\Microsoft.Kinect.dll"
#else
module Kinect20
#endif

open Microsoft.Kinect
open System.Collections.Generic
let bodyFrameReader = 
    let kinectSensor = KinectSensor.GetDefault()
    kinectSensor.IsAvailableChanged |> Event.add(fun _ -> 
        match kinectSensor.IsAvailable with 
        | true -> printfn "sensor available" 
        | false -> printfn "sensor disconnected")

    kinectSensor.Open()
    kinectSensor.BodyFrameSource.OpenReader()

let stop() = bodyFrameReader.Dispose()
let mutable bodyContainer = None // mutable API :-(

let bodyTracking = 
    bodyFrameReader.FrameArrived 
    |> Event.add(
        fun bfae ->
        use bodyFrame = bfae.FrameReference.AcquireFrame()
        match bodyFrame<>null && bodyFrame.BodyCount > 0 with
        | false -> ()
        | true ->
            if bodyContainer = None then 
                bodyContainer <- Some(Array.create bodyFrame.BodyCount (Unchecked.defaultof<Body>))
            
            match bodyContainer with
            | None -> ()
            | Some bodies -> //printfn "bc: %i bf: %i" bodies.Length bodyFrame.BodyCount
                bodyFrame.GetAndRefreshBodyData(bodies)
                let parts = bodies |> Seq.filter (fun b -> b.IsTracked)
                parts
                |> Seq.map (fun b -> b.Joints)
                |> Seq.iter(fun j -> j |> Seq.iter(fun p -> 
                            let part = p.Key.ToString("F")
                            let pos = p.Value.Position                              
                            printfn "Bodypart %s at %f %f %f" part pos.X pos.Y pos.Z))
//                parts 
//                |> Seq.filter(fun b -> b.HandLeftState = HandState.Lasso) 
//                |> Seq.iter(fun i -> printfn "Lasso!")            
        )