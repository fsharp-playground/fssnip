(**
Understanding the world with F#
===============================

These days, you can get access to data about almost anything you like. But to turn the
raw data into useful information is a challenge. You need to link data from different 
sources (that are rarely polished), understand the data set and build useful visualizations
that help explaining the data.

This is becoming an important task for increasing number of companies, but for the purpose
of this article, we'll work for the public good and become data journalists. Our aim
is to understand US government debt during the 20th century and see how different presidents
contributed to the debt.

F# for data science
-------------------

Why are we choosing F# for working with data? Firstly, F# is succinct and has a great
interactive programming mode, so we can easily run our analysis as we write it. Secondly,
F# has great data science libraries. After creating a new "F# Tutorial" project, you can 
import all of them by installing NuGet package `FsLab`. 

We'll write our code interactively, so let's open `Tutorial.fsx` and load the FsLab package:
*)
#load "packages/FsLab.0.0.1-beta/lib/FsLab.fsx"
open System
open FSharp.Charting
open FSharp.Data
open FSharp.DataFrame
open RProvider
(**
You'll need to update the number in the `#load` command. The next lines open the namespaces
of four F# projects that we'll use in this article:

 - **F# Charting** is a simple library for visualization and charting based on .NET charting API
 - **Deedle** is a data frame and time series manipulation library that we'll need to combine data 
   from different sources and to get basic idea about the data structure
 - **F# Data** is a library of type providers that we'll use to get information about US presidents
   and to read US debt data from a CSV file 
 - **R type provider** makes it possible to interoperate with statistical system named R, 
   which implements advanced statistical and visualization tools used by professional statisticians

Reading government debt data
----------------------------

To plot the US government debts during different presidencies, we need to combine two 
data sources. The easy part is getting historical debt data - we use a CSV file downloaded
from [usgovernmentspending.com](http://www.usgovernmentspending.com). The F# Data library
also makes it easy to get data from the World Bank, but sadly, World Bank does not have
historical US data. Reading CSV file is equally easy though:
*)
type UsDebt = CsvProvider<"C:\Data\us-debt.csv">
let csv = UsDebt.Load("C:\Data\us-debt.csv")
let debtSeries = 
  series [ for row in csv.Data ->
             row.Year, row.``Debt (percent GDP)`` ] 
(**
The snippet uses CSV type provider to read the file. The type provider looks at a 
sample CSV file (specified on the first line) and generates a type `UsDebt` that 
can be used to read CSV files with the same structure. This means that when loading
the data on the second line, we could use a live URL for the CSV file rather than
a local copy.

The next line shows the benefit of the type provider - when iterating over rows,
the `row` value has properties based on the names of columns in the sample CSV file.
This means that ` ``Debt (percent GDP)`` ` is statically checked. The compiler would
warn us if we typed the column name incorrectly (and IntelliSense shows it as an
ordinary property). The double-backtick notation is just an F# way of wrapping arbitrary
characters (like spaces) in a property name.

Finally, the notation `series [ .. ]` turns tha data into a Deedle time series
that we can turn into a data frame and plot: 
*)
let debt = Frame.ofColumns [ "Debt" => debtSeries ] 
Chart.Line(debt?Debt)
(**
The first line creates a data frame with single column named "Debt". A data frame
is a bit like database table - a 2D data structure with index (here years) and
multiple columns. We started with just a single column, but will add more later.

Once we have data in a data frame, we can use `debt?Debt` to get a specified
column. By passing the series to `Chart.Line` we get a chart that looks like this:

<div style="text-align:center">
<img src="debt-by-year.png" style="width:612px;margin-left:auto;margin-right:auto" />
</div>

Listing US presidents
---------------------

To get information about US presidents, you could go to Wikipedia and spend the next
5 minutes typing the data. Not a big deal, but it does not scale! Instead, we'll use
another type provider and get the data from Freebase. Freebase is a collaboratively
created knowledge base - a bit like Wikipedia, but with well-defined data schema.

The F# type provider for Freebase exposes the data as F# types with properties, so 
we can start at the root and look at `Society`, `Government` and `US Presidents`:
*)
let fb = FreebaseData.GetDataContext()
let presidentInfos = 
  query { for p in fb.Society.Government.``US Presidents`` do
          sortBy (Seq.max p.``President number``)
          skip 23 }
(**
The code uses F# implementation of LINQ to order the presidents by their number and
skip the first 23. This gives us William McKinley whose presidency started in 1897 
as the first one. As usual in LINQ, the query is executed on the servere-side (by
Freebase). The next step is to find the start and end year of the terms in the office.

Each object in the `presidentInfos` has a type representing US politician, which 
has the `Government Position Held` property. This means that we can iterate over
all their official positions, find the one titled "President" and then get the 
`From` and `To` values: 
*)
let presidentTerms =
  [ for pres in presidentInfos do
    for pos in pres.``Government Positions Held`` do
    if string pos.``Basic title`` = "President" then
      // Get start and end year of the position
      let starty = DateTime.Parse(pos.From).Year
      let endy = if pos.To = null then 2013 else
                   DateTime.Parse(pos.To).Year
      // Return three element tuple with the info
      yield (pres.Name, starty, endy) ]
(**
The code uses F# sequence expressions, which are quite similar to C# iterators. 
When we have a president and a position, we can get the years of `From` and `To` 
values. The only difficulty is that `To` is `null` for the current president - 
so we simply return 2013.

Now we have the data as an ordinary F# list, but we need to turn it into a Deedle
data frame, so that we can combine it with the debt numbers:
*)
let presidents =
  presidentTerms
  |> Frame.ofRecords
  |> Frame.indexColsWith ["President"; "Start"; "End"]
(**
The function `Frame.ofRecords` takes a collection of any .NET objects 
and creates data frame with columns based on the properties of the type. 
We get a data frame with `Item1`, `Item2` and `Item3`. The last line
renames the columns to more useful names. Here, we also use the pipelining
operator `|>`, which is used to apply multiple operations in a sequence.

If you now type `presidents` in F# Interactive, you'll see a nicely formatted
table with the last 20 US presidents and their start and end years. 

Analysing debt change
---------------------

Before going furter, let's do a quick analysis to find out how the government debt 
changed during the terms of different presidents. To do the calculation, we take
the data frame `presidents` and add debt at the end of the term. 

The data frame provides a powereful "joining" operation that can align data from
different data frames. To use it, we need to create a data frame like `presidents`
which has the end of the office year as the index (the index of the frame created
previously is just the number of the row):
*)
let byEnd = presidents |> Frame.indexRowsInt "End"
let endDebt = byEnd.Join(debt, JoinKind.Left)
(**
The data frame `byEnd` is indexed by years and so we can now use `Join` to add
`debt` data to the frame. The `JoinKind.Left` parameter specifies that we want to
find a debt value for each key (presidency end year) in the left data frame.
The result `endDebt` is a data frame containing all presidents (column `President`)
together with the debt at the end of their presidency (column `Debt`).

Now we add one more column that represents the difference between the debt at the
of the presidency and the debt at the beginning. This is done by getting the
`endDebt?Debt` series and using an operation that calls a specified function for each 
pair of consecutive values:
*)
endDebt?Difference <-
  endDebt?Debt |> Series.pairwiseWith (fun _ (prev, curr) -> curr - prev)
(**
The data frame structure is mostly immutable, with the only exception - it is possible
to add and remove columns. The above line adds a column `Difference` that is calculated
by subtracting the previous debt value from the current debt value. If you now evaluate
`endDebt` in F# interactive (select it and hit Alt+Enter or type `endDebt;;` in the console)
you will see the following table:

<div style="height:300px;overflow-y:scroll">
<table class="table table-bordered table-striped">
<thead style="font-weight:bold">
<tr>
  <td></td><td>President             </td><td>Start </td><td>End  </td><td>Debt   </td><td>Difference         </td>
</tr></thead><tbody><tr>
  <td>1901</td><td>William McKinley     </td><td>1897 </td><td>1901</td><td> 18.60</td><td>  &lt;missing&gt;</td>
</tr><tr>
  <td>1909</td><td>Theodore Roosevelt   </td><td>1901 </td><td>1909</td><td> 18.65</td><td>  0.050          </td>
</tr><tr>
  <td>1913</td><td>William Howard Taft  </td><td>1909 </td><td>1913</td><td> 18.73</td><td>  0.080          </td>
</tr><tr>
  <td>1921</td><td>Woodrow Wilson       </td><td>1913 </td><td>1921</td><td> 45.10</td><td>  26.37          </td>
</tr><tr>
  <td>1923</td><td>Warren G. Harding    </td><td>1921 </td><td>1923</td><td> 38.95</td><td>  -6.15          </td>
</tr><tr>
  <td>1929</td><td>Calvin Coolidge      </td><td>1923 </td><td>1929</td><td> 32.25</td><td>  -6.7           </td>
</tr><tr>
  <td>1933</td><td>Herbert Hoover       </td><td>1929 </td><td>1933</td><td> 73.77</td><td>  41.52          </td>
</tr><tr>
  <td>1945</td><td>Franklin D. Roosevelt</td><td>1933 </td><td>1945</td><td> 124.1</td><td>6 50.39          </td>
</tr><tr>
  <td>1953</td><td>Harry S. Truman      </td><td>1945 </td><td>1953</td><td> 79.03</td><td>  -45.1          </td>
</tr><tr>
  <td>1961</td><td>Dwight D. Eisenhower </td><td>1953 </td><td>1961</td><td> 67.49</td><td>  -11.5          </td>
</tr><tr>
  <td>1963</td><td>John F. Kennedy      </td><td>1961 </td><td>1963</td><td> 64.00</td><td>  -3.49          </td>
</tr><tr>
  <td>1969</td><td>Lyndon B. Johnson    </td><td>1963 </td><td>1969</td><td> 50.72</td><td>  -13.2          </td>
</tr><tr>
  <td>1974</td><td>Richard Nixon        </td><td>1969 </td><td>1974</td><td> 46.01</td><td>  -4.71          </td>
</tr><tr>
  <td>1977</td><td>Gerald Ford          </td><td>1974 </td><td>1977</td><td> 47.55</td><td>  1.54           </td>
</tr><tr>
  <td>1981</td><td>Jimmy Carter         </td><td>1977 </td><td>1981</td><td> 43.46</td><td>  -4.09          </td>
</tr><tr>
  <td>1989</td><td>Ronald Reagan        </td><td>1981 </td><td>1989</td><td> 66.89</td><td>  23.43          </td>
</tr><tr>
  <td>1993</td><td>George H. W. Bush    </td><td>1989 </td><td>1993</td><td> 80.52</td><td>  13.63          </td>
</tr><tr>
  <td>2001</td><td>Bill Clinton         </td><td>1993 </td><td>2001</td><td> 71.17</td><td>  -9.35</td>
</tr><tr>
  <td>2009</td><td>George W. Bush       </td><td>2001 </td><td>2009</td><td> 104.47</td><td> 33.3 </td>
</tr><tr>
  <td>2013</td><td>Barack Obama         </td><td>2009 </td><td>2013</td><td> 124.84</td><td> 20.37</td>
</tr></tbody>
</table>
</div>

One of the main benefits of using the Doodle library is that we do not explicitly 
align the data. This is done automatically based on the index. For example, the 
library knows that `Difference` column starts from the second value (because we do
not have the previous debt for McKinley).

Plotting debt by president 
--------------------------

Our next step is to visualize the combined data. We want to draw a chart similar
to the one earlier, but with differently coloured areas, depending on who was the
president during the term. We'll also add a label, showing the president's name
and election year.

To do this, we build a data frame that contains the debt for each year, but adds
the name of the current president in a separate column. This means that we want
to repeat the president's name for each year in the office. Once we have this, we
can group the data and create a chart for each group.

First, we need to use `Join` again. This time, we use the `Start` column as the
index for each president and then use left join on the `debt` data frame. This means
that we want to find the current president for every year since 1900:
*)
let byStart = presidents |> Frame.indexRowsInt "Start"
let aligned = debt.Join(byStart, JoinKind.Left, Lookup.NearestSmaller)
(**
The data frame `byStart` only contains values for years when the president was
elected. By specifying `Lookup.NearestSmaller`, we tell the `Join` operation that
it should find the president at the exact year, or at the nearest smaller year 
(when we have debt for a given year, the president is the most recently elected
president).

This means that `aligned` now has debt and a current president (together with 
his or her start and end years) for each year. To build a nicer chart, we make
one more step - we create a series that contains the name of the president together
with their start year (as a string) to make the chart labels more useful:
*)
let infos = aligned.Rows |> Series.map (fun _ row ->
  sprintf "%s (%d)" (row.GetAs "President") (row.GetAs "Start"))
(**
The snippet uses `aligned.Rows` to get all the rows from the data frame as a series
(containing individual rows as nested series). Then it formats each row into a string
containing the name and the start. The `GetAs` method is used to get column of a 
specified type and cast it to an appropriate type (here, the type is determined by
the format strings `%s` and `%d`, because `sprintf` is fully type checked).

The last step is to turn the `aligned` data frame into chunks (or groups) based on 
the current president and then draw a chart for each chunk. Because the data is ordered,
we can use `Series.chunkWhile` which creates consecutive chunks of a time series.
The chunks are determined by a predicate on keys (years):
*)
let chunked = aligned?Debt |> Series.chunkWhile(fun y1 y2 -> 
  infos.[y1] = infos.[y2])
(**
In our example, the predicate checks that the president for the first year of the 
chunk (`y1`) is the same as the president for the last year of the chunk (`y2`).
This means that we group the debt values (obtained using `aligned?Debt`) into chunks
that have the same president. For each chunk, we get a series with debts during the
presidency. Now we have everything we need to plot the data:
*)
chunked 
|> Series.observations
|> Seq.map (fun (startYear, chunkDebts) -> 
    Chart.Area(chunkDebts, Name=infos.[startYear]))
|> Chart.Combine
(**
The snippet takes all observations from the chunked series as tuples and iterates
over them using standard `Seq.map` function. Each observation has `startYear` of the
chunk together with a series `chunkDebts`. We turn each chunk into an area chart
with the corresponding president's name as a label. Finally, all charts are combined
into a single one (using `Chart.Combine`), which gives us the following result:

<div style="text-align:center">
<img src="debt-by-president-area.png" style="width:702px;margin-left:auto;margin-right:auto" />
</div>

Visualizing data using R
------------------------
Aside from using great F# and .NET libraries, we can also interoperate with a wide
range of other non-.NET systems. In our last example, we have a quick look at the statistical
system R. This is an environment used by many professional data scientists - thanks
to the R type provider, we can easily call it from F# and get access to the powerful
libraries and visualization tools available in R.

The installed R packages are automatically mapped to F# namespaces, so we start by 
opening the `base` and `ggplot2` packages:
*)
open RProvider.``base``
open RProvider.ggplot2
(**
The `ggplot2` package is a popular visualization and data exploration library. We can
use it to quickly build a chart similar to the one from the previous section. To do this,
we need to construct a dictionary with parameters and then call `R.qplot` (to build
the chart) followed by `R.print` (to display it):
*)
namedParams [
  "x", box aligned.RowKeys
  "y", box (Series.values aligned?Debt)
  "colour", box (Series.values infos) 
  "geom",  box [| "line"; "point" |] ]
|> R.qplot
|> R.print
(**
The `namedParams` function is used to build a dictionary of arguments for functions
with variable number of parameters. Here, we specify `x` and `y` data series (years and
corresponding debts, respectively) and we set `colour` parameter to the series with
president names (so that the colour is determined by the president). We also instruct
the function to plot the data as a combination of "line" and "point" geometries, which
gives us the following result:

<div style="text-align:center">
<img src="debt-by-president-r.png" style="width:705px;margin-left:auto;margin-right:auto" />
</div>

Summary
-------

With a bit of more work, we could turn our analysis into a newspaper article. The only
missing piece is breaking the data down by the political parties. If you play with the
Freebase type provider, you'll soon discover that there is a `Party` property on the object
representing presidents, so you can get the 


`p.Party`

*)