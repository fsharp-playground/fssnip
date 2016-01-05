open System
open System.Collections
open System.Collections.Generic

type bigintRangeEnumerator(start: bigint, max: bigint) =
    let mutable current = bigint.Zero
    let mutable started = false
    interface IEnumerator<bigint> with
        member this.Current = current
        member this.Dispose() = ()

    interface IEnumerator with
        member this.Current = box(current)
        member this.MoveNext() = 
            if ( started = false ) then
                started <- true
                current <- start
                true
            else if ( bigint.op_LessThan(current,max) ) then
                current <- bigint.op_Increment(current)
                true
            else
                false
        member x.Reset() = ()
             
type bigintRange(start: bigint, max: bigint) =
    interface IEnumerable<bigint> with 
        member this.GetEnumerator() = new bigintRangeEnumerator(start, max) :> IEnumerator<bigint>
    interface IEnumerable with
        member this.GetEnumerator() = new bigintRangeEnumerator(start, max) :> IEnumerator
        
let bigint_range s e = new bigintRange(s,e)

let rangeEnd=(bigint (Int32.MaxValue / 100))

#time "on"
bigint_range 1I rangeEnd |> Seq.sum |> printfn "bigint_range range: %A" 
#time "on"
{ 1I.. rangeEnd } |> Seq.sum |> printfn "F# bigint range: %A" 
#time "off"

