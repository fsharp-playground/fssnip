type System.Type with 
  /// Returns nicely formatted name of the type
  member t.NiceName =
    let sb = new System.Text.StringBuilder()
    let rec build (t:System.Type) =
      if t.IsGenericType then 
        // Remove the `1 part from generic names
        let tick = t.Name.IndexOf('`')
        let name = t.Name.Substring(0, tick) 
        Printf.bprintf sb "%s" t.Name
        Printf.bprintf sb "<"
        // Print generic type arguments recursively
        let args = t.GetGenericArguments()
        for i in 0 .. args.Length - 1 do 
          if i <> 0 then Printf.bprintf sb ", "
          build args.[i]
        Printf.bprintf sb ">"
      else
        // Print ordiary type name
        Printf.bprintf sb "%s" t.Name
    build t
    sb.ToString()
