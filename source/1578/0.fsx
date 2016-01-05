open System
open System.Runtime.InteropServices
open Microsoft.FSharp.NativeInterop

[<System.Runtime.InteropServices.DllImport("pgm_cpp.dll", EntryPoint="add", CallingConvention=CallingConvention.Cdecl)>]
extern void add(nativeint a, nativeint b);
let add2 x y =
    let mutable x1 = nativeint x
    let mutable y1 = nativeint y
    add(x1, y1)

// declaration
// extern "C"  __declspec(dllexport) void add(int a, int b);
// definition
// void add(int a, int b) { printf("Why?"); }