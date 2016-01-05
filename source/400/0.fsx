open System
open System.Diagnostics
open System.Runtime.InteropServices
open System.Runtime.InteropServices.ComTypes

[<AutoOpen>]
module ActivatorExtension =
    [<DllImport("ole32.dll")>]  
    extern int CreateBindCtx([<In>]int reserved,  [<Out>]IBindCtx& ppbc)

    [<DllImport("ole32.dll")>]  
    extern int GetRunningObjectTable([<In>]int reserved, [<Out>]IRunningObjectTable& prot)

    type ComObject(name, instance) =
        member x.Name
            with get() = name
        member x.Instance
            with get() = instance                

    let getRunningObjects() =
        let runningObjectTable = 
            let mutable rot = null
            if GetRunningObjectTable(0, &rot) <> 0 then failwith "GetRunningObjectTable failed!"
            rot

        let getMonikers() =
            let enumerator = 
                let mutable monikerEnumerator = null
                runningObjectTable.EnumRunning(&monikerEnumerator)
                monikerEnumerator.Reset()
                monikerEnumerator

            let getNextMoniker(moniker) =
                let monikers = Array.init<ComTypes.IMoniker> 1 (fun _ -> null)
                let success = enumerator.Next(1, monikers, IntPtr.Zero) = 0
                moniker := monikers.[0]
                success
            
            seq { let moniker = ref null
                  while getNextMoniker(moniker) do yield !moniker }

        let getRunningObjectName(moniker:IMoniker) =
            let mutable runningObjectName, ctx = null, null
            if CreateBindCtx(0, &ctx) <> 0 then failwith "CreateBindCtx failed!"
            moniker.GetDisplayName(ctx, null, &runningObjectName)
            runningObjectName

        let getRunningObject(moniker:IMoniker) =
            let mutable runningObject = null
            if runningObjectTable.GetObject(moniker, &runningObject) <> 0 then failwith "IRunningObjectTable.GetObject failed!"
            runningObject

        getMonikers() |> Seq.map(fun m -> new ComObject(m |> getRunningObjectName, lazy getRunningObject(m)))

    type System.Activator with
        /// <summary>
        /// Gets running COM instances.
        /// </summary>
        /// <param name="namePredicate">A predicate function which filters the instances by name</param>
        static member GetObjects(namePredicate) =
            getRunningObjects() |> Seq.filter(fun comObj -> namePredicate(comObj.Name))

        /// <summary>
        /// Gets a running COM instance and casts it to 't.
        /// </summary>
        /// <param name="runningObjectName">The name of the com instance</param>
        static member GetObject<'t>(runningObjectName) =
            Activator.GetObjects(fun objName -> objName = runningObjectName)
            |> Seq.map(fun comObj -> comObj.Instance.Value :?> 't)
            |> Seq.nth 0

module Test =
    //This function enables the retrieval of the Visual Studio EnvDTE instance given a process id.
    let getVisualStudioInstance processId =
        let instanceName = sprintf "!VisualStudio.DTE.10.0:%i" processId
        let vsComObj = Activator.GetObjects(fun name -> name = instanceName) |> Seq.nth 0
        vsComObj.Instance.Value
