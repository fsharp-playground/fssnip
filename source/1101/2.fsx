open System.Threading
open System.Windows

let launch_window_on_new_thread(title) =
    let w = ref null
    let c = ref null
    let h = new ManualResetEventSlim()
    let isAlive = ref true
    let launcher() =
        w := new Window()
        (!w).Loaded.Add(fun _ ->
            c := SynchronizationContext.Current
            h.Set())
        (!w).Title <- title
        (!w).ShowDialog() |> ignore
        isAlive := false
    let thread = new Thread(launcher)
    thread.SetApartmentState(ApartmentState.STA)
    thread.IsBackground <- true
    thread.Name <- sprintf "UI thread for '%s'" title
    thread.Start()
    h.Wait()
    h.Dispose()
    !w,!c,isAlive