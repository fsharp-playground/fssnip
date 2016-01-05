open System
open System.IO
open System.Security.Cryptography

let types = [|"MD5"; "SHA1"; "SHA256"; "SHA384"; "SHA512"; "RIPEMD160"|]

for arg in fsi.CommandLineArgs |> Seq.skip 1 do
  let itm = Path.GetFullPath(arg)

  if File.Exists(itm) then
    if (new FileInfo(itm)).Length <> 0L then
      printfn "\n%s:" itm
      let content = File.ReadAllBytes(itm)
      for t in types do
        let bytes = content |> HashAlgorithm.Create(t).ComputeHash
        Console.WriteLine("{0, 9}: {1}", t, BitConverter.ToString(bytes).Replace("-", "").ToLower())
    else
      printfn "File %s has null length." arg
  else
    printfn "File %s does not exist." arg