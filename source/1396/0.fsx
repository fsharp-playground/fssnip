/// This is a simple implementation of the 2048 game
/// It's playable in fsi, I might hook it up to an app later using Xamarin's tooling. 
/// But so far it was just funny to implement the rules.
namespace T2048

module Game =

    type Row = List<int>
    type Board = List<Row>
    type Direction = LEFT | RIGHT | UP | DOWN
    let NEW_TILES = [2; 4]

    let mutable theScore = 0
    let addScore s = theScore <- theScore + s

    // Here is the main logic of the game, moving the tiles and combining identical neighbours
    // We always move from right to left, other directions are performed by turning the board first
    let rec moveRow r = 
        match r with
        | [] -> []
        | 0::bs -> moveRow bs
        | [a] -> [a]
        | a::b::bs when b=0 -> moveRow(a::bs)
        | a::b::bs when a=b -> addScore(a+b) ; (a+b)::moveRow bs
        | a::b::bs -> a::moveRow(b::bs)

    let pad elem toLen bs =
        let padLen = toLen - List.length bs
        if padLen<=0 then bs else
            let newTail = [for i in 1 .. padLen -> elem]
            bs @ newTail

    let moveBoard b =
        let size = List.length b
        let moveAndPad = moveRow >> (pad 0 size)
        List.map moveAndPad b

    let reverseBoard = List.map List.rev

    let rec transposeBoard b = 
        match b with
        | [] -> []
        | []::_ -> []
        | _ ->
            let heads = List.map List.head b
            let tails = List.map List.tail b
            heads :: transposeBoard tails

    // Turn the board, move left, and turn back
    let moveDirection d =
        match d with
        | LEFT -> moveBoard
        | RIGHT -> reverseBoard >> moveBoard >> reverseBoard
        | UP -> transposeBoard >> moveBoard >> transposeBoard
        | DOWN -> transposeBoard >> reverseBoard >> moveBoard >> transposeBoard >> List.rev

    let emptySlots b =
        let slot i j v = ((i,j),v)
        let numberRow f row = List.mapi f row
        let numberBoard = List.mapi (fun i -> numberRow (slot i))
        let empties = List.choose (fun (a,b) -> if b=0 then Some(a) else None)
        b |> numberBoard |> List.concat |> empties
       
    let rnd = new System.Random()
    let oneOf vs =
        let i = rnd.Next(List.length vs)
        List.nth vs i
    
    let updateBoard coords v board =
        let (posRow,posCol) = coords
        let updateRow pos v row = List.mapi (fun i b -> if i=pos then v else b) row
        List.mapi (fun i row -> if i=posRow then updateRow posCol v row else row) board

    let addNewTile b =
        let empties = emptySlots b
        let slotCoords = oneOf empties
        let v = oneOf NEW_TILES
        updateBoard slotCoords v b

    // To detect "game over", try a direction which is 90 degress from the current one and see if any tiles can move
    let otherDirection d = 
        match d with
        | LEFT -> UP
        | RIGHT -> UP
        | UP -> LEFT
        | DOWN -> LEFT

    // Perform a move
    let nextMove b d =
        let newBoard = b |> (moveDirection d) |> addNewTile
        let gameOver = newBoard=b && (moveDirection (otherDirection d) b)=b
        (newBoard, gameOver)

    // Install a pretty printer if we are running in fsi 
    let buildString sep l = List.foldBack (fun a acc -> sprintf "%4i %s %s" a sep acc) l ""

    let printBoard (b:Board) =
        b |> List.map (buildString "|") |> List.reduce (fun a b -> a+"\n"+b)
#if INTERACTIVE
    fsi.AddPrinter printBoard
#endif

    // Here is the current state
    let mutable theBoard:Board = []

    // Start a new game
    let reset () = 
        let _ = theScore <- 0
        let _ = theBoard <- [[0;0;0;0];
                             [0;0;0;0];
                             [0;0;0;0];
                             [0;0;0;0]] |> addNewTile
        theBoard

    // Make a move and update the current state
    let move d = 
        let (newBoard, gameOver) = nextMove theBoard d
        let _ = if gameOver then printf "Game over, score=%i" theScore
        let _ = theBoard <- newBoard
        theBoard

