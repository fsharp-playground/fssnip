module Email =
    type EmailAddress =
        private
        | ValidEmail of string
        | InvalidEmail of string
    
    let ofString = function
        | "validEmail" -> ValidEmail "validEmail"
        | invalid -> InvalidEmail invalid 
    
    let (|ValidEmail|InvalidEmail|) = function
        | ValidEmail email -> ValidEmail email
        | InvalidEmail email -> InvalidEmail email

open Email

let invalid = Email.ofString "invalid"
let valid = Email.ofString "validEmail"

match invalid with
| InvalidEmail invalid -> printfn "invalid was InvalidEmail %s" invalid
| ValidEmail valid -> printfn "invalid was ValidEmail %s" valid

match valid with
| InvalidEmail invalid -> printfn "valid was InvalidEmail %s" invalid
| ValidEmail valid -> printfn "valid was ValidEmail %s" valid
