module Yahoo
    open System
    open System.Net 
    open System.Text 
    open System.Web
    open System.IO
    open System.Security.Authentication 
    open System.Runtime.Serialization // needs system.runtime.serialization; system.xml


(*          
use datacontract defintiion of the record for serialization.
    [<DataContract>] 
    type YahooPrice = {
        [<field: DataMember(Name="yahooSymbol") >] 
        yahooSymbol : string ;

        [<field: DataMember(Name="day") >] 
        day: string ;

        [<field: DataMember(Name="openPrice") >] 
        openPrice: float ;

        [<field: DataMember(Name="closePrice") >] 
        closePrice: float ;

        [<field: DataMember(Name="highPrice") >] 
        highPrice: float ;

        [<field: DataMember(Name="lowPrice") >] 
        lowPrice: float ;

        [<field: DataMember(Name="adjustedClosePrice") >] 
        adjustedClosePrice: float;

        [<field: DataMember(Name="volume") >] 
        volume: float;

    }
*) 

    type YahooPrice = {
        yahooSymbol : string ;
        day: string ;
        openPrice: float ;
        closePrice: float ;
        highPrice: float ;
        lowPrice: float ;
        adjustedClosePrice: float;
        volume: float;
    }

    type YahooRequest = {
        day: string;
        ticker: string ;
    }

    exception BadResults of string * string

    let historicalQuote (r:YahooRequest) = 
        
        let date: string = r.day
        let symbol: string = r.ticker

        let oops() = 
            raise(BadResults(symbol, date))

        let pad (i:int) =  
            let s = i.ToString() ; 
            if s.Length < 2 then "0" + s else s

        let yr = date.Substring(0,4)
        let mth = Int32.Parse(date.Substring(5,2)) - 1
        let day = Int32.Parse(date.Substring(8,2))
        let u = "http://ichart.finance.yahoo.com/table.csv?s=" + symbol + "&a=" + pad(mth) + "&b=" + pad(day) + "&c=" + yr + "&d=" + pad(mth) + "&e=" + pad(day) + "&f=" + yr
        let request : HttpWebRequest = downcast WebRequest.Create(u) 
        request.Method <- "GET" 
        request.ContentType <- "application/x-www-form-urlencoded" 
                
        let resultsArr =
            try
                let response = request.GetResponse()        
                let result = 
                    try 
                        use reader = new StreamReader(response.GetResponseStream()) 
                        reader.ReadToEnd(); 
                    finally 
                        response.Close() 
                if result.Split('\n').Length < 2 then oops()
                if result.Split('\n').[1].Length < 2 then oops()
                if result.Split('\n').[1].Split(',').Length < 7 then oops()
                result.Split('\n').[1].Split(',')
            with 
                | _ -> [|date; "-1"; "-1"; "-1"; "-1"; "-1"; "-1";|]

        {
            yahooSymbol  = symbol ;
            day= resultsArr.[0] ;
            openPrice= Double.Parse(resultsArr.[1]);
            closePrice= Double.Parse(resultsArr.[4]);
            highPrice= Double.Parse(resultsArr.[2]);
            lowPrice= Double.Parse(resultsArr.[3]);
            volume= Double.Parse(resultsArr.[5]);
            adjustedClosePrice= Double.Parse(resultsArr.[6]);
        }


    let valu = historicalQuote {day="2010/04/26"; ticker="MSFT";}