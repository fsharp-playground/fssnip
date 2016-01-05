open System

//[snippet:timing function]
//Just a short snippet to measure time spent in an eager function
//Not gonna work with a lazy function, e.g. function returning a sequence
let time jobName job = 
    let startTime = DateTime.Now;
    let returnValue = job()
    let endTime = DateTime.Now;
    printfn "%s took %d ms" jobName (int((endTime - startTime).TotalMilliseconds))
    returnValue
//[/snippet]