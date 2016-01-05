module DirectoryExtensions

open System
open System.IO

/// As System.IO.Directory.EnumerateFiles() but ignoring any files
/// or directories which cannot be accessed, eg. because of 
/// permissions.
///
/// Always recurses into subdirectories.
///
let public SafeEnumerateFiles(path : string, searchPattern: string) =
    // Attempts to list the directories below the specified path, and fails silently (empty result) if permission denied.
    let tryEnumerateDirs path =
        try
            Directory.EnumerateDirectories(path)
        with
            // Silently fail if we don't have access to list subdirs:
            | :? System.UnauthorizedAccessException -> Seq.empty

    // Attempts to list the files the specified path, and fails silently (empty result) if permission denied.
    let tryEnumerateFiles path namePattern =
        try
            Directory.EnumerateFiles(path, namePattern)
        with
            // Silently fail if we don't have access to list files:
            | :? System.UnauthorizedAccessException -> Seq.empty

    // Lists all directories below (and including) the specified directory.
    let rec dirsUnder (basePath : string) : seq<string> =
        seq {
            yield! [|basePath|]
            for subDir in tryEnumerateDirs(basePath) do
                yield! dirsUnder subDir
        }

    dirsUnder path
    |> Seq.map (fun path -> tryEnumerateFiles path searchPattern) 
    |> Seq.concat

// Usage examples:
//
// F#: SafeEnumerateFiles(@"x:\mybigdir", "*.*") |> Seq.length
//
// C#: Console.WriteLine(DirectoryExtensions.SafeEnumerateFiles(@"x:\mybigdir", @"*.*").Count());