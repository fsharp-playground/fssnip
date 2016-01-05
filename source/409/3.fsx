namespace FSharp.Collections

open System
open System.Diagnostics.Contracts

//[snippet: ByteString]
/// An ArraySegment with structural comparison and equality.
/// An ArraySegment with structural comparison and equality.
[<CustomEquality; CustomComparison>]
[<SerializableAttribute>]
type BS =
  struct
    val Array: byte[]
    val Offset: int
    val Count: int
    new (array: byte[]) = { Array = array; Offset = 0; Count = array.Length }
    new (array: byte[], offset: int, count: int) = { Array = array; Offset = offset; Count = count }
    static member Compare (a:BS, b:BS) =
      let x,o,l = a.Array, a.Offset, a.Count
      let x',o',l' = b.Array, b.Offset, b.Count
      if x = x' && o = o' && l = l' then 0
      elif x = x' then
        if o = o' then if l < l' then -1 else 1
        else if o < o' then -1 else 1 
      else let foldr res b b' =
              if res <> 0 then res
              else if b = b' then 0
                   elif b < b' then -1
                   else 1
           let left = [| for i in o..(o+l-1) -> x.[i] |]
           let right = [| for i' in o'..(o'+l'-1) -> x'.[i'] |]
           Array.fold2 foldr 0 left right
    override x.Equals(other) = 
      match other with
      | :? BS as other' -> BS.Compare(x, other') = 0
      | _ -> false
    override x.GetHashCode() = hash x
    interface System.IComparable with
      member x.CompareTo(other) =
        match other with
        | :? BS as other' -> BS.Compare(x, other')
        | _ -> invalidArg "other" "Cannot compare a value of another type."
  end
  
module ByteString =

  /// An active pattern for conveniently retrieving the properties of a BS.
  let (|BS|) (x:BS) = x.Array, x.Offset, x.Count
  
  let empty = BS()
  let singleton c = BS(Array.create 1 c, 0, 1)
  let create arr = BS(arr, 0, arr.Length)
  let ofArraySegment (segment:ArraySegment<byte>) = BS(segment.Array, segment.Offset, segment.Count)
  let ofSeq s = let arr = Array.ofSeq s in BS(arr, 0, arr.Length)
  let ofList l = BS(Array.ofList l, 0, l.Length)
  let ofString (s:string) = s.ToCharArray() |> Array.map byte |> create
  let toSeq (bs:BS) =
    seq { for i in bs.Offset..(bs.Offset + bs.Count - 1) do yield bs.Array.[i] }
  let toList (bs:BS) =
    [ for i in bs.Offset..(bs.Offset + bs.Count - 1) -> bs.Array.[i] ]
  let toString (bs:BS) =
    System.Text.Encoding.ASCII.GetString(bs.Array, bs.Offset, bs.Count)
  let isEmpty (bs:BS) = Contract.Requires(bs.Count >= 0); bs.Count <= 0
  let length (bs:BS) = Contract.Requires(bs.Count >= 0); bs.Count
  let index (bs:BS) pos =
    Contract.Requires(bs.Offset + pos <= bs.Count)
    bs.Array.[bs.Offset + pos]
  let head (bs:BS) =
    if bs.Count <= 0 then
      failwith "Cannot take the head of an empty byte string."
    else bs.Array.[bs.Offset]
  let tail (bs:BS) =
    Contract.Requires(bs.Count >= 1)
    if bs.Count = 1 then empty
    else BS(bs.Array, bs.Offset+1, bs.Count-1)
  
  /// cons uses Buffer.SetByte and Buffer.BlockCopy for efficient array operations.
  /// Please note that a new array is created and both the head and tail are copied in,
  /// disregarding any additional bytes in the original tail array.
  let cons hd (bs:BS) =
    let x,o,l = bs.Array, bs.Offset, bs.Count in
    if l = 0 then singleton hd
    else let buffer = Array.init (l + 1) byte
         Buffer.SetByte(buffer,0,hd)
         Buffer.BlockCopy(x,o,buffer,1,l)
         BS(buffer,0,l+1)
  
  /// append uses Buffer.BlockCopy for efficient array operations.
  /// Please note that a new array is created and both arrays are copied in,
  /// disregarding any additional bytes in the original, underlying arrays.
  let append a b = 
    if isEmpty a then b
    elif isEmpty b then a
    else let x,o,l = a.Array, a.Offset, a.Count
         let x',o',l' = b.Array, b.Offset, b.Count
         let buffer = Array.init (l + l') byte
         Buffer.BlockCopy(x,o,buffer,0,l)
         Buffer.BlockCopy(x',o',buffer,l,l')
         BS(buffer,0,l+l')
  
  let fold f seed bs =
    let rec loop bs acc =
      if isEmpty bs then acc 
      else
        let hd, tl = head bs, tail bs
        loop tl (f acc hd)
    loop bs seed

  let span pred (bs:BS) =
    if isEmpty bs then empty, empty
    else
      let x,o,l = bs.Array, bs.Offset, bs.Count
      let rec loop acc =
        if l = acc + 1 && pred x.[o+acc] then bs, empty
        elif not (pred x.[o+acc]) then BS(x,o,acc), BS(x,o+acc,l-acc)
        else loop (acc+1)
      loop 0
  
  let split pred bs = span (not << pred) bs
  
  let splitAt n (bs:BS) =
    Contract.Requires(n >= 0)
    if isEmpty bs then empty, empty
    elif n = 0 then empty, bs
    elif n >= bs.Count then bs, empty
    else let x,o,l = bs.Array, bs.Offset, bs.Count in BS(x,o,n), BS(x,o+n,l-n)
  
  let skip n bs = splitAt n bs |> snd
  let skipWhile pred bs = span pred bs |> snd
  let skipUntil pred bs = split pred bs |> snd
  let take n bs = splitAt n bs |> fst 
  let takeWhile pred bs = span pred bs |> fst
  let takeUntil pred bs = split pred bs |> fst 
//[/snippet]

module FSharp.Collections.Tests.ByteStringTest
//[snippet: Tests]
open System
open FSharp.Collections
open FSharp.Collections.ByteString
open NUnit.Framework
open FsUnit

[<Test>]
let ``test ByteString_length should return the length of the byte string``() =
  let input = create "Hello, world!"B
  let actual = length input
  actual |> should equal 13

let spanAndSplitTests = [|
  [| box "Howdy! Want to play?"B; box ' 'B; box 6 |]
  [| box "Howdy! Want to play?"B; box '?'B; box 19 |]
  [| box "Howdy! Want to play?"B; box '\r'B; box 20 |]
|]

[<Test>]
[<TestCaseSource("spanAndSplitTests")>]
let ``test ByteString_span correctly breaks the ByteString on the specified predicate``(input:byte [], breakChar:byte, breakIndex:int) =
  let str = create input
  let expected = if input.Length = breakIndex then str, empty
                 else BS(input, 0, breakIndex), BS(input, breakIndex, input.Length - breakIndex)
  let actual = span ((<>) breakChar) str
  actual |> should equal expected

[<Test>]
[<TestCaseSource("spanAndSplitTests")>]
let ``test ByteString_split correctly breaks the ByteString on the specified predicate``(input:byte [], breakChar:byte, breakIndex:int) =
  let str = create input
  let expected = if input.Length = breakIndex then str, empty
                 else BS(input, 0, breakIndex), BS(input, breakIndex, input.Length - breakIndex)
  let actual = split ((=) breakChar) str
  actual |> should equal expected

[<Test>]
let ``test ByteString_span correctly breaks the ByteString on \r``() =
  let input = "test\r\ntest"B
  let str = create input
  let expected = BS(input, 0, 4), BS(input, 4, 6)
  let actual = span (fun c -> c <> '\r'B && c <> '\n'B) str
  actual |> should equal expected

[<Test>]
let ``test ByteString_split correctly breaks the ByteString on \r``() =
  let input = "test\r\ntest"B
  let str = create input
  let expected = BS(input, 0, 4), BS(input, 4, 6)
  let actual = split (fun c -> c = '\r'B || c = '\n'B) str
  actual |> should equal expected

[<Test>]
let ``test ByteString_splitAt correctly breaks the ByteString on the specified index``() =
  let input = "Howdy! Want to play?"B
  let str = create input
  let expected = BS(input, 0, 6), BS(input, 6, 14)
  let actual = splitAt 6 str
  actual |> should equal expected

[<Test>]
let ``test ByteString_fold should concatenate bytes into a string``() =
  create "Howdy"B
  |> fold (fun a b -> a + (char b).ToString()) ""
  |> should equal "Howdy"

[<Test>]
let ``test ByteString_take correctly truncates the ByteString at the selected index``() =
  let input = "Howdy! Want to play?"B
  let str = create input
  let expected = BS(input, 0, 6)
  let actual = take 6 str
  actual |> should equal expected

[<Test>]
[<Sequential>]
let ``test drop should drop the first n items``([<Values(0,1,2,3,4,5,6,7,8,9)>] x) =
  let input = "Howdy! Want to play?"B
  let actual = skip 7 (create input)
  actual |> should equal (BS(input,7,13))

[<Test>]
let ``test dropWhile should drop anything before the first space``() =
  let input = create "Howdy! Want to play?"B
  let dropWhile2Head = skipWhile ((<>) ' 'B) >> head
  let actual = dropWhile2Head input
  actual |> should equal ' 'B

[<Test>]
let ``test take should return an empty ArraySegment when asked to take 0``() =
  let actual = take 0 (create "Nothing should be taken"B)
  actual |> should equal empty

[<Test>]
let ``test take should return an empty ArraySegment when given an empty ArraySegment``() =
  let actual = take 4 empty
  actual |> should equal empty

[<Test>]
[<Sequential>]
let ``test take should take the first n items``([<Values(1,2,3,4,5,6,7,8,9,10)>] x) =
  let input = [|0uy..9uy|]
  let expected = BS(input,0,x)
  let actual = take x (create input)
  actual |> should equal expected

[<Test>]
let ``test takeWhile should return an empty ArraySegment when given an empty ArraySegment``() =
  let actual = takeWhile ((<>) ' 'B) empty
  actual |> should equal empty

[<Test>]
let ``test takeWhile should take anything before the first space``() =
  let input = "Hello world"B
  let actual = takeWhile ((<>) ' 'B) (create input)
  actual |> should equal (BS(input, 0, 5))

[<Test>]
let ``test takeUntil should return an empty ArraySegment when given an empty ArraySegment``() =
  let actual = takeUntil ((=) ' 'B) empty
  actual |> should equal empty

[<Test>]
let ``test takeUntil should correctly split the input``() =
  let input = "abcde"B
  let actual = takeUntil ((=) 'c'B) (create input)
  actual |> should equal (BS(input, 0, 2))
//[/snippet]