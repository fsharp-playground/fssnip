open System

/// Each survey question in LivePerson has it's own unique id/name.
/// E.g. NPS survey question is something like: 
///         "How likely are you to suggest" might be "survey889802129"
/// Even though the question stays the same, each LOB in chat has it's
/// own ID/name for the NPS question, which makes it a pain to pull them all
/// together to run numbers.
/// For example, the following are all the same question, in different lines of business:
        //NPS
        //	Appliance		survey889802123
        //	Bath			survey889802129
        //	Carpet			survey095702179
        //	Cart			survey889802135
        //	Cust Serv		survey889802153
        //	Flooring		survey889802159
        //	Husky			survey889802164
        //	Outdoors		survey889802169
        //	Water Heat		survey889802174

// First I define a type that stands for any list of survey names/id's, with a method to test
// a survey name to see if it exists in my list.
type SurveyList(repList: seq<string>) =
    member this.contains surveyName = Seq.exists ((=) surveyName) repList

// Create a SurveyList of all the NPS survey ID names.
let npsSurveyList = SurveyList(["survey889802123"; "survey889802129";"survey095702179";
                                "survey889802135";"survey889802153";"survey889802159";
                                "survey889802164";"survey889802169";"survey889802174"])

// I could have built everything above into the Active Pattern below,
// but I liked having "if npsSurveyList.contains surveyName" because
// it's clear to me.
// Create an Active Pattern that matches an NPS survey:
let (|IsNpsSurveyScore|None|) surveyName =
    if npsSurveyList.contains surveyName then IsNpsSurveyScore else None

// Pretending I got the name of a varValue from a transcript
let varValueName = "survey889802123"

// Match the name with a list of possible names, then return a string about what the thing is.
let whatIsIt =
    match varValueName with
    | "skill" -> "This is the agent's skill."
    | "OrderTotal" -> "This is the dollar value of the order"
    | IsNpsSurveyScore -> "This is an NPS survey score"
    | _ -> "None of the above"

Console.WriteLine(whatIsIt) // Prints "This is an NPS survey score"

