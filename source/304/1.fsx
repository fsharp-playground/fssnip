let FinalAnswer = ref 0.0
let get_final_answer = !FinalAnswer

let rec eval_expr_fail =
        FinalAnswer := 7.0
        get_final_answer        // fails, returns 0.0

let rec eval_expr_works =
        FinalAnswer := 7.0
        !FinalAnswer           // works, return 7.0
