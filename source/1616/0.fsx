/// Validation can succeed or report a list
/// of validation error messages 
type ValidationResult = 
  | OK
  | Invalid of string list

// Validation #1 - check that name is not empty
let validateName (name:string, _, _) =
  if name = "" then Invalid ["Name cannot be empty"]
  else OK

// Validation #2 - check that passwords match
let validatePass (_, pass1, pass2) = 
  if pass1 <> pass2 then Invalid ["Passwords do not match"]
  else OK

// Validation #3 - check that password is long enough
let validatePassLen (_, pass:string, _) = 
  if pass.Length < 5 then Invalid ["Passwords too short"]
  else OK

// When combining two validation functions, we pass the input 'x' to
// both of them. Then we either return OK, if both returned OK, or
// we collect error messages from both funtions
let (<&>) f1 f2 x = 
  match f1 x, f2 x with
  | OK, OK -> OK
  | Invalid e1, Invalid e2 -> Invalid (e1 @ e2)
  | Invalid e, OK | OK, Invalid e -> Invalid e

// Now we can compose validation functions
// (to get function that reports all errors)
let validate = 
  validateName <&>
  validatePass <&>
  validatePassLen

validate ("Tomas", "pa", "password")