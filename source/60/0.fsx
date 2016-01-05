open System

let alphabet = "abcdefgijkmnopqrstwxyzABCDEFGHJKLMNPQRSTWXYZ23456789"
let rng = new Security.Cryptography.RNGCryptoServiceProvider()

Console.WriteLine("How long?")
let len = Int32.Parse(Console.ReadLine())

let byteArray = Array.init (4 * len) (fun _ -> (byte) 0)

rng.GetBytes(byteArray)

seq { for i in 1 .. len do yield BitConverter.ToUInt32(byteArray, (i-1) * 4) } // Get chars
    |> Seq.map (fun x -> x % (uint32) alphabet.Length) // Map into admissible range with "almost even" distribution
    |> Seq.map (fun x -> alphabet.[(int) x]) // Map alphabet indices to alphabet chars
    |> Seq.iter (printf "%c") // Print chars to console