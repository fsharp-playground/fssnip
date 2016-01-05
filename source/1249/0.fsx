// This does not work - no automatic conversion to 'obj'
let objs1 = [1; "hi"]
// ...but it works if compiler knows the type in advance
let objs2 : obj list = [1; "hi"]

// But, that only works for simple types - so even if we
// know the type in advance, it will not work for tuples :-(
let objs2 : (string*obj) list = [("hi", 1)]

// We could define our own tuple type to make this possible
// with a less generic base class - here, I just make base
// class generic in key, but not in value
type Tuple<'K> =  
  abstract Key : 'K
  abstract Value : obj

// And then we have fully generic tuple that implements the
// less-generic variant (and boxes the value)
type Tuple<'K, 'V> = 
  | KV of 'K * 'V
  member x.Key = let (KV(k, v)) = x in k
  member x.Value = let (KV(k, v)) = x in v
  interface Tuple<'K> with
    member x.Key = let (KV(k, v)) = x in k
    member x.Value = let (KV(k, v)) = x in box v
    
// Builds the more-generic version of the tuple
let (=>) k v = KV(k, v)

// This does not work, because we are creating list that
// contains 'Tuple<string, int>' and 'Tuple<string, string>'
let pars = ["x" => 1; "y" => "42"]

// But we can make it work if we know the type in advance
let namedPars (p:list<Tuple<string>>) = p
let pars = namedPars ["x" => 1; "y" => "42"]

// Note that this will NOT work
["x" => 1; "y" => "42"] |> namedPars

// It plays nicely with ParamArray too
type R() =
  static member plot(s:string, [<System.ParamArray>] pars:Tuple<string>[]) =
    printfn "%s - %A" s pars

// This gives us what Howard suggested!
R.plot("sin", "x" => 1, "y" => "42")

// But we can still write functions that require Tuple<string, int> 
// rather than tuple with any type of values
let fromNums (nums:list<Tuple<string, int>>) = nums

// Fine
fromNums ["a" => 1; "b" => 3]
// Not allowed
fromNums ["a" => 1; "b" => 'a']