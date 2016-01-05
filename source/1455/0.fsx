-- a function to find the state of a certain cell. This is done by pulling out
-- the corresponding values of each list to find a single state value.

getState :: World -> Cell -> State
getState world (x,y) = (world !! y) !! x

--This will show the relative cell in a wrapped world if
-- the cell is outside the scope of the worlds dimensions

wrap :: Int -> Int -> Cell -> Cell
wrap w h (x,y) 
    | negx < 0 && negy < 0 		= (w + negx,h + negy)
	| negx < 0 && negy >=0		= (w + negx,negy)
	| negx >=0 && negy < 0 		= (negx,h + negy)
	| negx >= 0 && negy >= 0 	= (negx,negy)
	where
	negx = x `rem` w
	negy = y `rem` h


-- This is a function to find the Moore neighbourhood around a particular cell,
-- This was done by using the wrap function and simply finding the cells 
-- plus or minus around the particular cell. We use the getstate function as well.

mooreHood :: World -> Cell -> [State]
mooreHood world (x,y) = [getState world (wrap (width world) (height world) (x + 1, y )),
			getState world (wrap (width world) (height world) (x - 1, y )),
			getState world (wrap (width world) (height world) (x + 1, y + 1 )),
			getState world (wrap (width world) (height world) (x + 1, y - 1 )),
			getState world (wrap (width world) (height world) (x - 1, y + 1 )),
			getState world (wrap (width world) (height world) (x - 1, y - 1)),
			getState world (wrap (width world) (height world) (x, y + 1 )),
			getState world (wrap (width world) (height world) (x, y - 1))]


mooreCells :: Int -> Int -> Cell -> [Cell]
mooreCells = error "mooreCells not implemented"



-- This is the same general funtion as mooreHood however it is only finding 4 values and not eight.

vnHood :: World -> Cell -> [State]
vnHood world (x,y) = 	[getState world (wrap (width world) (height world) (x + 1, y )),
			getState world (wrap (width world) (height world) (x - 1, y )),
			getState world (wrap (width world) (height world) (x, y + 1 )),
			getState world (wrap (width world) (height world) (x, y - 1))]


vnCells :: Int -> Int -> Cell -> [Cell]
vnCells = error "vnCells not implemented."


-- This function finds the cells in a row firstly and will be used later in
-- the allCells function.

rowCells :: Int -> Int -> [Cell]
rowCells width y = [( x , y ) | x <-[0..(width - 1)]]


-- This Function will output all Cells in a given world (with the
-- help of the rowCells function) and it uses list functions

allCells :: World -> [[Cell]]
allCells world= [ rowCells (width world) y | y <- [0..((height world) -1)]]

worldColor :: World -> [[Color]]
worldColor []		= []
worldColor (x:xs)  	= map stateColor x : worldColor xs