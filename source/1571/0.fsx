module Array =

   /// Returns a tuple containing the first element in
   /// the input array for which f returns true, and
   /// a new array containing all input elements apart
   /// from the selected one.  It is an error for
   /// no element to be found.
   let pluck f a =
      let hit = Array.find f a
      let remainder = 
         a |> Array.filter (fun x -> x <> hit)
      hit, remainder

   /// Returns Some tuple containing the first element in
   /// the input array for which f returns true, and
   /// a new array containing all input elements apart
   /// from the selected one.  If no element is found
   /// returns None.
   let tryPluck f a =
      let hit = Array.tryFind f a
      match hit with
      | Some h ->
         let remainder = 
            a |> Array.filter (fun x -> x <> h)
         Some (h, remainder)
      | None -> None