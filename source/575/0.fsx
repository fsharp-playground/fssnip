open System
// [snippet:The implementation of Bi directional map]
/// Bi directional map.
/// It stores correspondences of two values.
/// It yields correspond value from another value of the pair.
type BiMap<'a,'b when 'a : comparison and 'b : comparison>(item1s:'a list, item2s:'b list) =
  // reusing standard F# library's map to implement find functions
  let item1IsKey = List.zip item1s item2s |> Map.ofList
  let item2IsKey = List.zip item2s item1s |> Map.ofList
  
  member __.findBy1    key = Map.find    key item1IsKey
  member __.tryFindBy1 key = Map.tryFind key item1IsKey 
  // Ctrl + H. then replace 1 to 2 the above two lines of code
  member __.findBy2    key = Map.find    key item2IsKey 
  member __.tryFindBy2 key = Map.tryFind key item2IsKey 
// [/snippet]

// [snippet:The implementation of toString and fromString function for a descreminated union]
type Week = 
  | Sunday 
  | Monday 
  | Tuesday 
  | Wednesday 
  | Tursday 
  | Friday 
  | Saturday

let weekBiMap = 
  BiMap (
    [ Sunday ; Monday ; Tuesday ; Wednesday ; Tursday ; Friday ; Saturday ],
    ["Sunday";"Monday";"Tuesday";"Wednesday";"Tursday";"Friday";"Saturday"]
  )

type Week with
  member this.ToStr           = this  |> weekBiMap.findBy1
  static member FromStr sWeek = sWeek |> weekBiMap.tryFindBy2
// [/snippet]

// [snippet:usage]
Sunday.ToStr // val it : string = "Sunday"

Week.FromStr "Tuesday" // val it : Week option = Some Tuesday
Week.FromStr "Tuesady" // val it : Week option = None

// In this snippet I use a class instance as a reusable factory which
// removes trivial function implementation.
// It seems that OOP class features eliminated boilerplates from the FP code.
// And this OOP class is easily implemented by using FP features.
// It's an interesting result isn't it?
// (I know there are already many examples that shows the multi-paradimn language really shine)
// [/snippet]
