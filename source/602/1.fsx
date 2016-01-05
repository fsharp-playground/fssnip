/// An array whose index has a unit of measure
type MarkedArray<[<Measure>] 'K, 'T> = MarkedArray of 'T[]
with
    member this.Content =
        let (MarkedArray arr) = this
        arr

    member this.First : int<'K> =
        LanguagePrimitives.Int32WithMeasure 0

    member this.Last : int<'K> =
        let (MarkedArray arr) = this
        LanguagePrimitives.Int32WithMeasure (arr.Length - 1)

    member this.Item
        with get (i : int<'K>) =
            let (MarkedArray arr) = this
            arr.[int i]
        and set (i : int<'K>) (v : 'T) =
            let (MarkedArray arr) = this
            arr.[int i] <- v

[<RequireQualifiedAccess>]
module MarkedArray =
    let inline set (arr : MarkedArray<'K, 'T>) idx v =
        arr.[idx] <- v

    let inline get (arr : MarkedArray<'K, 'T>) idx =
        arr.[idx]

    /// arr.[idx] <- f (arr.[idx])
    let inline mutate f (arr, idx) =
        let v = get arr idx
        set arr idx (f v)

module Example =
    type Ship = Ship
    type Missile = Missile
    
    [<Measure>] type MissileIndex
    [<Measure>] type ShipIndex

    let missiles : MarkedArray<MissileIndex, _> = Array.create 42 (Some Missile) |> MarkedArray
    let ships : MarkedArray<ShipIndex, _> = Array.create 4 (Some Ship) |> MarkedArray

    // A missile hit the ship, destroy both
    let applyHit shipIdx missileIdx =
        missiles.[missileIdx] <- None
        ships.[shipIdx] <- None
    