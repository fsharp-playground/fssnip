//Define your array
let arr = [|1;23|]
//Get the nativeint of arr.[0]
let nativeint = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(arr,0)
//Construct a intptr using the nativeptr<unit> 
let intptr = new System.IntPtr(nativeint.ToPointer())
