let FinalAnswer = ref 0.0
let get_final_answer = !FinalAnswer
let rec evalExpr =
        FinalAnswer := 7.0
        get_final_answer            // fails
        //!FinalAnswer           // works