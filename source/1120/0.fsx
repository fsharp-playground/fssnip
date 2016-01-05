let fibonacci =
    Seq.unfold
        (fun (current, next) -> Some(current, (next, current + next)))
        (0, 1)