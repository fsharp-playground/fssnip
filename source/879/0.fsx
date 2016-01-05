#if INTERACTIVE
#r "System.Drawing"
#endif

module LatinSquaresDemo =

    module LatinSquares = 
        open System

        // Generate a Latin Square, populating with the specified values. To generate a 'reduced' Latin Square
        // send in the values sorted; otherwise randomise the values before calling.
        let inline LatinSquare(values : ^a[]) =
            let n = values |> Array.length
            let r = new Random()
            let scramble = Array.sortBy (fun _ -> r.Next(n))

            let hasConflicts (a : ^a option[,]) r =
                let (=?) (m : ^a option) (n : ^a option) =
                       (n.IsNone && m.IsNone)
                    || (n.IsSome && m.IsSome && (n.Value = m.Value))
                let mutable conflicts = false
                for c in 0..n-1 do
                    let bottom = a.[r,c]
                    for r' in 0..r-1 do
                        if bottom =? a.[r',c] then
                            conflicts <- true
                conflicts

            let arr = Array2D.init n n (fun _ _ -> None)

            values |> Array.iteri (fun c x -> arr.[0, c] <- Some(x))

            for r in 1..n-1 do
                let mutable conflicts = true
                while conflicts do
                    values |> scramble |> Array.iteri (fun c x -> arr.[r,c] <- Some(x))
                    conflicts <- hasConflicts arr r

            arr |> Array2D.map (fun e -> e.Value)

    module Demo =

        open System
        open System.Drawing
        open LatinSquares

        // [[1; 2; 3]
        //  [2; 3; 1]
        //  [3; 1; 2]]
        let ints3x3 = LatinSquare([|1..3|])

        // [[1; 2; 3; 4; 5; 6; 7]
        //  [5; 4; 2; 1; 6; 7; 3]
        //  [4; 6; 5; 7; 1; 3; 2]
        //  [7; 1; 4; 3; 2; 5; 6]
        //  [3; 7; 1; 6; 4; 2; 5]
        //  [6; 5; 7; 2; 3; 1; 4]
        //  [2; 3; 6; 5; 7; 4; 1]]
        let ints7x7 = LatinSquare([|1..7|])

        // [[1.5; 2.0; 2.5; 3.0; 3.5; 4.0; 4.5]
        //  [3.5; 3.0; 2.0; 1.5; 4.0; 4.5; 2.5]
        //  [3.0; 4.0; 3.5; 4.5; 1.5; 2.5; 2.0]
        //  [4.5; 1.5; 3.0; 2.5; 2.0; 3.5; 4.0]
        //  [2.5; 4.5; 1.5; 4.0; 3.0; 2.0; 3.5]
        //  [4.0; 3.5; 4.5; 2.0; 2.5; 1.5; 3.0]
        //  [2.0; 2.5; 4.0; 3.5; 4.5; 3.0; 1.5]]
        let floats7x7 = LatinSquare([|1.5..0.5..4.5|])

        //[[02/01/2013 00:00:00; 03/01/2013 00:00:00; 04/01/2013 00:00:00;
        //    05/01/2013 00:00:00; 06/01/2013 00:00:00; 07/01/2013 00:00:00;
        //    08/01/2013 00:00:00]
        //    [06/01/2013 00:00:00; 05/01/2013 00:00:00; 03/01/2013 00:00:00;
        //    02/01/2013 00:00:00; 07/01/2013 00:00:00; 08/01/2013 00:00:00;
        //    04/01/2013 00:00:00]
        //    [05/01/2013 00:00:00; 07/01/2013 00:00:00; 06/01/2013 00:00:00;
        //    08/01/2013 00:00:00; 02/01/2013 00:00:00; 04/01/2013 00:00:00;
        //    03/01/2013 00:00:00]
        //    [08/01/2013 00:00:00; 02/01/2013 00:00:00; 05/01/2013 00:00:00;
        //    04/01/2013 00:00:00; 03/01/2013 00:00:00; 06/01/2013 00:00:00;
        //    07/01/2013 00:00:00]
        //    [04/01/2013 00:00:00; 08/01/2013 00:00:00; 02/01/2013 00:00:00;
        //    07/01/2013 00:00:00; 05/01/2013 00:00:00; 03/01/2013 00:00:00;
        //    06/01/2013 00:00:00]
        //    [07/01/2013 00:00:00; 06/01/2013 00:00:00; 08/01/2013 00:00:00;
        //    03/01/2013 00:00:00; 04/01/2013 00:00:00; 02/01/2013 00:00:00;
        //    05/01/2013 00:00:00]
        //    [03/01/2013 00:00:00; 04/01/2013 00:00:00; 07/01/2013 00:00:00;
        //    06/01/2013 00:00:00; 08/01/2013 00:00:00; 05/01/2013 00:00:00;
        //    02/01/2013 00:00:00]]
        let dates7x7 = LatinSquare([|1..7|] |> Array.map (fun d -> DateTime(2013, 1, 1).AddDays(d |> float)))

        // [["Harder"; "Better"; "Faster"; "Stronger"]
        //  ["Stronger"; "Harder"; "Better"; "Faster"]
        //  ["Faster"; "Stronger"; "Harder"; "Better"]
        //  ["Better"; "Faster"; "Stronger"; "Harder"]]
        let daftPunk = LatinSquare([|"Harder"; "Better"; "Faster"; "Stronger"|])

        //<html>
        //  <table style="border:2px solid #404040; border-collapse: collapse; ">
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //    </tr>
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //    </tr>
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //    </tr>
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //    </tr>
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //    </tr>
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //    </tr>
        //    <tr>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:DarkGreen"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Purple"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:LightBlue"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Yellow"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Brown"></td>
        //      <td style="width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:Red"></td>
        //    </tr>
        //  </table>
        //</html>
        let stainedGlassHTML =
            let colors = [| Color.Purple; Color.LightBlue; Color.DarkGreen; Color.Red; Color.Brown; Color.Yellow; Color.DarkBlue |]
            let n = colors |> Array.length

            let sb = new System.Text.StringBuilder()
            let (+~) line = sb.AppendLine(line) |> ignore

            "<html>" |> (+~)
            "  <table style=\"border:2px solid #404040; border-collapse: collapse; \">" |> (+~)

            LatinSquare(colors)
            |> Array2D.iteri (fun _ c panel -> if c = 0 then   "    <tr>" |> (+~)
                                               sprintf         "      <td style=\"width:20px; height:22px; border:2px solid #404040; border-collapse: collapse; background-color:%s\"></td>" panel.Name |> (+~)
                                               if c = n-1 then "    </tr>" |> (+~))
            "  </table>" |> (+~)
            "</html>" |> (+~)

            sb.ToString()
