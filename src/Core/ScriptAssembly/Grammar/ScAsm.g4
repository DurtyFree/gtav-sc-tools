grammar ScAsm;

script
    : (statement? EOL)* EOF
    ;

statement
    : directive
    | function
    | struct
    | statics
    | args
    ;

struct
    : K_STRUCT identifier K_BEGIN EOL
      (fieldDeclWithInitializer? EOL)*
      K_END
    ;

statics
    : K_STATICS K_BEGIN
      (fieldDeclWithInitializer? EOL)*
      K_END
    ;

args
    : K_ARGS K_BEGIN
      (fieldDeclWithInitializer? EOL)*
      K_END
    ;

fieldDeclWithInitializer
    : fieldDecl ('=' (integer | float))?
    ;

fieldDecl
    : identifier ':' type
    ;

type
    : identifier
    | type '[' integer ']'
    ;

function
    : K_FUNC K_NAKED? identifier functionArgList? functionReturnType?
      (functionLocalDecl? EOL)*
      K_BEGIN EOL
      (functionBody? EOL)*
      K_END
    ;

functionArgList
    : '(' (fieldDecl (',' fieldDecl)*)? ')'
    ;

functionReturnType
    : ':' type
    ;

functionLocalDecl
    : fieldDecl
    ;

functionBody
    : label? instruction?
    ;

instruction
    : identifier operandList
    ;

directive
    : '$' identifier operandList
    ;

operandList
    : operand*
    ;

operand
    : integer
    | float
    | string
    | identifier
    | operandSwitchCase
    | operator
    ;

operator
    : K_SIZEOF '(' identifier ('.' identifier)* ')'
    | K_OFFSETOF '(' identifier ('.' identifier)+ ')'
    | K_ITEMSIZEOF '(' identifier ('.' identifier)* ')'
    | K_LENGTHOF '(' identifier ('.' identifier)* ')'
    | K_HASH '(' string ')'
    | K_ADDROF '(' identifier ')'
    ;

operandSwitchCase
    : integer ':' identifier
    ;

label
    : identifier ':'
    ;

identifier
    : IDENTIFIER
    ;

integer
    : DECIMAL_INTEGER
    | HEX_INTEGER
    ;

float
    : FLOAT
    ;

string
    : STRING
    ;

// keywords
K_FUNC : F U N C;
K_STRUCT : S T R U C T;
K_BEGIN : B E G I N;
K_END : E N D;
K_NAKED : N A K E D;
K_STATICS : S T A T I C S;
K_ARGS : A R G S;
K_SIZEOF : S I Z E O F;
K_OFFSETOF : O F F S E T O F;
K_ITEMSIZEOF : I T E M S I Z E O F;
K_LENGTHOF : L E N G T H O F;
K_HASH : H A S H;
K_ADDROF : A D D R O F;

IDENTIFIER
    :   [a-zA-Z_] [a-zA-Z_0-9]*
    ;

FLOAT
    : DECIMAL_INTEGER '.' DECIMAL_INTEGER
    ;

DECIMAL_INTEGER
    :   [-+]? DIGIT+
    ;

HEX_INTEGER
    :   '0x' HEX_DIGIT+
    ;

STRING
    : UNCLOSED_STRING '"'
    ;

UNCLOSED_STRING
    : '"' (~["\\\r\n] | '\\' (. | EOF))*
    ;

COMMENT
    : ';' ~ [\r\n]* -> skip
    ;

EOL
    : [\r\n]+
    ;

WS
    : [ \t] -> skip
    ;

fragment DIGIT : [0-9];
fragment HEX_DIGIT : [0-9a-fA-F];

fragment A : [aA];
fragment B : [bB];
fragment C : [cC];
fragment D : [dD];
fragment E : [eE];
fragment F : [fF];
fragment G : [gG];
fragment H : [hH];
fragment I : [iI];
fragment J : [jJ];
fragment K : [kK];
fragment L : [lL];
fragment M : [mM];
fragment N : [nN];
fragment O : [oO];
fragment P : [pP];
fragment Q : [qQ];
fragment R : [rR];
fragment S : [sS];
fragment T : [tT];
fragment U : [uU];
fragment V : [vV];
fragment W : [wW];
fragment X : [xX];
fragment Y : [yY];
fragment Z : [zZ];