// [snippet:Abstract Syntax Tree]
type Name = string
type VarName = Name
type TypeName = Name
type MemberName = Name
type LabelName = Name
type Arg = Arg of TypeName * VarName
type Value = obj
type EnumValue = Name * Value
type Import = 
    | Import of Name
    | Abbreviation of Name * Name
type Access = Public | Private | Protected | Internal
type Modifier = Sealed | Static
type Expr = 
    | Literal of Value
    | Variable of VarName
    | MethodInvoke of MemberName * Expr list
    | PropertyGet of MemberName
    | InfixOperator of Expr * string * Expr
    | PrefixOperator of string * Expr
    | PostfixOperator of Expr * string
    | TernaryOperator of Expr * Expr * Expr
type From = TypeName * Name * Expr
type To = Expr
type Step = Expr list
type Statement =
    | Define of TypeName * VarName
    | Assignment of VarName * Expr
    | PropertySet of MemberName * Expr
    | If of Expr * Block
    | Else of Expr * Block
    | Switch of Expr * Case list
    | For of From * To * Step * Block
    | ForEach of TypeName * VarName * Expr    
    | While of Expr * Statement list
    | DoWhile of Statement List * Expr
    | Try of Statement list
    | Catch of TypeName * Block
    | Finally of TypeName * Block
    | Lock of Expr * Block    
    | Using of Expr * Block
    | Label of LabelName
    | Goto of LabelName
    | Break
    | Continue
    | Return of Expr
and Case = Case of Value option * Block
and Block = Statement list
type MemberInfo = MemberInfo of Access * Modifier option * TypeName * Name
type Member =
    | Field of MemberInfo * Expr option
    | Method of MemberInfo * Arg list * Block
    | Property of MemberInfo * Block
type CSharpType = 
    | Class of Access * Modifier option * Name * Member list
    | Struct of Access * Name * Member list
    | Enum of Access * TypeName * EnumValue list
type Scope =
    | Namespace of Import list * Name * Scope list
    | Types of Import list * CSharpType list
// [/snippet]

#r @"..\packages\FParsec.1.0.1\lib\net40-client\FParsecCS.dll"
#r @"..\packages\FParsec.1.0.1\lib\net40-client\FParsec.dll"

// [snippet:Parser]
open FParsec

let ws = spaces
let str_ws s = pstring s .>> ws
let str_ws1 s = pstring s .>> spaces1

type Lit = NumberLiteralOptions
let numberFormat = Lit.AllowMinusSign ||| Lit.AllowFraction ||| Lit.AllowExponent
let pnumber : Parser<Expr, unit> =
    numberLiteral numberFormat "number"
    |>> fun nl ->
            if nl.IsInteger then Literal(int nl.String)
            else Literal(float nl.String)

let pidentifier =
    let isIdentifierFirstChar c = isLetter c || c = '_'
    let isIdentifierChar c = isLetter c || isDigit c || c = '_'
    many1Satisfy2L isIdentifierFirstChar isIdentifierChar "identifier"
let pidentifier_ws = pidentifier .>> ws
let pvar = pidentifier |>> (fun x -> Variable(x))
let pexpr = pnumber <|> pvar

let passign = pipe3 pidentifier_ws (str_ws "=") pexpr
               (fun var _ expr -> Assignment(var,expr))

let pstatement = passign .>> str_ws ";"

let ppublic = str_ws1 "public" |>> (fun _ -> Public)
let pprivate = str_ws1 "private" |>> (fun _ -> Private)
let pprotected = str_ws1 "protected" |>> (fun _ -> Protected)
let pinternal = str_ws1 "internal" |>> (fun _ -> Internal)
let paccess = 
    opt (ppublic <|> pprivate <|> pprotected <|> pinternal)
    |>> (fun access -> defaultArg access Internal)
let psealed = str_ws1 "sealed" |>> (fun _ -> Sealed)
let pstatic = str_ws1 "static" |>> (fun _ -> Static)
let pmodifier = psealed <|> pstatic

let pstatementblock = between (str_ws "{") (str_ws "}") (many pstatement) 

let parg = pipe2 pidentifier_ws pidentifier_ws (fun ty name -> Arg(ty,name)) 
let pargs = str_ws "(" >>. sepBy parg (str_ws ",") .>> str_ws ")"

let pmemberinfo = 
    pipe4 paccess (opt pmodifier) pidentifier_ws pidentifier_ws
     (fun access modifier ty name -> MemberInfo(access,modifier,ty,name))

let pfield = pmemberinfo .>> str_ws ";" |>> (fun mi -> Field(mi, None))
let pproperty = 
    pipe2 pmemberinfo pstatementblock (fun mi block -> Property(mi,block))
let pmethod =
    pipe3 pmemberinfo pargs pstatementblock 
     (fun mi args block -> Method(mi,args,block))

let pmember = attempt pfield <|> attempt pproperty <|> attempt pmethod

let pmemberblock = between (str_ws "{") (str_ws "}") (many pmember) 
                   |>> (fun members -> members)
let penumblock = between (str_ws "{") (str_ws "}") ws |>> (fun _ -> [])

let pclass = 
    pipe5 paccess (opt pmodifier) (str_ws1 "class") pidentifier_ws pmemberblock
     (fun access modifier _ name block -> Class(access, modifier, name, block))
let pstruct =
    pipe4 paccess (str_ws1 "struct") pidentifier_ws pmemberblock
     (fun access _ name block -> Struct(access, name, block))
let penum =
    pipe4 paccess (str_ws1 "enum") pidentifier_ws penumblock
     (fun access _ name block -> Struct(access, name, block))
let ptypedeclaration = pclass <|> pstruct <|> penum

let pscope, pscopeimpl = createParserForwardedToRef()

let pscopesblock = between (str_ws "{") (str_ws "}") (many pscope) |>> (fun scopes -> scopes)

let pimport = str_ws1 "using" >>. pidentifier .>> str_ws ";" |>> (fun name -> Import(name))
let pnsblock =
    pipe3 (many pimport) (str_ws1 "namespace" >>. pidentifier_ws) pscopesblock
     (fun imports name block -> 
        let types = Types([],[])
        Namespace(imports,name,block))
let ptypes = 
    pipe2 (many pimport) (many1 ptypedeclaration)
     (fun imports classes -> Types(imports, classes))
pscopeimpl := pnsblock <|> (ptypes)
// [/snippet]

// [snippet:Example]
let program = """using X; 
using Y; 
namespace A { 
  namespace B { 
  class A { 
    float x;
    void foo(string arg) { 
      x = 1;
    }
  } 
  struct B { } 
  enum C { }
  } 
}
"""

run pscope program
// [/snippet]