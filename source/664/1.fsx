open FileHelpers  
[<DelimitedRecord(",")>]
[<IgnoreFirst(1)>] 
type CsvRecord =
    class
        val field1 : string
        val field2 : int
        val field3 : string
    end
 
let engine = new FileHelperEngine<CsvRecord>()

let res = 
   try
      engine.ReadFile("test.csv")
   with
      | :? FileHelpers.FileHelpersException -> Array.empty<CsvRecord>