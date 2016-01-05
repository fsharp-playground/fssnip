/// <summary>
/// Convenience function to raise an ArgumentException
/// </summary>
/// <param name="format">The input formatter</param>
let raiseArgEx format =
    Printf.ksprintf (fun s -> raise (System.ArgumentException(s))) format

let n = -1

if n < 0 then
    // the following code can be replaced
    // let msg = sprintf "n = %d; must be >= 0" n
    // raise(ArgumentException(msg))
    raiseArgEx "n = %d; must be >= 0" n