// [snippet:Description]
// ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ----
// -- Copy text file but omit certain lines, i.e. keep only specified lines.
// ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ---- ----

// At work we had a semicolon separated file that was too large for the
// current Excel version. We knew we didn't need the entire file anyway.
// A colleague had Visual Studio and hence F# installed on his computer,
// so we filtered away everything except the lines containing particular
// key values that we needed. Soon after we had a small filtered copy of
// the csv file that we could easily load in Excel.

// In the example below, the lines (or rows) that are copied to the new
// text file are the ones that contain either the text ";Some value;" or
// ";Some other value;" (or both), which in practice turns out to be the
// rows where one of the separated values are either "Some value" or
// "Some other value". 

// In our case we knew that the key was so particular that there would
// be no possibility that we would end up with too many lines. In other
// cases maybe it wouldn't hurt to end up with too many lines as long as
// file is short enough to be imported to Excel. 

// [/snippet]


// [snippet:Script definition]
open System.IO


/// Read all lines from UTF-8 encoded text file as a sequence.
let linesFromFile filename =
    seq { use reader = File.OpenText filename
          while not reader.EndOfStream
             do yield reader.ReadLine () }


/// Create a new UTF-8 encoded text file and
/// write all lines from a sequence to the new file.
let linesTofile filename (lines: string seq) =
    use writer = File.CreateText filename
    for line in lines
     do writer.WriteLine line


/// Filter to apply for each line.
let lineFilter keeperPhrases (line: string) =
    Array.exists line.Contains keeperPhrases

// [/snippet]


// [snippet:Usage example]
[<Literal>]
let inputfilename = @"C:\inputfile.csv"

[<Literal>]
let outputfilename = @"C:\filteredfile.csv"

let textToKeep = [| @";Some value;"; @";Some other value;" |]

// When the final do command is issued the first file, named "inputfile.csv",
// is made into a sequence of text lines (strings) that are read as they are
// needed.
// The sequence is filtered, so that some text lines are kept while others
// are skipped. The lines that are kept are those that contains the text
// ";Some value;" or ";Some other value;" (or both) somewhere in the line.
// The kept lines are written to the output file, named "filteredfile.csv",
// as the the input file is processed.

// Read, filter and write sequence:
do linesFromFile inputfilename
   |> Seq.filter (lineFilter textToKeep)
   |> linesTofile outputfilename

// [/snippet]
