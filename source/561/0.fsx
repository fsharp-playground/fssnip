namespace Pit

open Pit
open Pit.Dom

module Calculator =

    let [<Js>] (?) (el:DomElement) name =
        el.GetAttribute(name) 
    let [<Js>] (?<-) (el:DomElement) name value =
        el.SetAttribute(name,value)
    let [<Js>] tag name attributes =
        let el = document.CreateElement(name)
        for (name,value) in attributes do
            el.SetAttribute(name,value)
        el

    let [<Js>] display = tag "input" ["type","text";"value","0"]
    let [<Js>] mutable operation = None
    let [<Js>] mutable append = false
    
    let [<Js>] digit d =
        let s = d.ToString()       
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

    let [<Js>] add () = 
        calculate ()
        append <- false
        let value = display?value |> int
        operation <- (+) value |> Some
        
    let [<Js>] total () = calculate ()          

    let [<Js>] buttons =
        [[digit 7; digit 8; digit 9]
         [digit 4; digit 5; digit 6]
         [digit 1; digit 2; digit 3]
         [digit 0; "+", add; "=", total]]               

    [<DomEntryPoint>]    
    let [<Js>] main() =                                                                                          
        let createRow row =
            let createCell (text,action) =
                let td = tag "td" []
                let input = tag "input" ["type","button";"value",text]         
                input |> Event.click |> Event.add (fun _ -> action ())      
                td.AppendChild input
                td
            let tr = tag "tr" [] 
            for cell in row do cell |> createCell |> tr.AppendChild 
            tr      
        let table = (tag "table" [] |> DomTable.Of)  
        let td = tag "td" ["colspan","3"]
        td.AppendChild display
        let tr = tag "tr" []
        tr.AppendChild td 
        table.AppendChild tr 
        for row in buttons do
            row |> createRow |> table.AppendChild
        let div = document.GetElementById "calculator"
        div.AppendChild table

// Note: add this div to body of project's HTML page: <div id="calculator"></div>