open System  
open System.Diagnostics  
open System.Numerics  
  
// Long Factorial  
let rec FactorialInt64(n:int): int64 =  
    match n with  
    | 1 -> int64(1)  
    | n -> int64(n) * FactorialInt64(n - 1)  
  
// Double Factorial  
let rec FactorialDouble(n:int): double =  
    match n with  
    | 1 -> double(1)  
    | n -> double(n) * FactorialDouble(n - 1)  
  
// BigInteger Factorial  
let rec FactorialBigInteger(n:int): bigint =  
    match n with  
    | 1 -> bigint(1)  
    | n -> bigint(n) * FactorialBigInteger(n - 1)  
  
let timer:Stopwatch = new Stopwatch()  
let mutable facIntResult:int64 = int64(0)  
let mutable facDblResult:double = double(0)  
let mutable facBigResult:bigint = bigint(0)  
let mutable i:int = 0  
  
let values:list<int> = [5..5..50]  
  
printfn "\nFactorial using Int64"  
// Benchmark Factorial using Int64  
for i in values do  
    timer.Start();    
    facIntResult <- FactorialInt64(i)  
    timer.Stop();   
    printfn "(%d) = %s : %s" i (timer.Elapsed.ToString()) (facIntResult.ToString())  
  
printfn "\nFactorial using Double"  
// Benchmark Factorial using Double  
for i in values do  
    timer.Start();    
    facDblResult <- FactorialDouble(i)  
    timer.Stop();   
    printfn "(%d) = %s : %s" i (timer.Elapsed.ToString()) (facDblResult.ToString())  
  
printfn "\nFactorial using BigInteger"  
// Benchmark Factorial using Double  
for i in values do  
    timer.Start();  
    facBigResult <- FactorialBigInteger(i)  
    timer.Stop();   
    printfn "(%d) = %s : %s" i (timer.Elapsed.ToString()) (facBigResult.ToString())  