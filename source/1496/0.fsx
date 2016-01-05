module Array =
   let tryFindLastIndex predicate (array:'T array) =
      let mutable index = array.Length - 1
      while index >= 0 && not(predicate array .[index]) do index <- index - 1 
      if index >= 0 then Some index else None

module ASCII =
   module Char =
      let ToLower (c:byte) =
         if c >= 'A'B && c <= 'Z'B then c - 'A'B + 'a'B
         else c
      let ToUpper (c:byte) =
         if c >= 'a'B && c <= 'z'B then c - 'a'B + 'A'B
         else c
      let IsWhiteSpace (c:byte) =
         c = ' 'B || c = '\t'B || c = '\r'B || c = '\n'B

   module String =
      let Empty : byte[] = [||]   
      let IsEmpty (str:byte[]) = str.Length = 0
      let IsNullOrWhiteSpace str = 
         str = null || Array.forall Char.IsWhiteSpace str
      let Compare (strA:byte[],indexA,strB:byte[],indexB,length) =
         let rec compare n =
            if n = length then 0
            else 
               let d = strA.[indexA+n] - strB.[indexB+n] 
               if d = 0uy then compare (n+1)
               else int d
         compare 0

open System.Runtime.CompilerServices

type System.Convert with
   static member ToString(value:byte[]) =
      System.Text.ASCIIEncoding.ASCII.GetString(value)

[<Extension>]
type AsciiStringExtensions =
   [<Extension>] 
   static member ToLower(str) = 
      Array.map ASCII.Char.ToLower str
   [<Extension>]
   static member ToUpper(str) = 
      Array.map ASCII.Char.ToUpper str
   [<Extension>]
   static member Substring(str:byte[],index) = 
      str.[index..]
   [<Extension>]
   static member Substring(str:byte[],index,length) = 
      str.[index..index+length-1]
   [<Extension>]
   static member Remove(str:byte[],startIndex) =
      str.[0..startIndex-1]
   [<Extension>]
   static member Remove(str:byte[],startIndex:int,length:int) =
      let array = System.Array.CreateInstance(typeof<byte>, str.Length-length) :?> byte[]
      System.Array.Copy(str, array, startIndex)
      System.Array.Copy(str, startIndex+length, array, startIndex, str.Length-startIndex-length)
      array
   [<Extension>]
   static member IndexOf(str:byte[], value:byte) = 
      System.Array.IndexOf(str, value)
   [<Extension>]
   static member IndexOfAny(str, anyOf:byte[]) =
      let exists c = Array.exists ((=) c) anyOf
      match Array.tryFindIndex exists str with
      | Some index -> index
      | None -> -1
   [<Extension>]
   static member LastIndexOf(str, value:byte) = 
      System.Array.LastIndexOf(str, value)
   [<Extension>]
   static member LastIndexOfAny(str, anyOf:byte[]) =
      let exists c = Array.exists ((=) c) anyOf
      match Array.tryFindLastIndex exists str with
      | Some index -> index
      | None -> -1 
   [<Extension>]
   static member StartsWith(str:byte[],value:byte[]) =
      value.Length <= str.Length &&
      ASCII.String.Compare(str,0,value,0,value.Length) = 0
   [<Extension>]
   static member EndsWith(str:byte[],value:byte[]) =
      value.Length <= str.Length &&
      ASCII.String.Compare(str, str.Length-value.Length, value, 0, value.Length) = 0
   [<Extension>]
   static member Contains(str:byte[],value:byte[]) =
      let rec contains index =
         if index < str.Length - value.Length then compare index
         else false
      and compare index =
         if ASCII.String.Compare(str,index,value,0,value.Length) <> 0
         then contains (index+1)
         else true
      contains 0
   [<Extension>]
   static member Trim(str:byte[]) =
      let mutable i = 0
      while i < str.Length && ASCII.Char.IsWhiteSpace str.[i] do i <- i + 1
      let mutable j = str.Length - 1
      while j > i && ASCII.Char.IsWhiteSpace str.[j] do j <- j - 1
      if i=0 && j=str.Length-1 then str 
      else str.[i..j]

"Hello"B.StartsWith("Hell"B)