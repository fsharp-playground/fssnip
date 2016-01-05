open System
open System.Linq

let Primes =
  seq { 
        let PrimesSoFar = ref [2]
        let Primality =
          fun(primes : Int32 list, number) ->
            let IsComposite = primes.AsParallel().Any(fun x-> (number % x)=0)
            (not IsComposite , primes @ (if IsComposite then [] else [number;]))
        //  Generate the Prime Number Sequence
        yield 2
        for number in 3..2..Int32.MaxValue do
          let ( IsPrime, PrimeSeq ) =  Primality(!PrimesSoFar, number)
          if IsPrime then
            PrimesSoFar := PrimeSeq
            yield number
      }

// Example
//
// let PrimeSum = Prime.Take(1000).Sum()