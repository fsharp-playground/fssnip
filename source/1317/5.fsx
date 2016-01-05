// [snippet:Abstract Syntax Tree]
type Name = string
type VarName = Name
type TypeName = Name
type MemberName = Name
type LabelName = Name
type Arg = Arg of TypeName * VarName
type Value = obj
type EnumValue = EnumValue of Name * Value
type Import = 
    | Import of Name
    | Abbreviation of Name * Name
type Access = Public | Private | Protected | Internal
type Modifier = Sealed | Static
type Literal = Literal of Value
type Expr = 
    | Value of Literal
    | Variable of VarName
    | MethodInvoke of MemberName * Expr list
    | PropertyGet of MemberName
    | InfixOp of Expr * string * Expr
    | PrefixOp of string * Expr
    | PostfixOp of Expr * string
    | TernaryOp of Expr * Expr * Expr
type Define = Define of TypeName * VarName
type Init = 
    | Assign of Name * Expr
    | Construct of TypeName * Name * Expr
type Condition = Expr
type Iterator = Expr
type Statement =
    | Definition of Define
    | Assignment of Init
    | PropertySet of MemberName * Expr
    | Invoke of MemberName * Expr list
    | If of Expr * Block
    | IfElse of Expr * Block * Block
    | Switch of Expr * Case list
    | For of Init list * Condition * Iterator list * Block
    | ForEach of Define * Expr * Block
    | While of Expr * Block
    | DoWhile of Block * Expr
    | Throw of Expr
    | Try of Block
    | Catch of TypeName * Block
    | Finally of Block
    | Lock of Expr * Block    
    | Using of Expr * Block
    | Label of LabelName
    | Goto of LabelName
    | Break
    | Continue
    | Return of Expr
and Case = 
    | Case of Literal * Block
    | Default of Block
and Block = Statement list
type MemberInfo = MemberInfo of Access * Modifier option * TypeName * Name
type Member =
    | Field of MemberInfo * Expr option
    | Constructor of MemberInfo
    | Method of MemberInfo * Arg list * Block
    | Property of MemberInfo * Block option * Block option
type Members = Member list   
type Implements = Name list
type CSharpType = 
    | Class of Access * Modifier option * Name * Implements * Members
    | Struct of Access * Name * Member list
    | Interface of Access * Name * Implements * Member list
    | Enum of Access * TypeName * EnumValue list    
type Scope =
    | Namespace of Import list * Name * Scope list
    | Types of Import list * CSharpType list
// [/snippet]

#r @"..\packages\FParsec.1.0.1\lib\net40-client\FParsecCS.dll"
#r @"..\packages\FParsec.1.0.1\lib\net40-client\FParsec.dll"

// [snippet:Parser]
open FParsec

// White space

let maxCount = System.Int32.MaxValue
let pcomment = pstring "//" >>. many1Satisfy ((<>) '\n') 
let pspaces = spaces >>. many (spaces >>. pcomment >>. spaces)
let pmlcomment = pstring "/*" >>. skipCharsTillString "*/" true (maxCount)

let ws = pspaces >>. many (pspaces >>. pmlcomment >>. pspaces) |>> (fun _ -> ())
let ws1 = spaces1
let str_ws s = pstring s .>> ws
let str_ws1 s = pstring s .>> ws1

// Literals

type Lit = NumberLiteralOptions
let numberFormat = Lit.AllowMinusSign ||| Lit.AllowFraction ||| Lit.AllowExponent
let pnumber : Parser<Literal, unit> =
    numberLiteral numberFormat "number"
    |>> fun nl ->
            if nl.IsInteger then Literal(int nl.String)
            else Literal(float nl.String)
let ptrue = str_ws "true" |>> fun _ -> Literal(true)
let pfalse = str_ws "false" |>> fun _ -> Literal(false)
let pbool = ptrue <|> pfalse
let pstringliteral =
    let normalChar = satisfy (fun c -> c <> '\\' && c <> '"')
    let unescape c = match c with
                     | 'n' -> '\n'
                     | 'r' -> '\r'
                     | 't' -> '\t'
                     | c   -> c
    let escapedChar = pstring "\\" >>. (anyOf "\\nrt\"" |>> unescape)
    between (pstring "\"") (pstring "\"")
            (manyChars (normalChar <|> escapedChar))
    |>> fun s -> Literal(s)

let pliteral = pnumber <|> pbool <|> pstringliteral

// Expressions

let reserved = ["default" (*;...*)] // "default:" collides with "label:"
let pidentifierraw =
    let isIdentifierFirstChar c = isLetter c || c = '_'
    let isIdentifierChar c = isLetter c || isDigit c || c = '_'
    many1Satisfy2L isIdentifierFirstChar isIdentifierChar "identifier"
let pidentifier =
    pidentifierraw 
    >>= fun s -> 
        if reserved |> List.exists ((=) s) then fail "keyword" 
        else preturn s
let pidentifier_ws = pidentifier .>> ws
let pvar = pidentifier |>> fun x -> Variable(x)
let pvalue = (pliteral |>> fun x -> Value(x)) <|> pvar

type Assoc = Associativity

let opp = OperatorPrecedenceParser<Expr,unit,unit>()
let pexpr = opp.ExpressionParser
let term = pvalue .>> ws <|> between (str_ws "(") (str_ws ")") pexpr
opp.TermParser <- term
let inops = ["+";"-";"*";"/";"==";"!=";"<=";">=";"<";">"]
for op in inops do
    opp.AddOperator(InfixOperator(op, ws, 1, Assoc.Left, fun x y -> InfixOp(x, op, y)))
let preops = ["-";"++";"--"]
for op in preops do
    opp.AddOperator(PrefixOperator(op, ws, 1, true, fun x -> PrefixOp(op, x)))
let postops = ["++";"--"]
for op in postops do
    opp.AddOperator(PostfixOperator(op, ws, 1, true, fun x -> PostfixOp(x, op)))

let pexpr' = between (str_ws "(") (str_ws ")") pexpr

// Statement blocks

let pstatement, pstatementimpl = createParserForwardedToRef()
let psinglestatement = pstatement |>> fun statement -> [statement]
let pstatementblock =
    psinglestatement <|>
    between (str_ws "{") (str_ws "}") (many pstatement) 

// Assignement statements

let pdefine = pipe2 (pidentifier .>> ws1) (pidentifier)
                (fun ty name -> Define(ty,name))

let pdefinition = pdefine |>> fun d -> Definition(d)

let passign = pipe3 pidentifier_ws (str_ws "=") pexpr
               (fun var _ expr -> Assign(var,expr))

let pconstruct = 
    pipe4
        (pidentifier .>> ws1)
        pidentifier_ws
        (str_ws "=")
        pexpr
        (fun ty name _ e -> Construct(ty, name, e))

let passignment = attempt passign <|> attempt pconstruct |>> fun c -> Assignment(c)

// Selection statements

let pif =
    pipe2 (str_ws "if" >>. pexpr') pstatementblock
        (fun e block -> If(e,block))

let pifelse =
    pipe3 (str_ws "if" >>. pexpr') pstatementblock (str_ws "else" >>. pstatementblock)
        (fun e t f -> IfElse(e,t,f))

let pcase = str_ws1 "case" >>. pliteral .>> str_ws ":" 
let pcaseblock = pipe2 pcase (many pstatement) (fun case block -> Case(case,block))
let pdefault = str_ws "default" >>. str_ws ":" 
let pdefaultblock = pdefault >>. (many pstatement) |>> fun block -> Default(block)
let pcases' = many pcaseblock .>>. opt pdefaultblock 
              |>> fun (cases,d) -> cases@(Option.toList d)
let pcases = between (str_ws "{") (str_ws "}") pcases'

let pswitch =
    pipe2 (str_ws "switch" >>. pexpr') pcases
        (fun e cases -> Switch(e, cases))

// Iteration statements

let pforargs =
    let pinit = attempt passign <|> attempt pconstruct
    pipe3 
        (sepBy pinit (str_ws ",") .>> str_ws ";")
        (pexpr .>> str_ws ";")
        (sepBy pexpr (str_ws ","))
        (fun from until steps -> from, until, steps)

let pfor =
    pipe2 
        (str_ws "for" >>. between (str_ws "(") (str_ws ")") pforargs)
        pstatementblock
        (fun (inits,until,iterators) block -> For(inits,until,iterators,block))

let pforeachargs =
    pipe3 pdefine (str_ws1 "in") pexpr
        (fun define _ collection -> define, collection)

let pforeach =
    pipe2 (str_ws "foreach" >>. pforeachargs) pstatementblock
        (fun (define,collection) block -> ForEach(define,collection,block))

let pwhile = 
    pipe2 (str_ws "while" >>. pexpr') pstatementblock        
        (fun e block -> While(e,block))

let pdowhile =
    pipe2
        (str_ws "do" >>. pstatementblock)
        (str_ws "while" >>. pexpr')
        (fun block e -> DoWhile(block, e))

// Jump statements

let preturn = str_ws1 "return" >>. pexpr |>> fun e -> Return(e)

let pbreak = str_ws "break" |>> fun _ -> Break

let pcontinue = str_ws "continue" |>> fun _ -> Continue

let pgoto = str_ws1 "goto" >>. pidentifier_ws |>> fun label -> Goto(label)

let plabel = 
    pidentifier_ws .>> str_ws ":" 
    |>> fun label -> Label(label)

// Exception statements

let pthrow = str_ws1 "throw" >>. pexpr |>> fun e -> Throw(e)

let ptry = str_ws "try" >>. pstatementblock |>> fun block -> Try(block)

let pfinally = str_ws "finally" >>. pstatementblock |>> fun block-> Finally(block)

let pexception = between (str_ws "(") (str_ws ")") pidentifier_ws
let pcatch = str_ws "catch" >>. pexception .>>. pstatementblock 
             |>> fun (ex,block) -> Catch(ex, block)

// Lock statement

let plock = 
    str_ws "lock" >>. pexpr' .>>. pstatementblock 
    |>> (fun (e,block) -> Lock(e,block))

// Statement implementation

pstatementimpl :=
    attempt (preturn .>> str_ws ";") <|>
    attempt (pbreak .>> str_ws ";") <|>
    attempt (pcontinue .>> str_ws ";") <|>
    attempt (pgoto .>> str_ws ";") <|>  
    attempt (pdefinition .>> str_ws ";") <|>
    attempt (passignment .>> str_ws ";") <|> 
    attempt plabel <|>
    attempt pifelse <|> attempt pif <|> 
    attempt pswitch <|>
    attempt pfor <|> attempt pforeach <|>
    attempt pwhile <|> attempt pdowhile <|>
    attempt pthrow <|>
    attempt ptry <|> attempt pcatch <|> attempt pfinally
    attempt plock

// Access

let ppublic = str_ws1 "public" |>> (fun _ -> Public)
let pprivate = str_ws1 "private" |>> (fun _ -> Private)
let pprotected = str_ws1 "protected" |>> (fun _ -> Protected)
let pinternal = str_ws1 "internal" |>> (fun _ -> Internal)
let paccess = 
    opt (ppublic <|> pprivate <|> pprotected <|> pinternal)
    |>> (fun access -> defaultArg access Internal)

// Modifiers

let psealed = str_ws1 "sealed" |>> (fun _ -> Sealed)
let pstatic = str_ws1 "static" |>> (fun _ -> Static)
let pmodifier = psealed <|> pstatic

// Arguments

let parg = pipe2 pidentifier_ws pidentifier_ws (fun ty name -> Arg(ty,name)) 
let pargs = str_ws "(" >>. sepBy parg (str_ws ",") .>> str_ws ")"

// Members

let pmemberinfo = 
    pipe4 paccess (opt pmodifier) pidentifier_ws pidentifier_ws
     (fun access modifier ty name -> MemberInfo(access,modifier,ty,name))

let pfield = pmemberinfo .>> str_ws ";" |>> (fun mi -> Field(mi, None))
let pget = str_ws "get" >>. pstatementblock
let pset = str_ws "set" >>. pstatementblock
let ppropertyblock =
    between (str_ws "{") (str_ws "}") ((opt pget) .>>. (opt pset))
let pproperty = 
    pipe2 pmemberinfo ppropertyblock
     (fun mi (gblock,sblock) -> Property(mi,gblock,sblock))
let pmethod =
    pipe3 pmemberinfo pargs pstatementblock 
     (fun mi args block -> Method(mi,args,block))

let pmember = attempt pfield <|> attempt pmethod <|> attempt pproperty

let pmembersblock = between (str_ws "{") (str_ws "}") (many pmember) 
                   |>> (fun members -> members)
let penumblock = 
    between (str_ws "{") (str_ws "}") (sepBy pidentifier_ws (str_ws ","))
    |>> fun names -> names |> List.mapi (fun i name -> EnumValue(name,i))

// Types

let pclasspreamble =
    paccess .>>. (opt pmodifier) .>> (str_ws1 "class")
let pimplements =
    opt (str_ws ":" >>. sepBy1 (pidentifier_ws) (str_ws ","))
    |>> function Some xs -> xs | None -> []
let pclass = 
    pipe4 pclasspreamble pidentifier_ws pimplements pmembersblock
     (fun (access,modifier) name implements block -> 
        Class(access, modifier, name, implements, block))
let pstruct =
    pipe4 paccess (str_ws1 "struct") pidentifier_ws pmembersblock
     (fun access _ name block -> Struct(access, name, block))
let pinterface =
    pipe5 paccess (str_ws1 "interface") pidentifier_ws pimplements pmembersblock
     (fun access _ name implements block -> 
        Interface(access, name, implements, block))
let penum =
    pipe4 paccess (str_ws1 "enum") pidentifier_ws penumblock
     (fun access _ name block -> Enum(access, name, block))
let ptypedeclaration = pclass <|> pstruct <|> pinterface <|> penum

// Scopes

let pscope, pscopeimpl = createParserForwardedToRef()
let pscopesblock = between (str_ws "{") (str_ws "}") (many pscope)                   

let pimport = str_ws1 "using" >>. pidentifier_ws .>> str_ws ";" |>> (fun name -> Import(name))

let pnsblock =
    pipe3 (many pimport) (str_ws1 "namespace" >>. pidentifier_ws) pscopesblock
     (fun imports name block -> 
        let types = Types([],[])
        Namespace(imports,name,block))

let ptypes = 
    pipe2 (many pimport) (many1 ptypedeclaration)
     (fun imports classes -> Types(imports, classes))

pscopeimpl := ws >>. (pnsblock <|> ptypes)
// [/snippet]

// [snippet:Example]
let program = """
using X; 
using Y; 
namespace A { 
  namespace B {
  /* My class */
  class MyClass : IMarker {
    float y;
    // My foo
    bool foo(string arg) {
      string s = "hello"; // assign string lit
      for(int i = 1; i<=100; i++) { }
      switch(y) { case 0: break; case 1: y = 1; default: }
      bool x;   
      x = true;  
      while(x) { 
        if (true) { x = false; } else if (false) { goto end; }
      }
end:
      return true;
    }
    bool MyProp {
      get { }
      set { y = value; }
    }
  } 
  struct MyStruct { }
  interface IMarker { }
  enum MyEnum { One, Two, Three }
  } 
}
"""

run pscope program
// [/snippet]