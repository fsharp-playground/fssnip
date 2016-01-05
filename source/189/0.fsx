//Discriminated unions to represents the elements of a tense.
type Name = | John | Galt
type Verb = | Is | Was
type Mark = QuestionMark | ExclamationMark

//We must use symbolic identifiers to represent Marks since union cases must
//be uppercase identifiers. They are not function values!
let (?) = QuestionMark
let (!) = ExclamationMark

//A function to represent a tense.
//Please note that we must use QuestionMark instead of (?) in the pattern matching
//because identifiers in pattern matching are mean to capture values, then when using (?)
//it will behave like _
let Who a b c d = 
    match a,b,c,d with
    | Is, John, Galt, QuestionMark | Was, John, Galt, QuestionMark 
        -> "John Galt is a fictional character in Ayn Rand's novel Atlas Shrugged."
    | Is,_,_,_ | Was,_,_,_ 
        -> "I will suggest you to Google or Bing that name."

//The famous query, now a valid F# expression
Who Is John Galt (?)