type Message = {Text: string; Number:int}
type Obj = {Message:Message}

let text = "text"
let number = 0
let obj = {Obj.Message = {Text=text; Number=number}}

match obj.Message with
| c when c.Text = text && c.Number = number -> true
| _ -> false