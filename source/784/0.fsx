[<AutoOpen>]
module FsiProp
open System
open System.Windows.Forms
let prop = 
    let screen = Screen.PrimaryScreen.Bounds
    let workingArea = Screen.PrimaryScreen.WorkingArea
    let prop = new PropertyGrid(Dock = DockStyle.Fill)
    let form = new Form(Text = "Here you can see the properties of 'it'.")
    form.Controls.Add(prop)
    let gotIt = ref DateTime.Now (* HACK: save time of getting 'it' here *)
    fsi.AddPrintTransformer(fun it -> (* HACK: no calls for properties of it *)
        if DateTime.Now - !gotIt > TimeSpan.FromSeconds(1.0) then
            prop.SelectedObject <- it
            if not form.Visible then form.Visible <- true
            form.Text <- if it = null then "null" else it.GetType().Name
            gotIt := DateTime.Now
        null)
    form.Closing.Add(fun args -> args.Cancel <- true; form.Hide())
    form.Show() (* position and size must be set after showing *)
    form.WindowState <- FormWindowState.Normal
    form.Top <- 0
    let offset = 25 (* maximize Fsi to see what this means *)
    form.Left <- screen.Width / 2 - offset
    form.Width <- screen.Width / 2 + offset (* below: don't cover task bar *)
    form.Height <- screen.Height - (screen.Height - workingArea.Height)
    prop (* You can manipulate prop! Just type prop;; *)