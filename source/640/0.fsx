#r "System.Windows.Forms"

module Trie =
    type Impl<'Char> when 'Char : comparison =
    | Multi of Map<'Char, Node<'Char>>

    and Node<'Char> when 'Char : comparison =
        { impl : Impl<'Char>
          someWordEndsHere : bool }
    
    let leaf = { impl = Multi Map.empty; someWordEndsHere = false }

    let rec insert node (word : 'Char seq) =
        if Seq.isEmpty word then
            { node with someWordEndsHere = true }
        else
            let first, rest = Seq.head word, Seq.skip 1 word
            match node.impl with
            | Multi m ->
                let c, next =
                    match Map.tryFind first m with
                    | Some next ->
                        (first, insert next rest)
                    | None ->
                        (first, insert leaf rest)
                let m =
                    Map.add c next m
                { node with impl = Multi m }

    let rec tryFindNode node (prefix : 'Char seq) =
        if not <| Seq.isEmpty prefix then
            let first, rest = Seq.head prefix, Seq.skip 1 prefix
            match node.impl with
            | Multi m ->
                match Map.tryFind first m with
                | Some next ->
                    tryFindNode next rest
                | None ->
                    None
        else
            Some node

    let rec getSuffixes node =
        seq {
            if node.someWordEndsHere then
                yield []
            match node.impl with
            | Multi m ->
                for (c, next) in Map.toSeq m do
                    yield!
                        getSuffixes next
                        |> Seq.map (fun s -> c :: s)
        }

    let findSuffixes node prefix =
        let results =
            tryFindNode node prefix
            |> Option.bind(fun node ->
                getSuffixes node
                |> Some)
        match results with
        | None -> Seq.empty
        | Some s -> s


module Extensions =
    open System.Windows.Forms

    type System.String with
        member this.ReverseAt(idx) =
            seq {
                for i in (idx - 1) .. -1 .. 0 -> this.[i]
            }

        static member Reverse(s : char seq) =
            let s = Seq.toArray s
            let sb = System.Text.StringBuilder()
            for i in s.Length - 1 .. -1 .. 0 do
                sb.Append(s.[i])
                |> ignore
            sb.ToString()

        static member ofSeq(s : char seq) =
            let sb = System.Text.StringBuilder()
            for c in s do
                sb.Append(c)
                |> ignore
            sb.ToString()
            
    type System.Windows.Forms.TextBoxBase with
        member this.GetWordBeforeCarret(isValidInWord) =
            this.Text.ReverseAt(this.SelectionStart)
            |> Seq.takeWhile isValidInWord
            |> System.String.Reverse

        member this.AddAutoCompletion(words) =
            let isValidInWord = System.Char.IsLetterOrDigit
            let trie =
                words
                |> Seq.fold Trie.insert Trie.leaf

            let handleKey (kp : KeyEventArgs) =
                if kp.Control && kp.KeyCode = Keys.Space then
                    kp.SuppressKeyPress <- true
                    let prefix = this.GetWordBeforeCarret(isValidInWord)
                    let pos = this.SelectionStart
                    let before = this.Text.[0 .. pos - 1]
                    let after = this.Text.[pos .. ]
                    let insertSuffix s =
                        this.Text <- before + s + after
                        this.SelectionStart <- pos
                        this.SelectionLength <- s.Length
                    let suffixes =
                        Trie.findSuffixes trie prefix
                        |> Seq.map (System.String.ofSeq)
                        |> Seq.sort
                        |> Seq.truncate 10
                        |> Seq.toArray

                    match suffixes with                    
                    | [||] -> ()
                    | [|s|] ->
                        insertSuffix s
                    | _ ->
                        let cms = new ContextMenuStrip()
                        for s in suffixes do
                            cms.Items.Add(prefix + s, null, fun _ _ -> insertSuffix s)
                            |> ignore
                        let pos = this.GetPositionFromCharIndex(pos)
                        let pos = System.Drawing.Point(pos.X, pos.Y + cms.Font.Height)
                        cms.Show(this, pos)

            this.KeyDown.Subscribe handleKey


open System.Windows.Forms
open Extensions

let form = new Form()
let tb = new RichTextBox(Dock = DockStyle.Fill)
tb.AddAutoCompletion(["C#" ; "F#" ; "C++"; "C"; "Ocaml"; "Haskell"])
form.Controls.Add tb
form.Show()
