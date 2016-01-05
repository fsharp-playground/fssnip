namespace IronJS.Compiler
module Lexer =
  
  open System

  module private Char =
    
    type private Cat = 
      Globalization.UnicodeCategory

    let inline isCRLF cr lf = cr = '\r' && lf = '\n'
    let inline isAlpha c = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')
    let inline isDecimal c = c >= '0' && c <= '9'
    let inline isOctal c = c >= '0' && c <= '7'
    let inline isHex c = isDecimal c || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')
    let inline isQuote c = c = '"' || c = '\''
    let inline isLineTerminator c = 
      match c with
      | '\n'|'\r'|'\u2028'|'\u2029' -> true
      | _ -> false

    let inline isWhiteSpace (c:Char) =
      match int c with
      | 0x09   | 0x0B   | 0x0C   
      | 0x20   | 0xA0   | 0x1680 
      | 0x180E | 0x202F | 0x205F 
      | 0x3000 | 0xFEFF -> true
      | c -> c >= 8192 && c <= 8202;

    let isUnicodeIdentifierStart c =
      match c |> Char.GetUnicodeCategory with
      | Cat.UppercaseLetter
      | Cat.LowercaseLetter
      | Cat.TitlecaseLetter
      | Cat.ModifierLetter
      | Cat.OtherLetter 
      | Cat.LetterNumber -> true
      | _ -> false

    let inline isIdentifierStart c =
      if c |> isAlpha || c = '_' || c = '$' 
        then true
        else c |> isUnicodeIdentifierStart

    let isUnicodeIdentifier c =
      match c |> Char.GetUnicodeCategory with
      | Cat.UppercaseLetter 
      | Cat.LowercaseLetter
      | Cat.TitlecaseLetter
      | Cat.ModifierLetter
      | Cat.OtherLetter
      | Cat.LetterNumber 
      | Cat.NonSpacingMark
      | Cat.SpacingCombiningMark
      | Cat.DecimalDigitNumber
      | Cat.ConnectorPunctuation -> true
      | _ -> int c = 0x200C || int c = 0x200D

    let inline isIdentifier c = 
      if c |> isAlpha || c = '_' || c = '$' || c |> isDecimal 
        then true
        else c |> isUnicodeIdentifier

    let inline isPunctuation_Simple c =
      match c with
      | '{' | '}' | '(' | ')' | '[' | ']' 
      | ';' | ',' | '?' | ':' | '~' -> true
      | _ -> false

    let inline isPunctuation c =
      match c with
      | '<' | '>' | '=' | '+' | '-' | '!'
      | '%' | '&' | '|' | '^' | '*' -> true
      | _ -> false

  type Symbol
    //Keywords
    = Break = 0
    | Case = 1
    | Catch = 2
    | Continue = 3
    | Default = 4
    | Delete = 5
    | Do = 6
    | Else = 7
    | Finally = 9
    | Function = 10
    | If = 11
    | In = 12
    | InstanceOf = 13
    | New = 14
    | Return = 16
    | Switch = 17
    | This = 18
    | Throw = 19
    | Try = 20
    | TypeOf = 22
    | Var = 23
    | Void = 24
    | While = 25
    | With = 26
    | For = 84

    //Punctuation
    | LeftBrace = 27
    | RightBrace = 28
    | LeftParenthesis = 29
    | RightParenthesis = 30
    | LeftBracket = 31
    | RightBracket = 32
    | Semicolon = 33
    | Comma = 34
    | Equal = 35
    | NotEqual = 36
    | StrictEqual = 37
    | StrictNotEqual = 38
    | LessThan = 39
    | GreaterThan = 40
    | LessThanOrEqual = 41
    | GreaterThanOrEqual = 42
    | Plus = 43
    | Minus = 44
    | Multiply = 45
    | Divide = 46
    | Modulo = 47
    | Increment = 48
    | Decrement = 49
    | LeftShift = 50
    | RightShift = 51
    | URightShift = 52
    | BitwiseAnd = 53
    | BitwiseOr = 54
    | BitwiseXor = 55
    | BitwiseNot = 56
    | LogicalNot = 57
    | LogicalAnd = 58
    | LogicalOr = 59
    | Condition = 60
    | Colon = 61
    | Assign = 62
    | AssignAdd = 63
    | AssignSubtract = 64
    | AssignMultiply = 65
    | AssignDivide = 66
    | AssignModulo = 67
    | AssignLeftShift = 68
    | AssignSignedRightShift = 69
    | AssignUnsignedRightShift = 70
    | AssignBitwiseAnd = 71
    | AssignBitwiseOr = 72
    | AssignBitwiseXor = 73
    | Dot = 74

    //Literals
    | True = 75
    | False = 76
    | Null = 77
    | String = 78
    | Number = 79
    | RegExp = 80
    | LineTerminator = 81
    | Identifier = 82
    | Comment = 83

    //Special
    | StartOfInput = 100
    | EndOfInput = 101

  type Token = Symbol * string * int * int

  module private Input = 
    
    [<NoComparison>]
    type T = 
      val mutable Source : string
      val mutable Index : int
      val mutable Line : int
      val mutable Column : int
      val mutable Buffer : Text.StringBuilder
      val mutable Previous : Symbol

      new (source) = {
        Source = source
        Index = 0
        Line = 1
        Column = 0
        Buffer = Text.StringBuilder(1024)
        Previous = Symbol.StartOfInput
      }

    let create (input:string) = T(input)
    let inline newline (t:T) = t.Line <- t.Line + 1
    let inline current (t:T) = t.Source.[t.Index]
    let inline previous (t:T) = t.Source.[t.Index-1]
    let inline peek (t:T) = t.Source.[t.Index+1]
    let inline canPeek (t:T) = t.Index+1 < t.Source.Length
    let inline continue' (t:T) = t.Index < t.Source.Length
    let inline position (t:T) = t.Line, t.Column
    let inline rewind (t:T) = t.Index <- t.Index - 1
    let inline advance (t:T) = 
      t.Index <- t.Index + 1
      t.Column <- t.Column + 1

    let inline skip n (t:T) = 
      t.Index <- t.Index + n
      t.Column <- t.Column + n

    let inline buffer (t:T) (c:Char) = t.Buffer.Append(c) |> ignore
    let inline clearBuffer (t:T) = t.Buffer.Clear() |> ignore
    let inline bufferValue (t:T) = t.Buffer.ToString()
    let inline nextLine (t:T) =
      if t |> current = '\r' && t |> canPeek && t |> peek = '\n' 
        then t |> advance
        
      t.Line <- t.Line + 1
      t.Column <- 0

    let inline setupBuffer (t:T) =
      t |> advance
      t |> clearBuffer
      t.Line, t.Column

    let inline output symbol value line col (t:T) =
      t.Previous <- symbol
      symbol, value, line, col

    let endOfInput (t:T) =
      t |> output Symbol.EndOfInput null -1 -1
      
  open Char
  open Input
  open System.Collections.Generic

  [<Literal>]
  let private empty:string = null

  let private keywordMap = 
    [
      ("break", Symbol.Break)
      ("case", Symbol.Case)
      ("catch", Symbol.Catch)
      ("continue", Symbol.Continue)
      ("default", Symbol.Default)
      ("delete", Symbol.Delete)
      ("do", Symbol.Do)
      ("else", Symbol.Else)
      ("finally", Symbol.Finally)
      ("function", Symbol.Function)
      ("if", Symbol.If)
      ("in", Symbol.In)
      ("instanceof", Symbol.InstanceOf)
      ("new", Symbol.New)
      ("return", Symbol.Return)
      ("switch", Symbol.Switch)
      ("this", Symbol.This)
      ("throw", Symbol.Throw)
      ("try", Symbol.Try)
      ("typeof", Symbol.TypeOf)
      ("var", Symbol.Var)
      ("void", Symbol.Void)
      ("while", Symbol.While)
      ("with", Symbol.With)
      ("for", Symbol.For)
    ] |> Map.ofList

  let private punctuationMap =
    new Dictionary<string, Symbol>(
      [
        ("==", Symbol.Equal) 
        ("!=", Symbol.NotEqual) 
        ("===", Symbol.StrictEqual) 
        ("!==", Symbol.StrictNotEqual)
        ("<", Symbol.LessThan) 
        (">", Symbol.GreaterThan) 
        ("<=", Symbol.LessThanOrEqual)
        (">=", Symbol.GreaterThanOrEqual)
        ("+", Symbol.Plus)
        ("-", Symbol.Minus) 
        ("*", Symbol.Multiply) 
        ("%", Symbol.Modulo) 
        ("++", Symbol.Increment) 
        ("--", Symbol.Decrement)
        ("<<", Symbol.LeftShift) 
        (">>", Symbol.RightShift) 
        (">>>", Symbol.URightShift) 
        ("&", Symbol.BitwiseAnd) 
        ("|", Symbol.BitwiseOr)
        ("^", Symbol.BitwiseXor) 
        ("&&", Symbol.LogicalAnd) 
        ("||", Symbol.LogicalOr) 
        ("=", Symbol.Assign) 
        ("+=", Symbol.AssignAdd)
        ("-=", Symbol.AssignSubtract) 
        ("*=", Symbol.AssignMultiply) 
        ("%=", Symbol.AssignModulo) 
        ("<<=", Symbol.AssignLeftShift) 
        (">>=", Symbol.AssignSignedRightShift)
        (">>>=", Symbol.AssignUnsignedRightShift)
        ("&=", Symbol.AssignBitwiseAnd)
        ("|=", Symbol.AssignBitwiseOr)
        ("^=", Symbol.AssignBitwiseXor)
      ] |> Map.ofList
    )

  let private simplePunctuation (s:Input.T) c =
    s |> advance

    let symbol =
      match c with
      | '{' -> Symbol.LeftBrace
      | '}' -> Symbol.RightBrace
      | '(' -> Symbol.LeftParenthesis
      | ')' -> Symbol.RightParenthesis
      | '[' -> Symbol.LeftBracket
      | ']' -> Symbol.RightBracket
      | ';' -> Symbol.Semicolon
      | ',' -> Symbol.Comma
      | '?' -> Symbol.Condition
      | ':' -> Symbol.Colon
      | '~' -> Symbol.BitwiseNot
      | _ -> failwithf "Invalid simple punctuation %c" c

    symbol, empty, s.Line, s.Column

  let private identifier (s:Input.T) (first:char) =
    let line = s.Line
    let column = s.Column

    s |> advance
    s |> clearBuffer
    first |> buffer s

    while s |> continue' && s |> current |> isIdentifier do
      s |> current |> buffer s
      s |> advance

    let identifier = s |> bufferValue
    match keywordMap.TryFind identifier with
    | None -> s |> output Symbol.Identifier identifier line column
    | Some keyword -> s |> output keyword empty line column

  let private singleLineComment (s:Input.T) =
    let line = s.Line
    let column = s.Column

    s |> clearBuffer
    s |> advance

    while s |> continue' && s |> current |> isLineTerminator |> not do
      s |> current |> buffer s
      s |> advance

    s |> output Symbol.Comment (s |> bufferValue) line column

  let private multiLineComment (s:Input.T) =
    let line = s.Line
    let column = s.Column

    s |> clearBuffer
    s |> advance

    while current s <> '*' && peek s <> '/' do
      if s |> current |> isLineTerminator then
        s |> nextLine

      s |> current |> buffer s
      s |> advance
      
    s |> skip 2
    s |> output Symbol.Comment (s |> bufferValue) line column

  let private punctuation (s:Input.T) (first:char) =
    
    let inline makeToken (s:Input.T) (buffer:string) =
      let column = s.Column - buffer.Length
      s |> output punctuationMap.[buffer] empty s.Line column

    let rec punctuation (s:Input.T) (buffer:string) =
      s |> advance

      if s |> continue' then
        let newBuffer = buffer + (s |> current |> string)

        if punctuationMap.ContainsKey(newBuffer) 
          then newBuffer |> punctuation s
          else buffer |> makeToken s

      else
        buffer |> makeToken s

    first |> string |> punctuation s

  let private literalString (s:Input.T) (stop:char) =
    let line = s.Line
    let column = s.Column

    s |> clearBuffer

    let rec literalString (s:Input.T) =
      s |> advance

      if s |> continue' then
        match s |> current with
        | c when c |> isLineTerminator -> 
          failwith "Unexpected newline in literal string"

        | '\\' -> 
          s |> advance

          match s |> current with
          | 'n' -> '\n' |> buffer s
          | 'r' -> '\r' |> buffer s
          | 'b' -> '\b' |> buffer s
          | 'f' -> '\f' |> buffer s
          | 't' -> '\t' |> buffer s
          | 'v' -> '\v' |> buffer s
          | c -> c |> buffer s

          s |> literalString

        | c when c = stop ->
          s |> advance
          s |> output Symbol.String (s |> bufferValue) line column

        | c -> 
          c |> buffer s
          s |> literalString

      else
        failwith "Unexpected end of source in string"

    s |> literalString

  let create (source:string) =
    let buffer = Text.StringBuilder(512)
    let s = source |> Input.create

    let rec lexer () = 
      if s |> continue' then
        match s |> current with
        | c when c |> isWhiteSpace -> 
          s |> advance; lexer()

        | c when c |> isPunctuation_Simple ->
          c |> simplePunctuation s

        | c when c |> isPunctuation ->
          c |> punctuation s

        | c when c |> isIdentifierStart ->
          c |> identifier s

        | c when c |> isQuote ->
          c |> literalString s

        // Deal with comments, division and regexp literals
        | '/' ->
          s |> advance
          match s |> current with
          | '/' -> s |> singleLineComment
          | '*' -> s |> multiLineComment

        | c when c |> isLineTerminator ->
          s |> nextLine
          s |> advance
          s |> output Symbol.LineTerminator empty s.Line s.Column

        | c -> 
          failwithf "Incorrect input %c" c

      else
        s |> endOfInput

    lexer