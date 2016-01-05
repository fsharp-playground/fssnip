// Load type provider for CSV files
#r "Samples.Csv.dll"
open Samples.Csv

// ------------------------------------------------------------------
// TUTORIAL: Parsing and exploring the Titanic CSV data set 
// ------------------------------------------------------------------

// Download data from the web, use CSV provider to infer colum names
let [<Literal>] DataUrl = 
  "https://gist.github.com/tpetricek/263fb1bee9366170b2ef/raw/90d012bac3713e8618d3ae2f83f2f6535b6bebd9/titanic.csv"  
type Titanic = CsvFile<DataUrl, Schema="int,int,int,string,string,string,string,string,string,string,string,string">

// Load & explore the data from the web URL
let data = new Titanic()
let first = data.Data |> Seq.head

first.Name
first.Age

// Print names of surviving children
// (Note - the value of age may be missing, or silly)
for row in data.Data do
  if row.Survived = 1 && row.Age <> "" && (float row.Age) < 18.0 then
    printfn "%s (%s)" row.Name row.Age

// TASK #1: Skip suspicious floating point values 
// (You can use Contains member method to test for "."
// or you can look for values less than 1)

// TASK #2: Print names of surviving males 
// who have name longer than 40 characters


// ------------------------------------------------------------------
// TUTORIAL: Introdcing higher-order, first-class functions & collections 
// ------------------------------------------------------------------

// Helper functions that extract information from a row 
let survived (row:Titanic.Row) = 
  row.Survived = 1
  
let name (row:Titanic.Row) = 
  row.Name

let hasAge (row:Titanic.Row) = 
  (row.Age <> "") && (not (row.Age.Contains(".")))
     
let age (row:Titanic.Row) = 
  float row.Age 

// Call them on the first line
name first
hasAge first
age first

// Seq.* functions can be used to implement LINQ-like queries
// For example, get a sequence of names: 
Seq.map name data.Data

// Get count of passangers & average age on Titanic
Seq.length data.Data
Seq.average (Seq.map age (Seq.filter hasAge data.Data))

// Nicer notation using the pipelining operator
data.Data
|> Seq.filter hasAge
|> Seq.map age
|> Seq.average

// Or we can use lambda functions, which makes things easier
data.Data
|> Seq.filter (fun r -> r.Age <> "" && not (r.Age.Contains(".")))
|> Seq.averageBy (fun r -> float r.Age)

// TASK #3: Find out whether the average age of those who survived
// is greater/smaller than the average age of those who died

// ------------------------------------------------------------------
// TUTORIAL: More things to try on your own!
// ------------------------------------------------------------------

// Calculate the percentage of survivors by different embarkation point
data.Data
|> Seq.groupBy (fun row -> row.Embarked)
|> Seq.map (fun (embarked, data) ->
     let survivors =
       data |> Seq.filter (fun r -> r.Survived = 1)
            |> Seq.length
     let total = data |> Seq.length                 
     embarked, float survivors / float total * 100.0)

// TASK  #4: Calculate average age by different embarkation point
// (Use Seq.groupBy as above and then use Seq.averageBy on the 
// group 'data' as above to get average age)     