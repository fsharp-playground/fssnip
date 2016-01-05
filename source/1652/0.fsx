open System
open System.IO
open System.Data
open System.Collections.Generic
open Excel

/// Statistic types
type Stat =
   | Employed
   | Unemployed
   | MedianIncome
   | LaborForce

/// Record we can use for holding the data
type AreaStatistic = {
   State : string
   AreaName : string
   YearlyStats : (int * (Stat * float option)) list
   }

/// Reads a float value from the row at the specified column index
let ReadColumn(r:DataRow,i) =
   let s = r.ItemArray.[i].ToString()
   if String.IsNullOrEmpty(s) || String.IsNullOrWhiteSpace(s)
   then None
   else Some(Double.Parse(s))

/// Gets stats for a particular year
let GetStatsForYear(r:DataRow, year, i) =
   [year, (LaborForce, ReadColumn(r,i))
    year, (Employed, ReadColumn(r,i+1))
    year, (Unemployed, ReadColumn(r,i+2))]

/// Converts a single row into the AreaStatistic type
let ConvertRowToStat(r:DataRow) = 
   { State = r.ItemArray.[1].ToString()
     AreaName = r.ItemArray.[2].ToString()
     YearlyStats =     
      [for i in 0..13 do yield! GetStatsForYear(r,(2000+i),9+(i*4))
       yield (2013, (MedianIncome,  ReadColumn(r,65)))] }
   
/// Reads in the data and returns a list of statistics
let ReadInData (url:string) =
   let stats = List<AreaStatistic>()
   use reader = new StreamReader(url)
   let excelReader = ExcelReaderFactory.CreateBinaryReader(reader.BaseStream)
   let d = excelReader.AsDataSet()
   for dt in d.Tables do
      for r in dt.Rows do
         try stats.Add(ConvertRowToStat(r))
         with e -> Console.WriteLine(e.Message)
   stats

ReadInData @"C:\temp\Unemployment.xls"