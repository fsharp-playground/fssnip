module SQL_Highlighing

open System.Runtime.InteropServices

module Lock =
    [<DllImport(@"User32", CharSet = CharSet.Ansi, SetLastError = false, ExactSpelling = true)>]
    extern void LockWindowUpdate(int hWnd)

open System.Text.RegularExpressions
open System.Drawing

type SyntaxRTB() = 
    inherit System.Windows.Forms.RichTextBox()

    override X.OnTextChanged(e : System.EventArgs) =
        base.OnTextChanged(e); X.ColorTheKeyWords()

    member X.ColorTheKeyWords() =
        let HL s c =
            let color(m : Match, color : Color) =
                X.SelectionStart    <- m.Index
                X.SelectionLength   <- m.Length
                X.SelectionColor    <- color
            Regex.Matches(X.Text, "\\b" + s + "\\b", RegexOptions.IgnoreCase) |> fun mx ->
                for m in mx do if (m.Success) then color(m,c)

        let SelectionAt = X.SelectionStart
        Lock.LockWindowUpdate(X.Handle.ToInt32())

        HL "(select)|(where)|(from)|(top)|(order)|(group)|(by)|(as)|(null)|(insert)|(exec)|(into)" Color.Blue
        HL "(join)|(left)|(inner)|(outer)|(right)|(on)" Color.Red
        HL "(and)|(or)|(not)" Color.DarkGreen
        HL "(case)|(when)|(then)|(else)|(end)|(if)|(begin)" Color.Teal
        HL "(cast)|(nvarchar)|(bit)|(datetime)|(int)|(table)" Color.BlueViolet
        HL "(datepart)" Color.DarkOrange
        HL "(avg)|(abs)|(max)|(min)" Color.DarkRed

        X.SelectionStart    <- SelectionAt
        X.SelectionLength   <- 0
        X.SelectionColor    <- Color.Black

        Lock.LockWindowUpdate(0)