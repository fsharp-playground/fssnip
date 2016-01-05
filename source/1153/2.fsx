open System    

module native = 
  open System.Runtime.InteropServices

  [<DllImport @"boyer_moore.dll">]
  extern nativeint boyerMoore(
    nativeint data, 
    nativeint search, 
    int       datalen, 
    int       searchlen)

  let vrfy n = if n <= 0n then failwith "null ptr" else n
  
  let inline (~~) (data : GCHandle) = data.AddrOfPinnedObject()
  let inline (!~) (ptr  : GCHandle) = ptr.Free()
  
  let pin (data : 'a array)       = GCHandle.Alloc(data,GCHandleType.Pinned)

  let search (data : byte array) (search : byte array) =
    let d,s = pin data, pin search

    let ret = ref 0n
    
    lock ret (fun () ->
                ret := boyerMoore(~~d,~~s,data.Length,search.Length)
                !~d; !~s
                )
    !ret

let f1 = IO.File.ReadAllBytes @"C:\users\dklein\desktop\librhash.dll"

let sbox = [|
   27uy; 0uy; 249uy; 100uy; 246uy; 205uy; 221uy; 254uy; 226uy; 241uy; 143uy;
   124uy; 20uy; 21uy; 215uy; 17uy; 211uy; 24uy; 140uy; 139uy; 30uy; 136uy;
   223uy; 221uy|]

match native.search f1 sbox with
  | x when x > 0n -> sprintf "[s-box detected] snefru hash function at 0x%x" x
  | _             -> ""
