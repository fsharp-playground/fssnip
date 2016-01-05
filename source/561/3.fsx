namespace Pit

[<AutoOpen>]
module Snippet =
    type JsAttribute () = inherit System.Attribute()
    type DomEntryPoint () = inherit System.Attribute()
    type DomElement () =
        member el.GetAttribute(name:string) = ""
        member el.SetAttribute(name:string,value:string) = ()
        member el.AppendChild(value:DomElement) = ()
    type DomDocument () =
        member doc.CreateElement(name) = DomElement()
        member doc.GetElementById(name) = DomElement()
    type DomTable private () =
        inherit DomElement()
        static member Of (table:DomElement) = DomTable()
    let document = DomDocument()

// [snippet:Calculator sample]
open Pit
open Pit.Dom

module Calculator =

    let [<Js>] (?) (el:DomElement) name =
        el.GetAttribute(name)
    let [<Js>] (?<-) (el:DomElement) name value =
        el.SetAttribute(name,value)
    type DomAttribute = { Name:string; Value:obj }
    let [<Js>] (@=) name (value:'a) =
        { Name=name; Value=box value }
    let [<Js>] tag name (attributes:DomAttribute list) =
        let el = document.CreateElement(name)
        for a in attributes do el.SetAttribute(a.Name,a.Value.ToString())
        el

    let [<Js>] display =
        tag "input" ["type"@="text";"value"@=0;"style"@="text-align:right"]
    let [<Js>] mutable operation : (int -> int) option = None
    let [<Js>] mutable append = false

    let [<Js>] enter s =
        s, fun () ->
            let value = display?value
            if append then display?value <- value + s
            else display?value <- s; append <- true

    let [<Js>] calculate () =
        let value = int display?value
        operation |> Option.iter (fun op ->
            let newValue = op value
            display?value <- newValue.ToString() 
        )
        operation <- None

    let [<Js>] operator op () =
        calculate ()
        append <- false
        let value = display?value |> int
        operation <- op value |> Some
   
    let [<Js>] add = (+) 
    let [<Js>] sub = (-)
    let [<Js>] mul = (*)
    let [<Js>] div = (/)

    let [<Js>] buttons =
        [[enter "7"; enter "8"; enter "9"; "/", operator div]
         [enter "4"; enter "5"; enter "6"; "*", operator mul]
         [enter "1"; enter "2"; enter "3"; "-", operator sub]
         [enter "00"; enter "0"; "=", calculate; "+", operator add]]

    [<DomEntryPoint>]
    let [<Js>] main() =
        let table = (tag "table" [] |> DomTable.Of)  
        let tr = tag "tr" []
        let td = tag "td" ["colspan"@="4"]
        td.AppendChild display
        tr.AppendChild td 
        table.AppendChild tr 
        buttons |> List.iter (fun row ->
            let tr = tag "tr" [] 
            row |> List.iter (fun (text,action) ->
                let td = tag "td" []
                let input = 
                    tag "input" ["type"@="button";"value"@=text;"style"@="width:32px"]
                input |> Event.click |> Event.add (fun _ -> action ())
                td.AppendChild input
                tr.AppendChild td
            )
            table.AppendChild tr
        )
        let div = document.GetElementById "calculator"
        div.AppendChild table

// Note: add this div to body of project's HTML page: <div id="calculator"></div>
// [/snippet]F# Web Snippets