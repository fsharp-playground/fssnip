#r @"..\packages\FParsec.1.0.1\lib\net40-client\FParsecCS.dll"
#r @"..\packages\FParsec.1.0.1\lib\net40-client\FParsec.dll"

// [snippet:Abstract Syntax Tree]
type label = string
type identifier = string
type index = int
type MethodInfo = System.Reflection.MethodInfo
type arithmetic = Add | Subtract | Multiply | Divide
/// Small Basic comparison operaton
type comparison = Eq | Ne | Lt | Gt | Le | Ge
/// Small Basic logical operation
type logical = And | Or
/// Small Basic value
type value =
    | Bool of bool
    | Int of int
    | Double of double
    | String of string
/// Small Basic expression
type expr =
    | Literal of value
    | Var of identifier
    | GetAt of location
    | Func of call
    | Neg of expr
    | Arithmetic of expr * arithmetic * expr
    | Comparison of expr * comparison * expr
    | Logical of expr * logical * expr
and location =
    | Location of identifier * expr[]
and call =
    | Call of MethodInfo * expr[]
type assign =
    | Set of identifier * expr
type instruction =
    | Assign of assign
    | SetAt of location * expr 
    | Action of call
    | For of assign * expr * expr
    | EndFor
    | If of expr
    | ElseIf of expr
    | Else
    | EndIf
    | While of expr
    | EndWhile
    | Sub of identifier
    | EndSub
    | GoSub of identifier
    | Label of label
    | Goto of label
// [/snippet]

// [snippet:Parser]
open FParsec

type IMarker = interface end

let numberFormat = NumberLiteralOptions.AllowMinusSign ||| 
                   NumberLiteralOptions.AllowFraction ||| 
                   NumberLiteralOptions.AllowExponent                  

let pnumliteral: Parser<expr, unit> =
    numberLiteral numberFormat "number"
    |>> fun nl ->
            if nl.IsInteger then Literal(Int (int nl.String))
            else Literal(Double (float nl.String))

let ws = spaces
let str_ws s = pstring s .>> ws
let str_ws1 s = pstring s .>> spaces1

let pstringliteral = 
    between (pstring "\"") (pstring "\"") (manySatisfy (fun x -> x <> '"')) 
    |>> (fun s -> Literal(String(s)))

let pidentifier =
    let isIdentifierFirstChar c = isLetter c || c = '_'
    let isIdentifierChar c = isLetter c || isDigit c || c = '_'
    many1Satisfy2L isIdentifierFirstChar isIdentifierChar "identifier"
let pidentifier_ws = pidentifier .>> ws
let pvar = pidentifier |>> (fun x -> Var(x))

let pcall, pcallimpl = createParserForwardedToRef ()
let pfunc = pcall |>> (fun x -> Func(x))

let pvalue = pnumliteral <|> pstringliteral <|> attempt pfunc <|> attempt pvar

type Assoc = Associativity

let oppa = new OperatorPrecedenceParser<expr,unit,unit>()
let parithmetic = oppa.ExpressionParser
let terma = (pvalue .>> ws) <|> between (str_ws "(") (str_ws ")") parithmetic
oppa.TermParser <- terma
oppa.AddOperator(InfixOperator("+", ws, 1, Assoc.Left, fun x y -> Arithmetic(x, Add, y)))
oppa.AddOperator(InfixOperator("-", ws, 1, Assoc.Left, fun x y -> Arithmetic(x, Subtract, y)))
oppa.AddOperator(InfixOperator("*", ws, 2, Assoc.Left, fun x y -> Arithmetic(x, Multiply,y)))
oppa.AddOperator(InfixOperator("/", ws, 2, Assoc.Left, fun x y -> Arithmetic(x, Divide,y)))

let oppc = new OperatorPrecedenceParser<expr,unit,unit>()
let pcomparison = oppc.ExpressionParser
let termc = (parithmetic .>> ws) <|> between (str_ws "(") (str_ws ")") pcomparison
oppc.TermParser <- termc
oppc.AddOperator(InfixOperator("=", ws, 1, Assoc.Left, fun x y -> Comparison(x, Eq, y)))
oppc.AddOperator(InfixOperator("<>", ws, 1, Assoc.Left, fun x y -> Comparison(x, Ne, y)))
oppc.AddOperator(InfixOperator("<=", ws, 2, Assoc.Left, fun x y -> Comparison(x, Le, y)))
oppc.AddOperator(InfixOperator(">=", ws, 2, Assoc.Left, fun x y -> Comparison(x, Ge,y)))
oppc.AddOperator(InfixOperator("<", ws, 2, Assoc.Left, fun x y -> Comparison(x, Lt, y)))
oppc.AddOperator(InfixOperator(">", ws, 2, Assoc.Left, fun x y -> Comparison(x, Gt,y)))

let oppl = new OperatorPrecedenceParser<expr,unit,unit>()
let plogical = oppl.ExpressionParser
let terml = (pcomparison .>> ws) <|> between (str_ws "(") (str_ws ")") plogical
oppl.TermParser <- terml
oppl.AddOperator(InfixOperator("And", ws, 1, Assoc.Left, fun x y -> Logical(x,And,y)))
oppl.AddOperator(InfixOperator("Or", ws, 1, Assoc.Left, fun x y -> Logical(x,Or,y)))

let ptuple = between (str_ws "(") (str_ws ")") (sepBy parithmetic (str_ws ",")) |>> (fun xs -> xs)
pcallimpl := 
    pipe4 (pidentifier_ws) (pchar '.') (pidentifier) ptuple 
        (fun tn _ name args -> 
        let t = typeof<IMarker>.DeclaringType.GetNestedType(tn)
        let mi = t.GetMethod(name)
        Call(mi,args |> List.toArray) 
        )

let paction = pcall |>> (fun x -> Action(x))
let pset = pipe3 (pidentifier_ws) (str_ws "=") (parithmetic) (fun id _ n -> Set(id, n))
let passign = pipe3 (pidentifier_ws) (str_ws "=") (parithmetic) (fun id _ n -> Assign(Set(id, n)))
let pfor = 
    let pfrom = str_ws1 "For" >>. pset
    let pto = str_ws1 "To" >>. parithmetic
    let pstep = str_ws1 "Step" >>. parithmetic
    let toStep = function None -> Literal(Int(1)) | Some s -> s
    pipe3 pfrom pto (opt (pstep)) (fun e x s -> For(e, x, toStep s))
let pendfor = str_ws "EndFor" |>> (fun _ -> EndFor)
let pwhile = str_ws1 "While" >>. plogical |>> (fun e -> While(e))
let pendwhile = str_ws "EndWhile" |>> (fun _ -> EndWhile)
let pif = str_ws1 "If" >>. plogical |>>  (fun e -> If(e))
let pelseif = str_ws1 "ElseIf" >>. pcomparison |>> (fun e -> ElseIf(e))
let pelse = str_ws "Else" |>> (fun _ -> Else)
let pendif = str_ws "EndIf" |>> (fun _ -> EndIf)
let psub = str_ws1 "Sub" >>. pidentifier |>> (fun e -> Sub(e))
let pendsub = str_ws "EndSub" |>> (fun _ -> EndSub)
let pgosub = pidentifier_ws .>> (str_ws "()") |>> (fun routine -> GoSub(routine))
let plabel = pidentifier_ws .>> (str_ws ":") |>> (fun label -> Label(label))
let pgoto = str_ws1 "Goto" >>. pidentifier |>> (fun label -> Goto(label))

let pchoice =
    choice [
        attempt pfor;pendfor
        attempt pwhile;pendwhile
        attempt pif;attempt pelseif;pelse;pendif
        attempt psub;pendsub;attempt pgosub 
        attempt paction
        attempt passign        
        attempt plabel;
        attempt pgoto
    ]

let pinstruction = pipe3 ws pchoice eof (fun _ x _ -> x)
let parse (program:string) =
    let lines = program.Split('\n')
                |> Array.filter (fun x -> x.Trim().Length > 0)
    [|for line in lines ->
        match run pinstruction line with
        | Success(result, _, _)   -> result
        | Failure(errorMsg, e, s) -> failwith errorMsg
    |]
// [/snippet]

// [snippet:Library]
type TextWindow private () =
    static member WriteLine (o:obj) = System.Console.WriteLine(o)
// [/snippet]


// [snippet:Interpreter]
/// Converts value to obj
let fromObj (x:obj) =
    match x with
    | :? bool as x -> Bool x
    | :? int as x -> Int x
    | :? double as x -> Double x
    | :? string as x -> String x
    | null -> Int 0
    | x -> raise (new System.NotSupportedException(x.ToString()))
/// Converts value to obj
let toObj = function
    | Bool x -> box x
    | Int x -> box x
    | Double x -> box x
    | String x -> box x
/// Converts value to int
let toInt = function
    | Bool x -> raise (new System.NotSupportedException())
    | Int x -> x
    | Double x -> int x
    | String x -> int x
/// Converts value to bool
let toBool = function
    | Bool x -> x
    | _ -> raise (new System.NotSupportedException())
/// Coerces a tuple of numeric values to double
let (|AsDoubles|_|) = function
    | Double l, Double r -> Some(l,r)
    | Int l, Double r -> Some(double l,r)
    | Double l, Int r -> Some(l,double r)
    | _, _ -> None
/// Compares values
let compare lhs rhs =
    match lhs, rhs with
    | Bool l, Bool r -> l.CompareTo(r)
    | Int l, Int r -> l.CompareTo(r)
    | AsDoubles (l,r) -> l.CompareTo(r)
    | String l, String r -> l.CompareTo(r)
    | _ -> raise (new System.NotSupportedException(sprintf "%A %A" lhs rhs))

open System.Collections.Generic

type VarLookup = Dictionary<identifier,value>
type ArrayLookup = Dictionary<identifier,Dictionary<value,value>>

/// Evaluates expressions
let rec eval state (expr:expr) =
    let (vars:VarLookup), (arrays:ArrayLookup) = state
    match expr with
    | Literal x -> x
    | Var identifier -> vars.[identifier]
    | GetAt(Location(identifier,[|index|])) -> arrays.[identifier].[eval state index]
    | GetAt(Location(identifier,_)) -> raise (System.NotSupportedException())
    | Func(call) -> invoke state call
    | Neg x -> arithmetic (eval state x) Multiply (Int(-1))
    | Arithmetic(l,op,r) -> arithmetic (eval state l) op (eval state r)
    | Comparison(l,op,r) -> comparison (eval state l) op (eval state r)
    | Logical(l,op,r) -> logical (eval state l) op (eval state r)
and comparison lhs op rhs =
    let x = compare lhs rhs
    match op with
    | Eq -> x = 0   | Ne -> x <> 0
    | Lt -> x < 0   | Gt -> x > 0
    | Le -> x <= 0  | Ge -> x >= 0
    |> fromObj
and arithmetic lhs op rhs =
    match op, (lhs, rhs) with
    | Add, (Int l,Int r) -> Int(l + r)
    | Add, AsDoubles (l,r) -> Double(l + r)
    | Add, (String l, String r) -> String(l + r)
    | Subtract, (Int l,Int r) -> Int(l - r)
    | Subtract, AsDoubles (l,r) -> Double(l - r)
    | Multiply, (Int l,Int r) -> Int(l * r)
    | Multiply, AsDoubles (l,r) -> Double(l * r)
    | Divide, (Int l,Int r) -> Int(l - r)
    | Divide, AsDoubles (l,r) -> Double(l - r)
    | _ -> raise (System.NotImplementedException())
and logical lhs op rhs =
    match op, lhs, rhs with
    | And, Bool l, Bool r -> Bool(l && r)
    | Or, Bool l, Bool r -> Bool(l || r)
    | _, _, _ -> raise (System.NotImplementedException())
and invoke state (Call(mi,args)) =
    let args = args |> Array.map (eval state >> toObj)
    mi.Invoke(null, args) |> fromObj

/// Runs program
let run (program:instruction[]) =
    /// Program index
    let pi = ref 0
    /// Variable lookup   
    let variables = VarLookup()
    /// Array lookup
    let arrays = ArrayLookup()
    /// Current state
    let state = variables, arrays
    /// For from EndFor lookup
    let forLoops = Dictionary<index, index * identifier * expr * expr>()
    /// While from EndWhile lookup
    let whileLoops = Dictionary<index, index>()
    /// Call stack for Gosubs
    let callStack = Stack<index>()
    /// Evaluates expression with variables
    let eval = eval (variables,arrays)
    /// Assigns result of expression to variable
    let assign (Set(identifier,expr)) = variables.[identifier] <- eval expr
    /// Finds first index of instructions
    let findFirstIndex start (inc,dec) instructions =
        let mutable i = start
        let mutable nest = 0
        while nest > 0 || instructions |> List.exists ((=) program.[i]) |> not do 
            if inc program.[i] then nest <- nest + 1
            if nest > 0 && dec program.[i] then nest <- nest - 1
            i <- i + 1
        i
    /// Finds index of instruction
    let findIndex start (inc,dec) instruction =
        findFirstIndex start (inc,dec) [instruction]
    let isIf = function If(_) -> true | _ -> false
    let isEndIf = (=) EndIf
    let isFor = function For(_,_,_) -> true | _ -> false
    let isEndFor = (=) EndFor
    let isWhile = function While(_) -> true | _ -> false
    let isEndWhile = (=) EndWhile
    let isFalse _ = false
    /// Instruction step
    let step () =
        let instruction = program.[!pi]
        match instruction with
        | Assign(set) -> assign set
        | SetAt(Location(identifier,[|index|]),expr) ->
            let array = 
                match arrays.TryGetValue(identifier) with
                | true, array -> array
                | false, _ -> 
                    let array = Dictionary<value,value>()
                    arrays.Add(identifier,array)
                    array
            array.[eval index] <- eval expr
        | SetAt(Location(_,_),expr) -> raise (System.NotSupportedException())
        | Action(call) -> invoke state call |> ignore
        | If(condition) ->            
            if eval condition |> toBool |> not then
                let index = findFirstIndex (!pi+1) (isIf, isEndIf) [Else;EndIf]
                pi := index
        | ElseIf(condition) -> raise (System.NotImplementedException())
        | Else ->
            let index = findIndex !pi (isIf,isEndIf) EndIf
            pi := index
        | EndIf -> ()
        | For((Set(identifier,expr) as from), target, step) ->
            assign from
            let index = findIndex (!pi+1) (isFor,isEndFor) EndFor
            forLoops.[index] <- (!pi, identifier, target, step)
            if toInt(variables.[identifier]) > toInt(eval target) 
            then pi := index
        | EndFor ->
            let start, identifier, target, step = forLoops.[!pi]
            let x = variables.[identifier]
            variables.[identifier] <- arithmetic x Add (eval step)
            if toInt(variables.[identifier]) <= toInt(eval target) 
            then pi := start
        | While condition ->
            let index = findIndex (!pi+1) (isWhile,isEndWhile) EndWhile
            whileLoops.[index] <- !pi 
            if eval condition |> toBool |> not then pi := index
        | EndWhile ->
            pi := whileLoops.[!pi] - 1
        | Sub(identifier) ->
            pi := findIndex (!pi+1) (isFalse, isFalse) EndSub
        | GoSub(identifier) ->
            let index = findIndex 0 (isFalse, isFalse) (Sub(identifier))
            callStack.Push(!pi)
            pi := index
        | EndSub ->
            pi := callStack.Pop()
        | Label(label) -> ()
        | Goto(label) -> pi := findIndex 0 (isFalse,isFalse) (Label(label))
    while !pi < program.Length do step (); incr pi
// [/snippet]

// [snippet:Fizz Buzz sample]
let source = """
Sub Modulus
  Result = Dividend
  While Result >= Divisor
    Result = Result - Divisor
  EndWhile
EndSub

For A = 1 To 100        
  Dividend = A
  Divisor = 15
  Modulus()
  If Result = 0
    TextWindow.WriteLine("FizzBuzz")  
  Else        
    Dividend = A
    Divisor = 3
    Modulus()
    If Result = 0
    TextWindow.WriteLine("Fizz")
    Else
      Dividend = A
      Divisor = 5
      Modulus()
      If Result = 0
        TextWindow.WriteLine("Buzz")
      Else
        TextWindow.WriteLine(A)        
      EndIf
    EndIf
  EndIf
EndFor"""

let program = parse source
run program
// [/snippet]