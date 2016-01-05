type Earth = 
    | Land
    | Water

let board = array2D [[Land;  Land;  Land;  Water;];
                     [Water; Land;  Land;  Water;];
                     [Land;  Water; Water; Water;];
                     [Water; Land;  Land;  Water;]]
                                          
let moveRight position = 
    let (x,y) = position
    (x + 1, y)

let moveLeft position = 
    let (x,y) = position
    (x - 1, y)

let moveUp position = 
    let (x,y) = position
    (x, y + 1)

let moveDown position = 
    let (x,y) = position
    (x, y - 1)

let xSize board = Array2D.length1 board

let ySize board = Array2D.length2 board

let offBoard position board = 
    let (x,y) = position
    x < 0 || y < 0 || x >= (xSize board) || y >= (ySize board)

let markPosition position previousSpots = position::previousSpots

let positionOnTarget position board target = 
    if offBoard position board then 
        false
    else
        let (x, y) = position
        (Array2D.get board x y) = target

let positionExists position list = 
    List.exists(fun pos -> pos = position) list

(* 
   Iterate over each element in a 2d array, passing the x and y
   coordinate and the board, to the supplied function
   which can return an item. The items are all cons together
   and the function returns a new list
*)

let forEachElement (applier:(int -> int -> 'a[,] -> 'b)) (twoDimArray:'a[,]) =
    let mutable items = [] 
    for x in 0..(xSize board) do
        for y in 0..(ySize board) do            
            items <- (applier x y twoDimArray)::items
    items

let getLandMasses board target = 
    // keep track of all processed positions across the recursions
    let processed = ref []

    // recursively go through from the starting position
    // keeping track of any land masses you find
    let rec getLandMasses' position board currentMass =        

        // if you move off the board return
        if offBoard position board then
            currentMass

        // if you already processed this position then don't do anything
        else if positionExists position !processed then
            currentMass
        else  
            
            // branch out left, up, right, and down and see what you can find
            let up = moveUp position
            let down = moveDown position
            let left = moveLeft position
            let right = moveRight position
            
            let found = positionOnTarget position board target

            processed := position::!processed;
            match found with 
                | true ->
                    position::currentMass  
                        |> getLandMasses' up board
                        |> getLandMasses' down board
                        |> getLandMasses' left board
                        |> getLandMasses' right board

                | false -> 
                    // if you didn't find anything return the masses that you 
                    // found prevoiusly
                    currentMass


    // go through each board element and find masses starting at the
    // the current position
    // filter out any positions that found no masses
    let massFinder x y board = getLandMasses' (x, y) board []

    forEachElement massFinder board
        |> List.filter (fun i -> not (List.isEmpty i))

let masses = getLandMasses board Land

let largestList = List.maxBy(List.length) masses

System.Console.WriteLine("Largest mass is " + (List.length largestList).ToString());