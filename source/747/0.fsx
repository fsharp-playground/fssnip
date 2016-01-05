open System

// TODO: legend is not showing
// TODO: dojo has a new way of specifying dependencies, see http://livedocs.dojotoolkit.org/dojox/charting
// TODO: can't combine line and point charts. This may be a Dojo limitation.


module Js = 
    open System.Reflection
    let internal ass = Assembly.Load "System.Windows, Version=5.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
    let internal ass2 = Assembly.Load "System.Windows.Browser, Version=5.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"
    let internal htmlPageTy = ass2.GetType "System.Windows.Browser.HtmlPage"
    let internal htmlElementTy = ass2.GetType "System.Windows.Browser.HtmlElement"
    let internal htmlWindowTy = ass2.GetType "System.Windows.Browser.HtmlWindow"
    let internal htmlPageWindow = htmlPageTy.GetProperty "Window"
    let internal htmlWindowEval = htmlWindowTy.GetMethod "Eval"
    let internal contInvoke = typeof<obj->unit>.GetMethod "Invoke"
    let internal deploymentTy = ass.GetType "System.Windows.Deployment"
    let internal deploymentCurrent = deploymentTy.GetProperty "Current"
    let internal dispTy = ass.GetType "System.Windows.Threading.Dispatcher"
    let internal dispCheckAccess = dispTy.GetMethod "CheckAccess"
    let internal appDispatcher = deploymentTy.GetProperty "Dispatcher"
    let internal dispBeginInvoke = dispTy.GetMethods() |> Array.find (fun m -> m.Name = "BeginInvoke" && m.GetParameters().Length = 2)

    let uiasync (f: unit -> 'T) : Async<'T> = 
        let p = 
            Async.FromContinuations (fun (cont:obj->unit,econt,ccont) -> 
                let work () = 
                    try 
                      let res = box(f())
                      contInvoke.Invoke(cont, [| res |]) |> ignore
                    with e -> 
                      econt e
        
                let app = deploymentCurrent.GetValue(null, null)
                let disp = appDispatcher.GetValue(app, null)
                let ok = dispCheckAccess.Invoke(disp,null) 
                if ok :?> bool then 
                    work()
                else         
                    let action = new System.Action(work)            
                    dispBeginInvoke.Invoke(disp, [| box action; null |] ) |> ignore
                    )
        async { let! res = p in return (unbox res) }
    

    let uisync (f : unit -> 'T) : 'T = 
        uiasync f |> Async.RunSynchronously       

    let jsasync (script:string) : Async<obj> = 
        async { try 
                   return! uiasync (fun () -> 
                              let window = htmlPageWindow.GetValue(null, null)
                              htmlWindowEval.Invoke(window, [| script |]))
                with e -> printfn "error evaluating JS script <<<\n%s\n>>>" script; return! raise e }


    let jsDoAsync s = 
        jsasync s |> Async.Ignore 

    let jssync s = 
        jsasync s |> Async.RunSynchronously       

    let jsvoid s = 
        jssync s |> ignore

    let jsstart s = 
        jsDoAsync s |> Async.Start



    // Allow callbacks from Javascript to Silverlight through a dynamically crated Scriptable type. We don't 
    // define one statically since we don't have a static reference to System.Windows or System.Windows.Browser.
    open System
    open System.Reflection
    open System.Reflection.Emit

    let asmB = AppDomain.CurrentDomain.DefineDynamicAssembly(AssemblyName("SLCallback"),AssemblyBuilderAccess.Run)
    let modB = asmB.DefineDynamicModule("MainModule")
    let tyB = modB.DefineType("Class", TypeAttributes.Public)

    let scriptableTypeTy = ass2.GetType "System.Windows.Browser.ScriptableTypeAttribute"
    let scriptableMemberTy = ass2.GetType "System.Windows.Browser.ScriptableMemberAttribute"

    tyB.SetCustomAttribute(CustomAttributeBuilder(scriptableTypeTy.GetConstructor([| |]), [| |]))

    let fB = tyB.DefineField ("f", typeof<Action<string, obj>>, FieldAttributes.Private)
    let cB = tyB.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, [| typeof<Action<string, obj>> |])
    let cIL = cB.GetILGenerator()
    cIL.Emit(OpCodes.Ldarg_0)
    cIL.Emit(OpCodes.Call,typeof<obj>.GetConstructor([| |]))
    cIL.Emit(OpCodes.Ldarg_0)
    cIL.Emit(OpCodes.Ldarg_1)
    cIL.Emit(OpCodes.Stfld,fB)
    cIL.Emit(OpCodes.Ret)


    let mB = tyB.DefineMethod("Invoke",MethodAttributes.Public,returnType=typeof<System.Void>,parameterTypes=[| typeof<string>; typeof<obj> |])
    mB.SetCustomAttribute(CustomAttributeBuilder(scriptableMemberTy.GetConstructor([| |]), [| |]))
    let mIL = mB.GetILGenerator()
    mIL.Emit(OpCodes.Ldarg_0)
    mIL.Emit(OpCodes.Ldfld,fB)
    mIL.Emit(OpCodes.Ldarg_1)
    mIL.Emit(OpCodes.Ldarg_2)
    mIL.Emit(OpCodes.Callvirt,typeof<Action<string,obj>>.GetMethod("Invoke"))
    mIL.Emit(OpCodes.Ret)

    let ty = tyB.CreateType()

    uisync (fun () -> 
        let plugin = htmlPageTy.InvokeMember("Plugin", BindingFlags.GetProperty, null, null, [| |])
        let id = htmlElementTy.InvokeMember("Id", BindingFlags.GetProperty, null, plugin, [| |])
        if id :?> string = "" then 
            htmlElementTy.InvokeMember("SetProperty", BindingFlags.InvokeMethod, null, plugin, [| "id"; "silverlight" |]) |> ignore)

    let silverlightId = 
        uisync (fun () -> 
            let plugin = htmlPageTy.InvokeMember("Plugin", BindingFlags.GetProperty, null, null, [| |])
            htmlElementTy.InvokeMember("Id", BindingFlags.GetProperty, null, plugin, [| |]) :?> string)

    let mutable stamp = 0L
    let nextStamp() = stamp <- stamp + 1L; string stamp
    let nextCallback() = "SL" + nextStamp()

    let requests = new System.Collections.Generic.Dictionary<string,(obj -> unit)>()
    let invoke (tgt:string) (obj:obj) = 
        let f = requests.[tgt] 
        requests.Remove tgt |> ignore; 
        f obj

    let a = ty.GetConstructors().[0]
    let xobj = a.Invoke([| Action<string,obj>(invoke) |])

    uisync (fun () -> htmlPageTy.InvokeMember ("RegisterScriptableObject", BindingFlags.InvokeMethod, null, null, [| "SL"; xobj |]))

    let callback (f: 'T -> unit) = 
        let f2 (x:obj) = f (x :?> 'T)
        let stamp = nextStamp()
        requests.[stamp] <- f2
        "(function (arg) { return (document.getElementById('" + silverlightId + "')).Content.SL.Invoke('" + stamp + "',arg); })" 

    let callback0 (f: unit -> unit) = 
        let f2 (x:obj) = f ()
        let stamp = nextStamp()
        requests.[stamp] <- f2
        "(function () { document.getElementById('" + silverlightId + "').Content.SL.Invoke('" + stamp + "',''); })" 


    let convertible (x: IConvertible) = 
        match x with 
        | :? string as s -> "'" + s + "'"
        | _ -> System.Convert.ToString x
    let seq f xs = "[" + String.concat ", " (Seq.map f xs) + "]"

module Async = 
    let Once p = 
        let executed = ref false
        async { if not executed.Value then 
                    executed := true; 
                    do! p } 

module Seq = 
    let index xs = xs |> Seq.mapi (fun i x -> (i,x))       

module DojoChart = 
    module ChartTypes = 
        type ChartData = 
            | XYSeqData of seq<IConvertible * IConvertible>
            | XYSeqSeqData of seq<seq<IConvertible>>

        let conv (x:IConvertible) = x
        let YSeqSeq data = XYSeqSeqData (data |> Seq.map (Seq.map (fun y -> conv y)))
        let XYSeq data = XYSeqData (data |> Seq.map (fun (x,y) -> conv x, conv y))
 
//For any non “stacked” line plot type you can specify coordinate pairs. You need to use keys that correspond to the hAxis and vAxis parameters defined in the addPlot() call. These default to x and y.

        let (||||) a b = match a with None -> b | _ -> a
        let getSeriesXY plotName seriesName d = 
            let seriesName = defaultArg seriesName ("Series " + Js.nextStamp())
            ".addSeries('" + seriesName + "', " + (d |> Js.seq (fun (x,y) -> "{x: " + Js.convertible x + ",y: " + Js.convertible y + "}")) + ", {plotName:'" + plotName + "'})"

        let getSeriesY plotName seriesName d = 
            let seriesName = defaultArg seriesName ("Series " + Js.nextStamp() )
            ".addSeries('" + seriesName + "', " + (d |> Js.seq Js.convertible) + ", {plotName:'" + plotName + "'})"

//     {plot: "other", stroke: {color:"blue"}, fill: "lightblue"});

        let getData plotName seriesName d = 
            match d with 
            | XYSeqData xs -> [ getSeriesXY plotName seriesName xs ]
            | XYSeqSeqData xss -> xss |> Seq.map (getSeriesY plotName seriesName) |> Seq.toList

        type Theme internal (name:string) = 
            member x.Name = name

        type GenericChart internal (series : (string * string option * ChartData) list, animate:bool option, title: string option,theme: Theme option) = 
            static member internal Create(seriesKind, seriesName, seriesData) = 
                GenericChart(series=[(seriesKind,seriesName,seriesData)],animate=None,title=None,theme=Some(Theme("Wetland")))
            member internal __.With(?Title,?Animate,?SeriesName,?Theme) = 
                let series = series |> List.map (fun (kind,seriesName,data) -> (kind, (SeriesName |||| seriesName), data))
                GenericChart(series=series,animate=(Animate |||| animate),title=(Title |||| title),theme=(Theme |||| theme))

            member private __.Series = series
            member private __.Animate = animate
            member private __.Title = title
            member private __.Theme = theme
            static member internal Combine(ch1:GenericChart,ch2:GenericChart) = 
                GenericChart(series=ch1.Series @ ch2.Series,
                             animate=(ch1.Animate |||| ch2.Animate),
                             title=(ch1.Title |||| ch2.Title),
                             theme=(ch1.Theme |||| ch2.Theme))

            member ch.WithStyle(?Theme) = ch.With(?Theme=Theme)
            member internal ch.RenderAsync() = 
                let script = 
                    " dojo.empty('chartingArea'); 

                      var chart = new dojox.charting.Chart2D('chartingArea', {" + 
                          String.concat "," 
                              [ yield! Option.toList (title |> Option.map (fun s -> "title: '" + s + "'"))  ] 
                          + "});
                      chart" 
                    + 
                    String.concat "\n\t" 
                        [ yield ".addAxis('x', {fixLower: 'major', minorTicks: 'false', minorLabels: 'false', fixUpper: 'major'})"
                          yield ".addAxis('y', {vertical: true, minorTicks: 'false', minorLabels: 'false', fixLower: 'major', fixUpper: 'major', includeZero: true})" 
                          match theme with 
                          | None -> () 
                          | Some s -> yield ".setTheme(dojox.charting.themes." + s.Name + ")" 
                          for (i,(kind,seriesName,data)) in Seq.index series do 
                            let plotName = (if i = 0 then "default" else "plot" + string i)
                            let plotArgs = 
                                String.concat "," 
                                    [ yield "type: '" + kind + "' " 
                                      yield! (if animate = Some true then [ "animate: { duration: 1000, easing: dojo.fx.easing.linear}" ] else []) ]
                            yield (".addPlot('" + plotName + "', { " + plotArgs + "})" ) 
                            yield! getData plotName seriesName data ] 
                    + 
                    "\n\t.render();
                     "
                printfn "script = <<<\n%s\n>>>" script
                Js.jsDoAsync script

    open ChartTypes

    let initAsync = 
        Async.Once <| Async.FromContinuations(fun (cont,econt,_) -> 
          try 

           Js.jsstart ("

            //alert('in initAsync');
            var dojoConfig = {
                parseOnLoad: true,
                dojoBlankHtmlUrl: '/blank.html'
            };

            var oHead = document.getElementsByTagName('head')[0];
            var scriptB = document.createElement('script');
            scriptB.setAttribute('src', 'http://ajax.googleapis.com/ajax/libs/dojo/1.7/dojo/dojo.js')
            scriptB.onload = afterDojoLoad;

            oHead.appendChild(scriptB); 

            var oHead = document.getElementsByTagName('head')[0];
            try { 
              var silverlightDiv = document.getElementById('" + Js.silverlightId + "');
              silverlightDiv.style.height = '60%';

              if (document.getElementById('chartingArea') == null) {
                  var chartingArea = document.createElement('div');
                  chartingArea.id = 'chartingArea';
                  chartingArea.nodeName = 'chartingArea';
                  chartingArea.style.height = '40%';
                  silverlightDiv.parentNode.insertBefore(chartingArea,silverlightDiv.parentNode.childNodes[0]); 
              }
            }  catch (e) { alert (e.message ? e.message : e); }

            function afterDojoLoad(){
                //alert('in afterDojoLoad');
                try { 
                    dojo.require('dojox.charting.Chart2D'); 
                    dojo.require('dojox.charting.Chart3D');
                    dojo.require('dojox.charting.axis2d.Default');
                    dojo.require('dojox.charting.plot2d.Default');
                    dojo.require('dojox.charting.plot2d.Pie');
                    dojo.require('dojox.charting.plot2d.ClusteredColumns');
                    dojo.require('dojox.charting.action2d.Highlight');
                    dojo.require('dojox.charting.action2d.MoveSlice');
                    dojo.require('dojox.charting.action2d.Tooltip');
                    dojo.require('dojox.charting.widget.Legend');
                    dojo.require('dojox.charting.widget.SelectableLegend');
                    dojo.require('dojo.fx.easing');

                    dojo.require('dojox.charting.themes.Tufte');
                    dojo.require('dojox.charting.themes.Wetland');
                    dojo.require('dojox.charting.themes.MiamiNice');
                    dojo.require('dojox.charting.plot3d.Bars');

                }
                catch (e) { 
                    alert ('Failure in dojo.require: ' + (e.message ? e.message : e)); 
                }
                //alert('in afterDojoLoad, calling dojo.ready');
                dojo.ready(" + Js.callback0 cont + ");
            }")
          with e -> econt e)

    let startRender (ch:GenericChart) = 
            async { try 
                        do! initAsync
                        do! ch.RenderAsync() 
                    with e -> printfn "background error: %A" e }
                |> Async.Start

    type Chart() = 

        static member StackedArea(data, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("StackedAreas", Name, YSeqSeq data).With(?Title=Title,?Animate=Animate) 

        static member StackedLines(data, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("StackedLines", Name, YSeqSeq data).With(?Title=Title,?Animate=Animate) 

        static member StackedBars(data, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("StackedBars", Name, YSeqSeq data).With(?Title=Title,?Animate=Animate) 

        static member StackedColumns(data, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("StackedColumns", Name, YSeqSeq data).With(?Title=Title,?Animate=Animate) 

        static member Line(data:seq<#IConvertible * #IConvertible>, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("Lines", Name, XYSeq data).With(?Title=Title,?Animate=Animate) 

        static member Point(data:seq<#IConvertible * #IConvertible>, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("MarkersOnly", Name, XYSeq data).With(?Title=Title,?Animate=Animate) 

        static member Pie(data:seq<#IConvertible * #IConvertible>, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("Pie", Name, XYSeq data).With(?Title=Title,?Animate=Animate) 

        static member Bar(data:seq<#IConvertible * #IConvertible>, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("Bars", Name, XYSeq data).With(?Title=Title,?Animate=Animate) 

        static member Column(data:seq<#IConvertible * #IConvertible>, ?Title, ?Name, ?Animate) = 
            GenericChart.Create("Columns", Name, XYSeq data).With(?Title=Title,?Animate=Animate) 

        static member Combine(charts:seq<GenericChart>) = 
            Seq.reduce (fun x y -> GenericChart.Combine(x,y)) charts


do Microsoft.FSharp.Compiler.Interactive.Settings.fsi.AddPrinter (fun (ch:DojoChart.ChartTypes.GenericChart) -> DojoChart.startRender ch; "(chart)")
   
open DojoChart

let dataA =  [1.0; 2.0; 0.5; 1.5; 1.0; 2.8; 0.4] |> Seq.index
let dataB =  [2.6; 1.8; 2.0; 1.0; 1.4; 0.7; 2.0] |> Seq.index
let dataC =  [6.3; 1.8; 3.0; 0.5; 4.4; 2.7; 2.0] |> Seq.index

let stackedDataA =  [1.0; 2.0; 0.5; 1.5; 1.0; 2.8; 0.4] 
let stackedDataB =  [2.6; 1.8; 2.0; 1.0; 1.4; 0.7; 2.0] 
let stackedDataC =  [6.3; 1.8; 3.0; 0.5; 4.4; 2.7; 2.0] 


Chart.Bar( dataA, Title= "Predicted Risk") 
Chart.Column( dataA, Title= "Predicted Volatility") 
Chart.Line( dataA ) 
Chart.Pie( dataA, Title= "The True Data 2") 
Chart.Point( dataA ) 

Chart.StackedArea( [ stackedDataA; stackedDataB; stackedDataC ] ) 
Chart.StackedBars( [ stackedDataA; stackedDataB; stackedDataC ] ) 
Chart.StackedColumns( [ stackedDataA; stackedDataB; stackedDataC ] ) 
Chart.StackedLines( [ stackedDataA; stackedDataB; stackedDataC ] ) 


Chart.StackedArea( [ [ for i in 0 .. 9 -> i*i] ] ) 
Chart.Line( dataA, Name="Prices" ) 
Chart.Line( dataA, Name="Prices", Animate=true ) 
Chart.Line( dataA, Animate=true ) 
Chart.Line( dataA, Title= "The True Data 0") 


Chart.Bar( dataA, Title= "The True Data 3") 
Chart.Column( dataA, Title= "The True Data 1") 
Chart.Line( dataA ) 
Chart.Pie( dataA, Title= "The True Data 2") 
Chart.Point( dataA ) 
let rnd = new System.Random()

Chart.Point( [ for i in 0 .. 1000 do 
                   let (x,y) = (rnd.NextDouble()*2.0 - 1.0, rnd.NextDouble()*2.0 - 1.0)
                   if x*x + y*y < 1.0 then 
                       yield (x,y) ] )

Chart.Line( [ for i in 0 .. 1000 do 
                   let (x,y) = (rnd.NextDouble()*2.0 - 1.0, rnd.NextDouble()*2.0 - 1.0)
                   if x*x + y*y < 1.0 then 
                       yield (x,y) ] )

Chart.Pie( [ for i in 0 .. 1000 do 
                   let (x,y) = (rnd.NextDouble()*2.0 - 1.0, rnd.NextDouble()*2.0 - 1.0)
                   if x*x + y*y < 1.0 then 
                       yield (x,y) ] )

Chart.StackedArea( [ [ for i in 0 .. 9 -> i*i] ] ) 
Chart.Line( dataA, Name="Prices" ) 
let c = Chart.Line( dataA, Name="Prices", Animate=true ) 

Chart.Line( dataA, Animate=true ) 
Chart.Line( dataA, Title= "Predicted Prices") 


Chart.StackedArea( [ stackedDataA; stackedDataB; stackedDataC ] ) 

Chart.Combine [ Chart.Point( dataA ) ; Chart.Point( dataB ) ]


Chart.StackedArea( [ stackedDataA; stackedDataB; stackedDataC ] ) 
Chart.StackedBars( [ stackedDataA; stackedDataB; stackedDataC ] ) 
Chart.StackedColumns( [ stackedDataA; stackedDataB; stackedDataC ] ) 
Chart.StackedLines( [ stackedDataA; stackedDataB; stackedDataC ] ) 

Chart.Combine [ Chart.Line( dataA ) ; Chart.Line( dataB ) ]
