// Load type provider for CSV files
#r "Samples.Csv.dll"
open Samples.Csv

// Download data from the web with CSV provider
let [<Literal>] DataUrl = 
  "https://gist.github.com/tpetricek/263fb1bee9366170b2ef/raw/90d012bac3713e8618d3ae2f83f2f6535b6bebd9/titanic.csv"  
type Titanic = CsvFile<DataUrl, Schema="int,int,int,string,string,string,int,string,string,string,string,string">
let data = new Titanic()

// ------------------------------------------------------------------
// TUTORIAL: Qualifying passengers with decision tree 
// ------------------------------------------------------------------

// What is a feature? A feature is something that classifies a person
// into two groups - the result of calculating a feature on a given
// row is true or false. In F#, we can use a function.
type Feature = Titanic.Row -> bool

// The syntax ":" is a type annotation. We give the compiler hint,
// so that it knows what 'row' is later in the code.
let longName : Feature = (fun row -> row.Name.Length > 25)
let lowClass : Feature = (fun row -> row.Pclass = 3)
let ageOver20 : Feature = (fun row -> row.Age <> "" && (float row.Age > 20.0))

// Get the first person and do some experiments
let first = data.Data |> Seq.head

longName first
lowClass first
ageOver20 first

// TASK #1: Write features that test the following conditions:
//  * Person has more than 2 siblings
//  * Person is over 9.5 years old
//  * Person is a male

/// Calculates how common the feature is in the data set
/// (returns the percentage of 'true' cases among all passengers)
let frequency feature =
  let counts = data.Data |> Seq.countBy feature |> dict
  (float counts.[true]) / (float (Seq.length data.Data))  

/// What is the percentage of people who survived 
/// and have the specified feature
let surviveRate feature = 
  let subset = data.Data |> Seq.filter feature
  let subsetNot = data.Data |> Seq.filter (feature >> not)
  let survived = subset |> Seq.filter (fun r -> r.Survived = 1) |> Seq.length
  let survivedNot = subsetNot |> Seq.filter (fun r -> r.Survived = 1) |> Seq.length
  (float survived) / (float (Seq.length subset)),
  (float survivedNot) / (float (Seq.length subsetNot))
  
// TASK #2: Find out which of the features best classifies the data?
// (It should be relatively common - othrewise it 'over-fits' the data
// but the survival rate should be pretty high or low)   

frequency longName 
surviveRate longName

frequency lowClass
surviveRate lowClass

// ------------------------------------------------------------------
// TUTORIAL: Qualifying passengers with decision tree 
// ------------------------------------------------------------------

// Decision tree is a simple classifier - it either branches using
// a feature, or it produces a final result. For example, see:
// http://en.wikipedia.org/wiki/Decision_tree_learning
type DecisionTree =
  | Result of bool
  | Condition of Feature * DecisionTree * DecisionTree

/// Classify a specified input using a specified decision tree
let rec classify tree row =
  match tree with 
  | Result(value) -> value
  | Condition(feature, left, right) ->
      if feature row then classify left row 
      else classify right row 

// Very simple (and silly) decision tree - person survives 
// if he/she did not travel in class 3 and has a short name
//
//     class=3?
//     /      \
//  false    name.Length>20
//             /       \
//           true     false
//
let simpleTree = 
  Condition
    ( lowClass, Result(false),
      Condition
        ( longName, Result(true), Result(false) ))

// Run the simple tree on the first person
classify simpleTree first  
// Compare this with the actual result
first.Survived
// What are the survival rates (how well it classifies?)
surviveRate (classify simpleTree)

// TASK #3: Construct a decision tree based on the sample figure
// on WikiPedia: http://en.wikipedia.org/wiki/Decision_tree_learning