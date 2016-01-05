open System.Runtime.CompilerServices
open Ferop

[<Ferop>]
[<Header("""#include <stdio.h>""")>]
module Native =
    [<Import>]
    [<MethodImpl (MethodImplOptions.NoInlining)>]
    let printHelloWorld () : unit = C """printf("Hello World!\n");"""

[<EntryPoint>]
let main args =
    Native.printHelloWorld ()
    0