namespace RockSpot.Runtime

open System
open ServiceStack.Text

module ServiceStack =

    let inline deserializeOption x = 
        match x with 
        | null -> None 
        | _ -> Some (JsonSerializer.DeserializeFromString<_> x)

    let inline serializeOption x =
        match x with
        | Some v -> JsonSerializer.SerializeToString<_> v
        | None -> null

    let registerOptionFor<'a> () =
        JsConfig<'a option>.DeSerializeFn <- Func<_,_>(deserializeOption)
        JsConfig<'a option>.SerializeFn <- Func<_,_>(serializeOption)

    let registerPrimitiveOptions =
           registerOptionFor<string> 
        >> registerOptionFor<bool>
        >> registerOptionFor<char>
        >> registerOptionFor<byte>
        >> registerOptionFor<sbyte>
        >> registerOptionFor<int16>
        >> registerOptionFor<uint16>
        >> registerOptionFor<int>
        >> registerOptionFor<uint32>
        >> registerOptionFor<int64>
        >> registerOptionFor<uint64>
        >> registerOptionFor<nativeint>
        >> registerOptionFor<unativeint>
        >> registerOptionFor<single>
        >> registerOptionFor<double>
        >> registerOptionFor<decimal>
        >> registerOptionFor<DayOfWeek>
        >> registerOptionFor<Guid>
        >> registerOptionFor<TimeSpan>
        >> registerOptionFor<DateTime>
        >> registerOptionFor<DateTimeOffset>
        
