/// composite a function f with passing parameter a n times.
/// f: function
/// a: a is the parameter into f
/// n: composite the f n times

let composite f a n =
    [1..n] 
    |> Seq.map (fun _ -> f) 
    |> Seq.fold (fun acc n -> acc |> n) a