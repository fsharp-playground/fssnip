open System.IO

/// Custom operator for combining paths
let (+/) path1 path2 = Path.Combine(path1, path2)

// Root path obtained from some function
let root = (*[omit:(...)]*)"C:\\test"(*[/omit]*)
// Create path for a specific file
let file = root +/ "Subdirectory" +/ "Test.fsx"