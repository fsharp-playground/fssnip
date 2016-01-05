module PerlinNoise
open System
open System.Drawing

let perlinNoise width height persistence octaves zoom seed handler =
    let noise x y =
        let n = x + y * 57;
        let n = (n <<< 13) ^^^ n;
        let t = (n * (n * n * 15731 + 789221) + 1376312589) &&& 0x7fffffff;
        1.0 - (double t) * 0.931322574615478515625e-9
    let interpolate x y a =
        let amount = (a * a) * (3.0 - 2.0 * a)
        x + ((y - x) * a)
    let smoothedNoise x y =
        let xi = int x
        let yi = int y
        let corners = ((noise (xi - 1) (yi - 1)) + (noise (xi + 1) (yi - 1)) + (noise (xi - 1) (yi + 1)) + (noise (xi + 1) (yi + 1))) / 16.0
        let sides = ((noise (xi - 1) yi) + (noise (xi + 1) yi) + (noise xi (yi - 1)) + (noise xi (yi + 1))) / 8.0
        let center = (noise xi yi) / 4.0
        corners + sides + center
    let interpolatedNoise x y =
        let xi = int x
        let yi = int y
        let xf = x - (float xi)
        let yf = y - (float yi)
        let v1 = smoothedNoise xi yi
        let v2 = smoothedNoise (xi + 1) yi
        let v3 = smoothedNoise xi (yi + 1)
        let v4 = smoothedNoise (xi + 1) (yi + 1)
        let i1 = interpolate v1 v2 xf
        let i2 = interpolate v3 v4 xf
        interpolate i1 i2 yf
    let pixel x y =
        seq { 0.0 .. float (octaves - 1) }
        |> Seq.sumBy (fun octave -> let frequency = (1.0/16.0) ** octave
                                    let amplitude = persistence ** octave
                                    interpolatedNoise ((float x) * frequency / zoom) ((float y) * frequency / zoom) * amplitude)
    for x in 0 .. width - 1 do
        for y in 0 .. height - 1 do
            pixel x y |> handler x y

let generate () =
    let width = 2000
    let height = 2000
    let bitmap = new Bitmap(width, height)
    let prng = new Random()
    (fun x y v -> let c = (v + 0.5) * 255.0 |> int |> min 255 |> max 0
                  bitmap.SetPixel(x, y, Color.FromArgb(c, c, c)))
    |> perlinNoise width height 0.65 6 37.42 (prng.Next() / 4)
    bitmap.Save(@"c:\users\sthalik\desktop\perlin-noise.jpg")

generate ()