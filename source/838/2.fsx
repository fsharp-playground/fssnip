namespace ezmuze
//#r "FSharp.PowerPack"
open System
open System.Collections.Generic
open System.Linq
open System.Text
open Microsoft.FSharp.NativeInterop
#nowarn "9"

type FilterType =
    | LowPass
    | HighPass
    | BandPass
    | Notch
    | AllPass

type MammothFilter(filterType : FilterType) =
    let mutable _filterType = filterType
    let filtCoefTab = Array.zeroCreate<float> 5
    let mutable lx1 as lx2 as ly1 as ly2 = 0. // Left sample history
    let mutable outM as temp_y = 0

    member val Param_cutoff = 0. with get, set
    member val Param_resonance = 0. with get, set
    member o.FilterType = _filterType

    member o.SetFilter(filterType : FilterType) =
        _filterType <- filterType

    member o.setParams(param_cutoff : float, param_resonance : float) =
        let mutable alpha as omega as sn as cs = 0.
        let mutable a0 as a1 as a2 as b0 as b1 as b2 = 0.
        // These limits the cutoff frequency and resonance to
        // reasoneable values.
        let param_cutoff = param_cutoff * 22000.
        let param_resonance = param_resonance * 127.
        let param_cutoff =
            match param_cutoff with
            | p when p < 100. -> 100. 
            | p when p > 22000. -> 22000. 
            | p -> p
        let param_resonance =
            match param_resonance with
            | p when p < 1. -> 1. 
            | p when p > 127. -> 127. 
            | p -> p
        o.Param_cutoff <- param_cutoff
        o.Param_resonance <- param_resonance;
        //System.Diagnostics.Debug.WriteLine("c:" + Param_cutoff + " r:" + Param_resonance)
        omega <- (2.0 * Math.PI * o.Param_cutoff / 44100.)
        (*sn = 1.27323954 * omega - 0.405284735 * omega * omega;
        omega += 1.57079632;
        if omega > 3.14159265 then omega <- omega - 6.28318531
        if omega < 0 then cs = 1.27323954f * omega + 0.405284735.f * omega * omega;
        cs <- 1.27323954f * omega - 0.405284735f * omega * omega;
        *)
        sn <- sin omega
        cs <- cos omega
        alpha <- sn / (param_resonance / 16.)
        match _filterType with
        | LowPass ->
            b0 <- (1.0 - cs) / 2.0
            b1 <- 1.0 - cs
            b2 <- (1.0 - cs) / 2.0
            a0 <- 1.0 + alpha
            a1 <- -2.0 * cs
            a2 <- 1.0 - alpha
        | HighPass ->
            b0 <- (1. + cs) / 2.
            b1 <- -(1. + cs)
            b2 <- (1. + cs) / 2.
            a0 <- 1. + alpha
            a1 <- -2. * cs
            a2 <- 1. - alpha
        | BandPass ->
            b0 <- alpha
            b1 <- 0.
            b2 <- -alpha
            a0 <- 1. + alpha
            a1 <- -2. * cs
            a2 <- 1. - alpha
        | AllPass ->
            b0 <- 1. - alpha
            b1 <- -2. * cs
            b2 <- 1. + alpha
            a0 <- 1. + alpha
            a1 <- -2. * cs
            a2 <- 1. - alpha
        | Notch ->
            b0 <- 1.;
            b1 <- -2. * cs
            b2 <- 1.
            a0 <- 1. + alpha
            a1 <- -2. * cs
            a2 <- 1. - alpha
        filtCoefTab.[0] <- b0 / a0
        filtCoefTab.[1] <- b1 / a0
        filtCoefTab.[2] <- b2 / a0
        filtCoefTab.[3] <- -a1 / a0
        filtCoefTab.[4] <- -a2 / a0

    member o.ProcessShort(inputValue : int16) : int16 =
        outM <- int inputValue
        temp_y <- int (filtCoefTab.[0] * float outM +
                       filtCoefTab.[1] * lx1 +
                       filtCoefTab.[2] * lx2 +
                       filtCoefTab.[3] * ly1 +
                       filtCoefTab.[4] * ly2)
        ly2 <- ly1
        ly1 <- float temp_y
        lx2 <- lx1
        lx1 <- float outM
        outM <- temp_y
        int16 outM
    
    member o.ProcessShortArray(inputValue : int16[]) : int16[] =
#if USE_UNSAFE
        use pInputValue = PinnedArray.of_array inputValue
        let length = inputValue.Length
        let pSrc = pInputValue.Ptr
        let fCT0 = filtCoefTab.[0]
        let fCT1 = filtCoefTab.[1]
        let fCT2 = filtCoefTab.[2]
        let fCT3 = filtCoefTab.[3]
        let fCT4 = filtCoefTab.[4]
        for i = 0 to length - 1 do
            outM <- int <| NativePtr.read pSrc
            temp_y <- int (fCT0 * float outM +
                            fCT1 * lx1 +
                            fCT2 * lx2 +
                            fCT3 * ly1 +
                            fCT4 * ly2)
            ly2 <- ly1
            ly1 <- float temp_y
            lx2 <- lx1
            lx1 <- float outM
            outM <- temp_y
            NativePtr.set pSrc 1 <| int16 outM
#else
        for i = 0 to inputValue.Length - 1 do
            outM <- int inputValue.[i]
            temp_y <- int (filtCoefTab.[0] * float outM +
                           filtCoefTab.[1] * lx1 +
                           filtCoefTab.[2] * lx2 +
                           filtCoefTab.[3] * ly1 +
                           filtCoefTab.[4] * ly2)
            ly2 <- ly1
            ly1 <- float temp_y
            lx2 <- lx1
            lx1 <- float outM
            outM <- temp_y
            inputValue.[i] <- int16 outM
#endif
        inputValue

    member o.ProcessShortArray(pInputValue : nativeptr<int16>, length : int) =
        let mutable pSrc = pInputValue
        let fCT0 = filtCoefTab.[0]
        let fCT1 = filtCoefTab.[1]
        let fCT2 = filtCoefTab.[2]
        let fCT3 = filtCoefTab.[3]
        let fCT4 = filtCoefTab.[4]
        for i = 0 to length - 1 do
            outM <- int <| NativePtr.read pSrc
            temp_y <- int (fCT0 * float outM +
                           fCT1 * lx1 +
                           fCT2 * lx2 +
                           fCT3 * ly1 +
                           fCT4 * ly2)
            ly2 <- ly1
            ly1 <- float temp_y
            lx2 <- lx1
            lx1 <- float outM
            outM <- temp_y
            NativePtr.set pSrc 1 <| int16 outM            

    member o.FTfromInt(i : int) : FilterType =
        match i with
        | 0 -> LowPass
        | 1 -> HighPass
        | 2 -> BandPass
        | 3 -> Notch
        | _ -> AllPass