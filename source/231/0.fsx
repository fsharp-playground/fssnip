open System
open System.IO
open System.Xml
open System.Text
open System.Net
open System.Globalization

let makeUrl symbol (dfrom:DateTime) (dto:DateTime) = 
    //Uses the not-so-known chart-data:
    new Uri("http://ichart.finance.yahoo.com/table.csv?s=" + symbol + 
       "&e=" + dto.Day.ToString() + "&d=" + dto.Month.ToString() + "&f=" + dto.Year.ToString() +
       "&g=d&b=" + dfrom.Day.ToString() + "&a=" + dfrom.Month.ToString() + "&c=" + dfrom.Year.ToString() + 
       "&ignore=.csv")

let fetch (url : Uri) = 
    let req = WebRequest.Create (url) :?> HttpWebRequest
    use stream = req.GetResponse().GetResponseStream()
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

let reformat (response:string) = 
    let split (mark:char) (data:string) = 
        data.Split(mark) |> Array.toList
    response |> split '\n' 
    |> List.filter (fun f -> f<>"") 
    |> List.map (split ',') 
    
let getRequest uri = (fetch >> reformat) uri

type MarketData =
    {Date: DateTime;
     Open: double;
     High: double;
     Low: double;
     Close: double;
     Volume: int;
     AdjClose: double;}

let converter (data:List<List<string>>) =
    [for dataArray in data.Tail do
        yield {Date = DateTime.ParseExact(dataArray.[0],"yyyy-MM-dd",CultureInfo.InvariantCulture);
          Open = System.Convert.ToDouble(dataArray.[1]);
          High = System.Convert.ToDouble(dataArray.[2]);
          Low = System.Convert.ToDouble(dataArray.[3]);
          Close = System.Convert.ToDouble(dataArray.[4]);
          Volume = System.Convert.ToInt32(dataArray.[5]);
          AdjClose = System.Convert.ToDouble(dataArray.[6])}]

//Example: Microsoft, from 2010-03-20 to 2010-04-21
//getMarketData "MSFT" (new DateTime(2010,1,1)) (new DateTime(2010,1,30));;
let getMarketData symbol fromDate toDate = makeUrl symbol fromDate toDate  |> getRequest |> converter

//highLowVolatility "MSFT" (new DateTime(2010,1,1)) (new DateTime(2010,1,30));;
let highLowVolatility symbol (fromDate:DateTime) (toDate:DateTime) =
    let data = getMarketData symbol fromDate toDate
    data
    |> List.fold (fun acc mktdata -> acc + Math.Log(mktdata.High / mktdata.Low) )  0.0
    |> (*) ( 1.0 / ( ( 2.0 * System.Convert.ToDouble(data.Length)) * System.Math.Sqrt(System.Math.Log(2.0)) ) )
    |> (*) (System.Math.Sqrt 252.0)

//closeVolatility "MSFT" (new DateTime(2010,1,1)) (new DateTime(2010,1,30));;
let closeVolatility symbol (fromDate:DateTime) (toDate:DateTime) =
        let data = getMarketData symbol fromDate toDate
        let rec close (dt: MarketData list)  =
            match dt with
            | x::y::[] -> System.Math.Pow(System.Math.Log((y.Close / x.Close)),2.0) 
            | x::y::tail -> System.Math.Pow(System.Math.Log((y.Close / x.Close)),2.0)  + (close (List.append [y] tail))
            | [] -> 0.0
  
        let rec close1 (dt: MarketData list)  =
            match dt with
            | x::y::[] -> System.Math.Log(y.Close / x.Close)
            | x::y::tail -> System.Math.Log(y.Close / x.Close)  + (close1 (List.append [y] tail))
            | [] -> 0.0
        
        let firstValue = data
                            |> close
                            |> (*) (1.0 / (System.Convert.ToDouble(data.Length - 1) - 1.0))
                           
        let secondValue = (System.Math.Pow((close1 data), 2.0)) * ( 1.0 / (System.Convert.ToDouble(data.Length - 1) *(System.Convert.ToDouble(data.Length - 1) - 1.0 )))
       
        System.Math.Sqrt (firstValue - secondValue) * System.Math.Sqrt(252.0)     

//highLowCloseVolatility "MSFT" (new DateTime(2010,1,1)) (new DateTime(2010,1,30));;
let highLowCloseVolatility symbol (fromDate:DateTime) (toDate:DateTime) =
        let data = getMarketData symbol fromDate toDate
        let highLow (dt: MarketData list) = 
                    dt
                    |> List.fold (fun acc x -> acc + (0.5) * System.Math.Pow(System.Math.Log(x.High / x.Low),2.0))  0.0
                    |> (*) ( 1.0 / System.Convert.ToDouble(data.Length - 1))

        let rec closeCalc (dt: MarketData list)  =
                    match dt with
                    | x::y::[] -> ((2.0 * System.Math.Log(2.0)) - 1.0) * System.Math.Pow(System.Math.Log((y.Close / x.Close)),2.0) 
                    | x::y::tail -> ((2.0 * System.Math.Log(2.0)) - 1.0) * System.Math.Pow(System.Math.Log((y.Close / x.Close)),2.0)  + (closeCalc (List.append [y] tail))
                    | [] -> 0.0

        let close (dt: MarketData list) =
                dt
                |> closeCalc
                |> (*) ( 1.0 / System.Convert.ToDouble(data.Length - 1))

        System.Math.Sqrt(252.0) * System.Math.Sqrt((highLow data.Tail) - (close data))
