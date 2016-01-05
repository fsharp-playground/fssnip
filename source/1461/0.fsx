open System

type Keys = {Header:string;Pascal:string;Camel:string;Field:string;DisplayAccessor:string;HiddenDom:string}

let toPascalCase s = 
    s
    |> Seq.mapi (fun i l -> if i=0 && Char.IsLower(l) then Char.ToUpper(l) else l)
    |> String.Concat

let humanize camel :string = 
    
    seq {
        let pascalCased = toPascalCase camel
        yield pascalCased.[0]
        for l in  pascalCased |> Seq.skip(1) do
            if System.Char.IsUpper(l) then 
                yield ' '
                yield l
            else
                yield l
    }
    |> String.Concat

let createKeys camel field header accessor =
    let hidden = sprintf """<input type="hidden" name="%s"/>"""
    {
        Keys.Header=header
        Field=field
        DisplayAccessor = accessor field
        Camel=camel 
        HiddenDom= hidden field
        Pascal= toPascalCase camel //FromCamel
    }

    
let AllKeys = 
    let formattedPositiveIntAccessor = sprintf """GetFormattedPositiveInt(Eval("%s"))"""
    let defaultAccessor = sprintf """Eval("%s")"""  
    let reqMembersAccessor = sprintf """GetRequestedMembers(Eval("%s")) + (Convert.ToBoolean(Eval("IsQuotaCap")) ? "*" : "")""" 
    seq {
        yield(createKeys "overQuotaCap" "OverQuotaCap" "OverQuota Cap" formattedPositiveIntAccessor)
        yield(createKeys "reqCompletes" "RequestedMembers" "Requested Completes" reqMembersAccessor)
    }

//                      

type InlineEditorComponent = 
    |Display
    |Editor
    |EditLink
    |ItemChild


type ClientSelector(inlineComponent:InlineEditorComponent,keys) =
    //let roles = { InlineEditorRole.Display = sprintf "%sDisplay" keys.Pascal; Editor=sprintf "%sEditor" keys.Pascal;EditLink=sprintf "%sEditLink" keys.Pascal;ItemChild=sprintf "%sItemChild" keys.Pascal}
    static member private makeRole c = match c with 
                                        |Display -> sprintf "%sDisplay"
                                        |Editor -> sprintf "%sEditor"
                                        |EditLink -> sprintf "%sEditLink"
                                        |ItemChild -> sprintf "%sItemChild"
    static member private dataRoleSelector = sprintf "%s: '[data-role=%s]'"
    
    member x.InlineComponent = inlineComponent
    member x.Name = ClientSelector.makeRole inlineComponent keys.Pascal
    member x.Role = x.Name
    member x.DataRole = ClientSelector.dataRoleSelector x.Name x.Role
    
    member x.Dom = match inlineComponent with
                    |Display -> sprintf """<span data-role="%s" class="fieldDisplay"><%%#%s%%></span>""" x.Role keys.DisplayAccessor
                    |EditLink -> sprintf """<i  data-role="%s" class="onHover fa fa-pencil-square-o" title="edit"></i>""" x.Role
                    |Editor -> sprintf """<input data-role="%s" type="text" disabled="disabled" name="%s<%%# Eval("ProjectQuotaId") %%>" style="display:none;width:90px" value="" data-original-value="" data-quotaGroupId="<%%# Eval("ProjectQuotaId") %%>"/>""" x.Role keys.Pascal 
                    |ItemChild-> failwithf "Can't generate dom for itemChild directly"

type HandlerArgs = {StorageKey:string; HiddenName:string; EditorSelector:string}
type InitializeHandlerArgs = {EditLinkSelector:string;ChildSelector:string;}
type ClientSelectorCollection(keys) =
    
    let createClient com = ClientSelector(com,keys)
    member x.Keys = keys
    member x.Display = createClient InlineEditorComponent.Display
    member x.Editor = createClient InlineEditorComponent.Editor
    member x.EditLink = createClient InlineEditorComponent.EditLink
    member x.ItemChild = createClient InlineEditorComponent.ItemChild
    member x.Asp = sprintf """ <asp:TemplateColumn HeaderText="%s">
                            <ItemTemplate>
                                 <span data-role="%s">
                                        %s
                                        %s
                                        %s
                                    </span>
                                
                            </ItemTemplate>
                        </asp:TemplateColumn>""" keys.Header x.ItemChild.Role x.EditLink.Dom x.Display.Dom x.Editor.Dom
    member x.Selectors = [
            x.Display
            x.Editor
            x.EditLink
            x.ItemChild
        ]
    member x.JsClientSelectors = 
        sprintf """     %s, 
            %s,
            %s,
            %s""" x.Display.DataRole x.Editor.DataRole x.EditLink.DataRole x.ItemChild.DataRole
    member x.HandlerArgs = {HandlerArgs.StorageKey=keys.Camel+"Edits"; HiddenName=keys.Camel+"Edits";EditorSelector = sprintf "%sEditor" keys.Camel}
    member x.JsBeforeReq = //before save or updatePanel request starts
        sprintf """beforeSavePostHandler(currentTabIsQuotaGroups, quotaGroupTabStorage, '%s', '%s', clientSelectors.%s, $qgGrid);""" x.HandlerArgs.StorageKey x.HandlerArgs.HiddenName x.HandlerArgs.EditorSelector 
    member x.JsInitialize = sprintf """quotaGridInlineEditsInitializeHandler(clientSelectors.%s, clientSelectors.%s, clientSelectors.%s, clientSelectors.quotaGroupIdAttr, clientSelectors.%s, quotaGroupTabStorage, '%s');""" x.EditLink.Name x.ItemChild.Name x.Editor.Name x.Display.Name x.HandlerArgs.StorageKey

for k in AllKeys do
    let csColl = ClientSelectorCollection(k)
    printfn "%A" (k,csColl.Asp,csColl.Selectors,csColl.JsClientSelectors,csColl.JsBeforeReq,csColl.JsInitialize)
//validation
 
    let targetJs = """c:\development\products\app\web\lib\javascript\editproject.js"""
    let targetAspx = """c:\development\products\app\web\pages\projects\editproject.aspx"""
    let targetAscx = """c:\development\products\app\web\pages\projects\usercontrols\quotagroups.ascx"""
    
    let targetJsTextValidation = 
        let text = System.IO.File.ReadAllText(targetJs)
        let missingSelectors = 
            csColl.Selectors
            |>Seq.map (fun x->x.DataRole)
            |>Seq.filter(fun x->text.Contains(x)=false)
        if Seq.isEmpty(missingSelectors)=false then failwithf "some expected .js lines were missing %A" missingSelectors
        if text.Contains(csColl.JsBeforeReq)=false then failwithf ".js: missing call(s) to beforeSavePostHandler %A" csColl.JsBeforeReq
        if text.Contains(csColl.JsInitialize)= false then failwithf ".js: missing call(s) to InitializeHandler %A" csColl.JsInitialize
        printfn "js tests passed"
    let targetAspxValidation =
        let text = System.IO.File.ReadAllText(targetAspx)
        if text.Contains(csColl.Keys.HiddenDom)=false then failwithf ".aspx: missing %A" csColl.Keys.HiddenDom
        printfn "aspx tests passed"
    let targetAscxValidation = 
        let text = System.IO.File.ReadAllText(targetAscx)
        csColl.Selectors
        |> Seq.filter (fun s-> s.InlineComponent <> InlineEditorComponent.ItemChild)
        |> Seq.iter (fun s ->
            if text.Contains(s.Dom) = false then failwithf ".ascx: missing %A" s.Dom
            )
        printfn "ascx tests passed"
    ()