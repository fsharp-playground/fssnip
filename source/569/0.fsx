open Pit
open Pit.Dom
open Pit.Javascript

module App =
    (*[omit:(helper functions omitted)]*)
    type DomAttribute = { Name:string; Value:obj }
    let [<Js>] (@=) name (value:'a) =
        { Name=name; Value=box value }
    let [<Js>] tag name (attributes:DomAttribute list) =
        let el = document.CreateElement(name)
        for a in attributes do el.SetAttribute(a.Name,a.Value.ToString())
        el

    type Delay = Delay of int
    type DelayBuilder() =
        [<Js>] member x.Bind(Delay t, f:unit->unit) = window.SetTimeout(f, t) |> ignore
        [<Js>] member x.Return(t)  = fun () -> t
        [<Js>] member x.Delay(f:unit->'a)   = f()
        [<Js>] member x.Zero () = ()
    let [<Js>] delay = DelayBuilder() (*[/omit]*)

    let rec [<Js>] start (root:DomElement) =
        let addButton (total:int) = 
            let button = 
                tag "input" 
                    ["type"@="submit";
                     "value"@="Play " + total.ToString();
                     "style"@="font-size:large;margin:4px"]
            button |> Event.click |> Event.add (fun _ -> countdown total root)
            root.AppendChild button
        addButton 5
        addButton 10
        addButton 20

    and [<Js>] countdown total (root:DomElement) =
        delay {
            root.InnerHTML <- "3"
            do! Delay 1000
            root.InnerHTML <- "2"
            do! Delay 1000
            root.InnerHTML <- "1"
            do! Delay 1000
            root.InnerHTML <- "Go"
            do! Delay 1000
            play total root
        }

    and [<Js>] play total (root:DomElement) =
        let started = Date().GetTime()
        let question = 
            tag "div" ["style"@="font-size:x-large;text-align:center"]
        let answer = 
            tag "input" 
                ["type"@="text";"size"@="3";
                 "style"@="width:100px;font-size:x-large;text-align:center"]
            |> DomInput.Of
        let showQuestion () =
            root.InnerHTML <- ""
            root.AppendChild question
            root.AppendChild answer
        showQuestion ()
        let expected, asked = ref 42, ref "7 x 6"
        let ask () = 
            let next () = Math.random() * 13.0 |> Math.floor |> int
            let a, b = next(), next()
            expected := a * b
            asked := a.ToString() + " x "  + b.ToString()
            question.InnerHTML <- !asked
            answer.Value <- ""
            answer.Focus()
        ask ()
        let count, score = ref 0, ref 0
        let toint (f:float) = int f
        answer
        |> Event.keydown 
        |> Event.filter (fun e -> e.KeyCode = 13 && JsString(answer.Value).Length > 0)
        |> Event.add (fun _ ->
            let value = answer.Value |> int
            let cont() =
                count := !count + 1
                if !count = total then
                    let finished = Date().GetTime()
                    let ms = float (finished - started)
                    let seconds = ms / 1000.0 |> toint
                    finish seconds !score !count root
                else ask()
            if value = !expected then
                score := !score + 1
                cont()
            else
                delay {
                    root.InnerHTML <- !asked + " = " + (!expected).ToString()
                    do! Delay 3000
                    showQuestion()
                    cont()
                }
        )

    and [<Js>] finish seconds score count (root:DomElement) =
        root.InnerHTML <- ""
        let div = tag "div" ["style"@="font-size:x-large;text-align:center"] 
        root.AppendChild div
        div.InnerHTML <- 
            score.ToString() + " / " + count.ToString() + 
            " in " + seconds.ToString() + "s"
        delay {
            do! Delay 10000
            root.InnerHTML <- ""
            start root
        }

    [<Js;DomEntryPoint>]
    let main () = document.GetElementById("maindiv") |> start