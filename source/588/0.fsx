open System.Drawing.Design
open System.Windows.Forms.Design
open System.Windows.Forms


module private EditorInternals =
    let buildDefaultType (cType: Type) = 
        if cType.IsValueType then Activator.CreateInstance(cType)
        elif FSharpType.IsRecord cType then
            let pi : PropertyInfo = cType.GetProperty("Default", cType) in               
                pi.GetValue(null, null)
        else failwith (sprintf "Unsupported Type: %s" cType.Name)

type UnionEditor () =
    inherit System.Drawing.Design.UITypeEditor () 
    override t.GetEditStyle (context) = UITypeEditorEditStyle.DropDown
    override t.EditValue (context, provider, value) =
        let wfes = provider.GetService(typeof<IWindowsFormsEditorService>) :?> IWindowsFormsEditorService 
        if wfes <> null && FSharpType.IsUnion( context.PropertyDescriptor.PropertyType ) then
            let lb = new ListBox()
            lb.SelectionMode <- SelectionMode.One
            lb.Click.Add (fun _ -> wfes.CloseDropDown())
            let currentCase, args = FSharpValue.GetUnionFields(value,context.PropertyDescriptor.PropertyType)
            let cases = FSharpType.GetUnionCases context.PropertyDescriptor.PropertyType 
            for case in cases do 
                lb.Items.Add(case.Name) |> ignore
                if case = currentCase then lb.SelectedItem <- case.Name
            wfes.DropDownControl(lb)
            if lb.SelectedItem <> null && lb.SelectedIndices.Count = 1 && (lb.SelectedItem :?> string) <> currentCase.Name then 
                let newCase = cases |> Array.find (fun case -> case.Name = (lb.SelectedItem :?> string))
                let newargs = newCase.GetFields() |> Array.map (fun pi -> EditorInternals.buildDefaultType pi.PropertyType)
                FSharpValue.MakeUnion(newCase, newargs)
            else value
        else value