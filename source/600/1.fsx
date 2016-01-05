open System
open System.Web
open System.Web.Mvc


let enumToSelectList<'a, 'b when 'a : enum<'b> and 'a : equality>(enum:'a) = 
  let toSelectListItem (valv, text, selected) =
    let sli = new SelectListItem()
    sli.Selected <- selected
    sli.Value <- valv
    sli.Text <- text
    sli 

  System.Enum.GetValues(typeof<'a>)  
    |> Seq.cast<'a> 
    |> Seq.map (fun en -> toSelectListItem ((LanguagePrimitives.EnumToValue en).ToString(), en.ToString(), (enum = en)))