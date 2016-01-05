//Memory Address,you may get this address from some other operations
//not sure if this is readable safe address
let p = 0x002100000n

//Pointer to the Address
let address : nativeptr<int32> = Microsoft.FSharp.NativeInterop.NativePtr.ofNativeInt(p)

//set the value which is stored in the memory with given address and offset to 0xA
let setV = Microsoft.FSharp.NativeInterop.NativePtr.set(address) 0 0xA

//get the changed value
let result = Microsoft.FSharp.NativeInterop.NativePtr.read(address)
