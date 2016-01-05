let countingBitsOn (dword : int32) : int32 =
  //Counting 1-bits using 'divide and conquer' strategy 
  //from Hacker's Delight, Henry S. Warren, Jr.
  let mutable x = dword
  x <- x - ((x >>> 1) &&& 0x55555555)
  x <- (x &&& 0x33333333) + ((x >>> 0x2) &&& 0x33333333); 
  x <- (x + (x >>> 0x4)) &&& 0x0F0F0F0F
  x <- x + (x >>> 0x8);
  x <- x + (x >>> 0x10);
  x &&& 0x0000003F

let countingBitsOff (dword : int32) : int32 = 32 - countingBitsOn dword

for x in [0xA; 0xFF; 0x100; 0x400; System.Int32.MinValue; System.Int32.MaxValue] do
    printfn "%11d #bits on = %2d #bits off = %2d" x (countingBitsOn x) (countingBitsOff x)

(*    

         10 #bits on =  2 #bits off = 30
        255 #bits on =  8 #bits off = 24
        256 #bits on =  1 #bits off = 31
       1024 #bits on =  1 #bits off = 31
-2147483648 #bits on =  1 #bits off = 31
 2147483647 #bits on = 31 #bits off =  1

*)