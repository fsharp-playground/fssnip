module Assertions

open System
open System.IO
open System.Diagnostics
open System.Windows
open Microsoft.VisualStudio.TestTools.UnitTesting    

/// Opens a set of files in a diff viewer.
/// Change to suit your diff viewer
let openDiff file1 file2 =
    let startInfo = new ProcessStartInfo()
    startInfo.FileName <- @"C:\Program Files\Perforce\p4merge.exe"
    startInfo.Arguments <- sprintf "%s %s" file1 file2
    Process.Start(startInfo);

/// Useable tempfile (see: http://fssnip.net/4N/)
type TempFile() =
     let path = System.IO.Path.GetTempFileName()
     member x.Path = path
     interface System.IDisposable with
         member x.Dispose() = System.IO.File.Delete(path)

/// Predicate for matching strings.
let stringsDiffer s1 s2 = 
    not (s1 = s2)

/// Uses a tester function to determine 
/// if the given strings, s1 and s2 match.
let testStrings' testFails expected actual  = 

    let diffMessage = 
        Environment.NewLine + "-------------------------" + 
        Environment.NewLine + 
        "This string has been copied to the clipboard." + Environment.NewLine + 
        "To approve, simply paste it into your test."
    
    // Escape this to create a verabtim F# string that
    // may be pasted directly into the test code.
    let buildEscapedStringForClipboard (str:string) = 
        let doubleQuotesEscaped = str.Replace(@"""", @"""""")
        sprintf @"@""%s""" doubleQuotesEscaped

    let copyToClipboard str = 
        try 
            System.Windows.Clipboard.SetData(
                DataFormats.Text, 
                buildEscapedStringForClipboard str);
        with 
            | ex -> () // swallow any COM exceptions here. ... 

    // True if a debugger is attached to the 
    // process running the tests.
    let debuggerIsAttached =                 
        Debugger.IsAttached

    if not debuggerIsAttached then
        Assert.AreEqual(expected, actual) // Assume running on CI server.
    else
        if testFails expected actual
        then // Show any change in diff viewer and copy received to clipboard.
            use tempFileExpected = new TempFile()
            use tempFileActual   = new TempFile()
            let tempFiles = [| tempFileExpected.Path; tempFileActual.Path |] 
            [| expected; actual + diffMessage |] 
            |> Array.zip tempFiles 
            |> Array.iter (fun tpl -> File.WriteAllText((fst tpl), (snd tpl)))
            openDiff tempFileExpected.Path tempFileActual.Path |> ignore
            actual |> copyToClipboard 
            failwith (sprintf "Expected <%s>, but actual is <%s>." expected actual)

/// Tests the given strings for equality.
/// If they're found to differ, then the 
/// p4merge tools is opened to show the 
/// differences.  The received string is 
/// escaped and sent to the clipboard 
/// so that it may be easily copied to the 
/// relvant test to accept the changes if 
/// they're expected.
[<DebuggerStepThrough>]        
let IsSameStringAs = testStrings' stringsDiffer
