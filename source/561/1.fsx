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
    let [<Js>] mutable operation : (int -> int) option = None
    let [<Js>] mutable append = false
    
    let [<Js>] enter s  =
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

    let [<Js>] total () = calculate ()          

    let [<Js>] add = (+) 
    let [<Js>] sub = (-)
    let [<Js>] mul = (*)
    let [<Js>] div = (/)

    let [<Js>] buttons =
        [[enter "7"; enter "8"; enter "9"; "/", operator div]
         [enter "4"; enter "5"; enter "6"; "*", operator mul]
         [enter "1"; enter "2"; enter "3"; "-", operator sub]       
         [enter "00"; enter "0"; "=", total; "+", operator add]]               

    [<DomEntryPoint>]    
    let [<Js>] main() =                                                                                              
        let table = (tag "table" [] |> DomTable.Of)  
        let td = tag "td" ["colspan","4"]
        td.AppendChild display
        let tr = tag "tr" []
        tr.AppendChild td 
        table.AppendChild tr 
        buttons |> List.iter (fun row ->
            let tr = tag "tr" [] 
            row |> List.iter (fun (text,action) ->
                let td = tag "td" []
                let input = tag "input" ["type","button";"value",text;"style","width:32px"]         
                input |> Event.click |> Event.add (fun _ -> action ())      
                td.AppendChild input
                tr.AppendChild td
            )
            table.AppendChild tr
        )
        let div = document.GetElementById "calculator"
        div.AppendChild table

// Note: add this div to body of project's HTML page <div id="calculator"></div>