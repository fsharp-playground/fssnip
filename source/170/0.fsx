type Adder = 
    {
        add : unit -> int;
        getX : unit -> int;
        getY : unit -> int;
        setX : int -> unit;
        setY : int -> unit;
    }

let goodAdder x y = 
    let _x = ref x
    let _y = ref y
    let add () = !_x + !_y
    let getX () = !_x
    let getY () = !_y
    let setX x = _x := x
    let setY y = _y := y
    { add = add; getX = getX; getY = getY; setX = setX; setY = setY}

let weirdAdder x y = 
    let baseAdder = goodAdder x y
    let weirdAdd () = baseAdder.add() + 10
    { add = weirdAdd; getX = baseAdder.getX; getY = baseAdder.getY; 
    setX = baseAdder.setX; setY = baseAdder.setY}