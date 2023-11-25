```CuteC main.cutec
var input =
    """
    int a = 42;
    int b = 69;

    void main(){
        int x = 2;
        int y = a + b;
    }
    """;
/*
//VAR TABLE
// 0x00 - reserved
// 0x01 - global::a
// 0x02 - global::b

int a = 42;  // SETREG GP_I32_0 42          ;SET REGISTER 0 TO VALUE 42
             // MEM_WRITE GP_I32_0, 0x01    ;SET MEMORY ADDRESS 0x00 TO VALUE IN REGISTER 0

int b = 69;  // SETREG GP_I32_0 69          ;SET REGISTER 0 TO VALUE 42
             // MEM_WRITE GP_I32_0, 0x02    ;SET MEMORY ADDRESS 0x01 TO VALUE IN REGISTER 0

// VAR_TABLE_MAIN
// 0x03 - main::x
// 0x04 - main::z
// 0x05 - main::y
//LBL FUNC_MAIN:
void main(){
    int x = 2;         // SETREG GP_I32_0 2
                       // MEM_WRITE GP_I32_0, 0x03

    int z = a + b;     // MEM_READ GP_I32_0 0x01   ; read global::a into gp_i32_0
                       // MEM_READ GP_I32_1 0x02   ; read global::b into gp_i32_1
                       // ADD GP_I32_0 GP_I32_1    ; gp_i32_0 + gp_i32_1 =into=> gp_i32_0
                       // MEM_WRITE GP_I32_0 0x4   ; write addition result to main::z

    int y = x + z * b; // MEM_READ GP_I32_0 0x04   ; read main::z into reg 0
                       // MEM_READ GP_I32_1 0x02   ; read global::b into reg 1
                       // MUL GP_I32_0 GP_I32_1    ; reg0 = reg0 * reg1  ; z*b->reg0
                       // MEM_READ GP_I32_1 0x03   ; read main::x into reg 1
                       // ADD GP_I32_0 GP_I32_1    ; reg0 = reg0 + reg1 ; x+zb->reg0
                       // MEM_WRITE GP_I32_0 0x05  ; write main::y with math result
    //RET
}

//CALL FUNC_MAIN
//HALT
```

VARIABLE_DEC
FUNCTION_DEC

```

┌───────┬──────────────┬────────────────
│ Words │ Tokens       │Tree?                
├───────┼──────────────┼────────────────
│ int   │ Type         │ VAR_DEC     NAME:c
│ c     │ VarName      │ │-TYPE: int
│ ;     │ EndLine      │      
│ int   │ Type         │ VAR_DEC     NAME:a
│ a     │ VarName      │ │-TYPE: int  
│ =     │ Assignment   │ │ VAR_ASSIGNMENT    
│ 42    │ TypedValue   │   │-TYPED_VALUE            
│ ;     │ EndLine      │                
│ int   │ Type         │ VAR_DEC    NAME:b   
│ b     │ VarName      │ │-TYPE: int
│ =     │ Assignment   │ │ VAR_ASSIGNMENT            
│ 69    │ TypedValue   │   |-TYPED_VALUE             
│ ;     │ EndLine      │                
│ fn    │ Function     │ FUNC_DEC NAME:main               
│ main  │ VarName      │ │-ARGS: void  
│ (     │ OpenParen    │ |-RET : void
│ )     │ CloseParen   │ │ VAR_DEC  NAME:main::x        
│ :     │ OfType       │ │ │-TYPE:int                  
│ void  │ Type         │ │ │-VARIABLE_ASSIGNMENT       
│ {     │ OpenBracket  │ │   |-TYPED_VALUE             
│ int   │ Type         │ │ VAR_DEC NAME:main::y
│ x     │ VarName      │ │ │-TYPE: int  
│ =     │ Assignment   │ │ │-VARIABLE_ASSIGNMENT
│ 2     │ TypedValue   │   | |-EXPR
│ ;     │ EndLine      │     │ │ SUM
│ int   │ Type         │       │- A
│ y     │ VarName      │       │- B
│ =     │ Assignment   │
│ a     │ VarName      │
│ +     │ Add          │
│ b     │ VarName      │
│ ;     │ EndLine      │
│ }     │ CloseBracket │ 
└───────┴──────────────┴
```

```
┌───────┬──────────────┐
│ Words │ Tokens       │
├───────┼──────────────┤
PROGRAM ROOT:
    VAR DEC:
        │ int   │ Type         │
        │ c     │ VarName      │
    │ ;     │ EndLine      │
    VAR DEC:
        │ int   │ Type         │
        │ a     │ VarName      │
    │ ;     │ EndLine      │
    VAR DEC:
        │ int   │ Type         │
        │ b     │ VarName      │
    │ ;     │ EndLine      │
    VAR ASSIGNMENT:
        │ a     │ VarName      │
        │ =     │ Assignment   │
        EXPRESSION:
            │ 42    │ TypedValue   │
    │ ;     │ EndLine      │
    VAR ASSIGNMENT:
        │ b     │ VarName      │
        │ =     │ Assignment   │
        EXPRESSION:
            │ 69    │ TypedValue   │
    │ ;     │ EndLine      │
    FUNC DEC:
        │ fn    │ Function     │
        │ main  │ VarName      │
        │ (     │ OpenParen    │
        │ )     │ CloseParen   │
        │ :     │ OfType       │
        │ void  │ Type         │
        BODY:
        │ {     │ OpenBracket  │
            VAR DEC:
                │ int   │ Type         │
                │ x     │ VarName      │
            │ ;     │ EndLine      │
            VAR DEC:
                │ int   │ Type         │
                │ y     │ VarName      │
            │ ;     │ EndLine      │
            VAR ASSIGNMENT:
                │ x     │ VarName      │
                │ =     │ Assignment   │
                EXPRESSION:
                    │ 2     │ TypedValue   │
            │ ;     │ EndLine      │
            VAR ASSIGNMENT:
                │ y     │ VarName      │
                │ =     │ Assignment   │
                EXPRESSION:
                    │ a     │ VarName      │
                    │ +     │ Add          │
                    │ b     │ VarName      │
            │ ;     │ EndLine      │
        │ }     │ CloseBracket │
└───────┴──────────────┘

.:: = happy puppy syntax

```
```
"""
//var table
// global::a - 0x00
int globalVarA = 42;


//global::fn_main::c - 0x01
fn main (  ) : void
{
    int lv_demo = globalVarA;
}

fn other_func():void{int c=0;}

main();
""";
```
