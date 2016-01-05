let splitString (s : string) = 
    s.Split([|' '|])

type Token = 
    |SOURCE
    |REGISTRY
    |ZIP
    |SERVICE
    |DESTINATION
    |MAXDAYS
    |SMTPSERVER
    |FROMEMAIL
    |SUCCESSEMAIL
    |FAILUREEMAIL
    |VALUE of string

let tokenize (args : string[]) = 
    [for x in args do
        let token = 
            match x with
            | "-o" -> SOURCE
            | "-r" -> REGISTRY
            | "-z" -> ZIP
            | "-v" -> SERVICE
            | "-d" -> DESTINATION
            | "-m" -> MAXDAYS
            | "-t" -> SMTPSERVER
            | "-e" -> FROMEMAIL
            | "-s" -> SUCCESSEMAIL
            | "-f" -> FAILUREEMAIL
            | _ -> VALUE x
        yield token]

type Zip = ZipFiles | DoNotZipFiles

type Options = {
    Source : string;
    Registry : string;
    Zip : Zip;
    Service : string;
    Destination : string;
    MaxDays : int;
    SmtpServer : string;
    FromEmail : string;
    SuccessEmail : string;
    FailureEmail : string;
    }

let isWholeNumber s = String.forall (fun c -> System.Char.IsDigit(c)) s

// Strips VALUE tokens from top of list, returning the rest of the list
let returnNonValueTail tokenList =
    tokenList
    |>List.toSeq
    |>Seq.skipWhile (fun t -> match t with VALUE y -> true | _ -> false)
    |>Seq.toList

// Takes VALUE tokens from the top of the list, contatenates their associated strings and returns the contatenated string.
let returnConcatHeadValues tokenList =
    tokenList
    |> List.toSeq
    |> Seq.takeWhile (fun t -> match t with VALUE y -> true | _ -> false)
    |> Seq.fold (fun acc elem -> match elem with VALUE y -> acc + " " + y | _ -> acc) " "
    |> fun s -> s.Trim()

let rec parseTokenListRec tokenList optionsSoFar =
    match tokenList with
    | [] -> optionsSoFar
    | SOURCE::t -> 
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with Source = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the source argument."
    | REGISTRY::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with Registry = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the registry argument."
    | ZIP::t -> parseTokenListRec t {optionsSoFar with Zip = ZipFiles}
    | SERVICE::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with Service = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the service argument."
    | DESTINATION::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with Destination = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the destination argument."
    | MAXDAYS::t ->
        match t with
        |VALUE x::tt when (isWholeNumber x) -> parseTokenListRec tt {optionsSoFar with MaxDays = int x}
        | _ -> failwith "Expected a whole number to be supplied after the maxdays argument."
    | SMTPSERVER::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with SmtpServer = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the smtp server argument."
    | FROMEMAIL::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with FromEmail = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the from email argument."
    | SUCCESSEMAIL::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with SuccessEmail = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the success email argument."
    | FAILUREEMAIL::t ->
        match t with
        | VALUE x::tt -> parseTokenListRec (returnNonValueTail t) {optionsSoFar with FailureEmail = (returnConcatHeadValues t)}
        | _ -> failwith "Expected a value after the failure email argument."
    | VALUE x::t -> failwith (sprintf "Encountered a value ('%s') without an associated argument." x)

let parseArgs args =
    let tokenList = tokenize(args)

    let defaultOptions = {
        Source = "";
        Registry = "";
        Zip = DoNotZipFiles;
        Service = "";
        Destination = "";
        MaxDays = 0;
        SmtpServer = "";
        FromEmail = "";
        SuccessEmail = "";
        FailureEmail = "";
        }

    parseTokenListRec tokenList defaultOptions