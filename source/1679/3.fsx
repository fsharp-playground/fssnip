type Html =
   | Elem of string * Html list
   | Attr of string * string
   | Text of string
   with
   static member toString elem =
      let rec toString indent elem =
         let spaces = String.replicate indent " "
         match elem with
         | Attr(name,value) -> name+"=\""+value+"\""
         | Elem(tag, [Text s]) ->
            spaces+"<"+tag+">"+s+"</"+tag+">\r\n"
         | Elem(tag, content) ->
            let isAttr = function Attr _ -> true | _ -> false
            let attrs, elems = content |> List.partition isAttr
            let attrs =         
               if attrs = [] then ""
               else " " + String.concat " " [for attr in attrs -> toString 0 attr]
            spaces+"<"+tag+attrs+">\r\n"+
               String.concat "" [for e in elems -> toString (indent+1) e] +
                  spaces+"</"+tag+">\r\n"
         | Text(text) ->            
            spaces + text + "\r\n"
      toString 0 elem
   override this.ToString() = Html.toString this

let elem tag content = Elem(tag,content)
let html = elem "html"
let head = elem "head"
let title = elem "title"
let style = elem "style"
let body = elem "body"
let table = elem "table"
let thead = elem "thead"
let tbody = elem "tbody"
let tfoot = elem "tfoot"
let tr = elem "tr"
let td = elem "td"
let th = elem "th"
let strong = elem "strong"
let (~%) s = [Text(s.ToString())]
let (%=) name value = Attr(name,value)

// [snippet:Multi-currency domain]
type Money = private { Amount:decimal; Currency:Currency } 
   with   
   static member ( * ) (lhs:Money,rhs:decimal) = 
      { lhs with Amount=lhs.Amount * rhs }
   static member ( + ) (lhs:Money,rhs:Money) =
      if lhs.Currency <> rhs.Currency then invalidOp "Currency mismatch"
      { lhs with Amount=lhs.Amount + rhs.Amount}
   override money.ToString() = sprintf "%M%s" money.Amount money.Currency
and  Currency = string

type RateTable = { To:Currency; From:Map<Currency,decimal> }

let exchangeRate (rates:RateTable) cy =   
   if rates.To = cy then 1.0M else rates.From.[cy]

let convertCurrency (rates:RateTable) money =
   let rate = exchangeRate rates money.Currency
   { Amount=money.Amount / rate; Currency=rates.To }
// [/snippet]

// [snippet:Multi-currency report model]
type Report = { Rows:Row list; Total:Money }
and  Row = { Position:Position; Total:Money }
and  Position = { Instrument:string; Shares:int; Price:Money }

let generateReport rates positions =
   let rows =
      [for position in positions ->        
         let total = position.Price * decimal position.Shares
         { Position=position; Total=total } ]
   let total =
      rows
      |> Seq.map (fun row -> convertCurrency rates row.Total)   
      |> Seq.reduce (+)
   { Rows=rows; Total=total }
// [/snippet]

// [snippet:Multi-currency report view]
let toHtml (report:Report) =
   html [
      head [ title %"Multi-currency report" ]      
      body [
         table <|
            ("border"%="1") ::
            ("style"%="border-collapse:collapse;") ::
            ("cellpadding"%="8") ::
            thead [
               tr [th %"Instrument"; th %"Shares"; th %"Price"; th %"Total"] 
            ] ::
            tbody [
               for row in report.Rows ->
                  let p = row.Position
                  tr [td %p.Instrument; td %p.Shares; td %p.Price; td %row.Total]
            ] :: 
            [ tfoot [
               tr [td ("colspan"%="3"::"align"%="right"::[strong %"Total"])
                   td %report.Total]
            ]]
         ]
      ]
// [/snippet]

// [snippet:Example]
let USD amount = { Amount=amount; Currency="USD" }
let CHF amount = { Amount=amount; Currency="CHF" }

let positions =
   [{Instrument="IBM";      Shares=1000; Price=USD( 25M)}
    {Instrument="Novartis"; Shares= 400; Price=CHF(150M)}]

let inUSD = { To="USD"; From=Map.ofList ["CHF",1.5M] }

let positionsInUSD = generateReport inUSD positions

let report = positionsInUSD |> toHtml |> Html.toString
// [/snippet]

// [snippet:Show report embedded]
#r "System.Windows.Forms.dll"
open System.Windows.Forms
let form = new Form(Text="Multi-currency report")
let web = new WebBrowser(Dock=DockStyle.Fill)
form.Controls.Add(web)
web.Navigate("about:blank")
web.Document.Write(report)
form.Show()
// [/snippet]

// [snippet:Write report & launch in browser]
open System.IO
let name = System.Guid.NewGuid().ToString()
let path = Path.GetTempPath() + name + ".html"
let writer = File.CreateText(path)
writer.Write(report)
writer.Close()
System.Diagnostics.Process.Start(path)
// [/snippet]