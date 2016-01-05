module Program

// Creates y-flipped premultiplied alpha versions of all .png files in its folder

open System
open System.IO
open System.Drawing
open System.Drawing.Imaging



[<EntryPoint>]
let main argv = 
    let files = Directory.EnumerateFiles("./")

    for filepath in files do
        if filepath.EndsWith ".png" then
            use bmp = new Bitmap(filepath)
            let premultiplyAlpha (c : Color) =
                let toFloat (b : uint8) = float b / 255.
                let toBInt (f : float)  = (f * 255.) |> max 0. |> min 255. |> round |> int
                let mult comp =
                    toFloat c.A * toFloat comp |> toBInt
                Color.FromArgb(c.A |> int, mult c.R, mult c.G, mult c.B)

            for x = 0 to bmp.Width - 1 do
                for y = 0 to bmp.Height - 1 do
                    bmp.SetPixel(x, y, bmp.GetPixel(x, y) |> premultiplyAlpha)

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY)

            bmp.Save((filepath.Substring(0, filepath.Length - 4) + ".wsformat.png"), ImageFormat.Png)

    0