let fastInvSqrt (n : float32) : float32 =
    let MAGIC_NUMBER : int32 = 0x5f3759df 
    let THREE_HALVES = 1.5f
    let x2 = n * 0.5f
    let i = MAGIC_NUMBER - (System.BitConverter.ToInt32(System.BitConverter.GetBytes(n), 0) >>> 1)
    let y = System.BitConverter.ToSingle(System.BitConverter.GetBytes(i), 0)
    y * (THREE_HALVES - (x2 * y * y))

// Examples:

let x = fastInvSqrt 4.0f
// Output: val x : float32 = 0.499153584f

let x' = 1. / sqrt(4.0)
// Output: val x' : float = 0.5