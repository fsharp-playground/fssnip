(*
Expression parsing with Antlr
http://vimeo.com/groups/29150/videos/8138418

Operator precedence hierarchy (in decreasing order):

Explicit parens: ()
Boolean negation: not
Unary: + -
Binary mul: * / mod
Binary add: + -
Relation: = /= < <= >= >
Logical: and or

term       -> IDENT
            | '(' expression ')'
            | INTEGER
negation   -> 'not'* term
unary      -> ('+' | '-')* negation
mult       -> unary (('*' | '/' | 'mod') unary)*
add        -> mult (('+' | '-') mult)*
relation   -> add (('=' | '/=' | '<' | '<=' | | '>=' | '>') add)*
expression -> relation (('and' | 'or') relation)*
*)