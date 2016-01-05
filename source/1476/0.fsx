open System
let Game1 count=
  let rnd=new Random()
  List.init count (fun _ ->rnd.Next(3))
    |>List.fold (fun a x->x|>function
                       |x when x=rnd.Next(3)->a+1
                       |_->a)
                0|>printf "%d wins\n"
let Game2 count=
  let rnd=new Random()
  List.init count (fun _ ->rnd.Next(3))
    |>List.fold (fun a x->x|>function
                       |x when x=rnd.Next(3)->a
                       |_->a+1)
                0|>printf " %d wins\n"
//> Game1 1000;​;
//> 317 wins
//val it : unit = ()
//> Game2 1000;​;
//> 668 wins
//val it : unit = ()