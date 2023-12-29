# INSTRUCTIONS

## ASM SYMBOLS

| INSTRUCTION | ARG 0     | ARG 1 | NOTES                             |
|-------------|-----------|-------|-----------------------------------|
| NOOP        |           |       |                                   |
| SETREG      | DEST      | SRC   |                                   |
| ADD         | DEST      | SRC   |                                   |
| SUB         | DEST      | SRC   |                                   |
| DIV         | DEST      | SRC   |                                   |
| MUL         | DEST      | SRC   |                                   |
| LBL         | NAME      |       |                                   |
| CALL        | LBL / REG |       |                                   |
| HALT        |           |       |                                   |
| RET         |           |       |                                   |
| PUSH        | SRC       |       |                                   |
| POP         | DEST      |       |                                   |
| INC         | REG       |       |                                   |
| DEC         | REG       |       |                                   |
| CMP         | SRC A     | SRC B |                                   |
| JMP_EQ      | LBL NAME  |       |                                   |
| JMP_NEQ     | LBL NAME  |       |                                   |
| JMP_GTR     | LBL NAME  |       |                                   |
| JMP_GEQ     | LBL NAME  |       |                                   |
| JMP_LES     | LBL NAME  |       |                                   |
| JMP_LEQ     | LBL NAME  |       |                                   |
| JMP         | LBL NAME  |       |                                   |
| MEM_INTREAD | DEST      | ADDR  | Reads from mem addres -> dest reg |
| MEM_WRITE   | VAL       | ADDR  | Writes val -> address             |

## VM BYTECODE

REGISTER TABLE

| LOCATION | Register Name | Use                                    |
|---------:|---------------|----------------------------------------|
|   int_00 | INST_PTR      | PTR TO NEXT INSTRUCTION TO BE EXECUTED |
|   int_01 | FLAGS_0       | SEE FLAGS_0 USAGE TABLE                |    
|   int_02 | RESERVED      | RESERVED                               |
|   int_03 | RESERVED      | RESERVED                               |
|   int_04 | GP_I32_0      | GENERAL USE                            |
|   int_05 | GP_I32_1      | GENERAL USE                            |
|   int_06 | GP_I32_2      | GENERAL USE                            |
|   int_07 | GP_I32_3      | GENERAL USE                            |
|   int_08 |               |                                        |

| LOCATION | Register Name | Use         |
|---------:|---------------|-------------|
|   str_00 | GP_STR_0      | GENERAL USE |
|   str_01 | GP_STR_1      | GENERAL USE |
|   str_02 | GP_STR_2      | GENERAL USE |
|   str_03 | GP_STR_3      | GENERAL USE |

```asm
SETREG GP_STR_0 "Hello"
CCAT GP_STR_0 " "               ; GP_STR_0 becomes "Hello "
SETREG GP_STR_1 "World"
CCAT GP_STR_0 GP_STR_1          ; GP_STR_0 becomes "Hello World"
```

```asm
SETREG GP_STR_1 "Split me!"
SETREG GP_STR_0 " "             ; S_SUBS uses GP_STR_0 as split char
S_SUBS GP_STR_1 0               ; GP_STR_1 becomes "Split"
```

<ARG0 {USER,FUNC,DATA}>

```asm
LBL MAIN
    SETREG GP_STR_0 " "
    MEM_READ 0x00 GP_STR_1      ; READ STRING FROM STR_MEM_0 into string register 1
    S_SUBS GP_STR_1 0           ; GP_STR_1 becomes the first CLI argument
    S_COMP GP_STR_1 "USER"      ; Compare the first arg to the string user
    JMP_EQ PARSE_USER           ; if they where the same, 
    S_COMP GP_STR_1 "FUNC"      ; Compare the first arg to the string func
    JMP_EQ PARSE_FUNC           ; they where equle, jump to parse func
    S_COMP GP_STR_1 "DATA"      ; Compare the first arg to the string data
    JMP_EQ PARSE_DATA           ; If they where equle, jump to parse data
    PUSH "INVALID ARG!"         ; push data onto the value stack
    JMP ERROR_FUNC              ; HALT with error
    
LBL PARSE_DONE
    HALT

LBL PARSE_USER              ; First arg was USER
    SETREG GP_STR_0 " "
    MEM_READ 0x00 GP_STR_1      ; READ STRING FROM STR_MEM_0 into string register 1
    JMP PARSE_DONE

LBL PARSE_FUNC              ; First arg was FUNC
    JMP PARSE_DONE

LBL PARSE_DATA              ; First arg was DATA
    JMP PARSE_DONE
    
; stack 0 ( str error )    
LBL ERROR_FUNC
    POP GP_STR_1            ; takes the error arg and puts it in str 0
    HALT
    
```

FLAGS_0 USAGE TABLE

| BIT | NAME | USE                 |
|----:|------|---------------------|
|  00 | HALT | TRUE AFTER HALT RAN |
|  01 | EQ   | = A == B            |
|  02 | NEQ  | = A != B            |
|  03 | GTR  | = A >  B            |
|  04 | GEQ  | = A >= B            |
|  05 | LES  | = A <  B            |
|  06 | LEQ  | = A <= B            |

| OPPCODE | NAME                | ARG 0                 | ARG 1                 | NOTES                                 | IMPLED |
|--------:|---------------------|-----------------------|-----------------------|---------------------------------------|--------|
|      00 | NOOP                |                       |                       |                                       | x      |
|      01 | SETREG_INTR_INTC    | [DST] REGISTER        | [SRC] CONST INT VALUE | SETS REGISTER TO CONST INT            | x      |
|      02 | SETREG_INTR_INTR    | [DST] REGISTER 1      | [SRC] REGISTER  0     | MOVES VAL IN R0 INTO R1               | x      |
|      03 | ADD_INTR_INTC       | [DST] REGISTER        | [SRC] CONST INT VALUE | ADDS CONST TO VALUE REG STO IN REG    | x      |
|      04 | ADD_INTR_INTR       | [DST] REGISTER 1      | [SRC] REGISTER 0      | ADDS R0 TO R1 AND STORE IN R1         | x      |
|      05 | MUL_INTR_INTC       | [DST] REGISTER        | [SRC] CONST INT VALUE | MULT CONST TO VALUE REG STO IN REG    | x      |
|      06 | MUL_INTR_INTR       | [DST] REGISTER 1      | [SRC] REGISTER 0      | MULT R0 TO R1 AND STORE IN R1         | x      |
|      07 | SUB_INTR_INTC       | [DST] REGISTER        | [SRC] CONST INT VALUE | SUBS CONST FROM VALUE REG STO IN REG  | x      |
|      08 | SUB_INTR_INTR       | [DST] REGISTER 1      | [SRC] REGISTER 0      | SUBS R0 FROM R1 AND STORE IN R1       | x      |
|      09 | DIV_INTR_INTC       | [DST] REGISTER        | [SRC] CONST INT VALUE | DIVS CONST BY VALUE REG STO IN REG    | x      |
|      0A | DIV_INTR_INTR       | [DST] REGISTER 1      | [SRC] REGISTER 0      | DIVS R0 BY R1 AND STORE IN R1         | x      |
|      0B | INC                 | [DST] REGISTER        |                       | Inc register by 1                     | x      |
|      0C | DEC                 | [DST] REGISTER        |                       | Dec register by 1                     | x      |
|      0D | CMP_INTR_INTC       | [DST] REGISTER A      | [SRC] CONST    B      | A>B     A<B    A==B                   | x      |
|      0E | CMP_INTR_INTR       | [DST] REGISTER A      | [SRC] REGISTER B      | A>B     A<B    A==B                   | x      |
|      A0 | PUSH_INTC           | [SRC] CONST INT VALUE |                       | pushes the value into stack           | x      |
|      A0 | PUSH_INTC           | [SRC] CONST INT VALUE |                       | pushes the value into stack           | x      |
|      A1 | PUSH_INTR           | [SRC] REGISTER        |                       | pushes the value into stack           | x      |
|      A2 | POP_INTR            | [DST] REGISTER        |                       | pops the current value into register  | x      |
|      A3 | CALL                | [SRC] CONST INT VALUE |                       | calls the CONST INTs offset in memory | x      |
|      A4 | CALL                | [SRC] ADDR IN REG     |                       | calls the CONST INTs offset in memory | x      |
|      A5 | RET                 |                       |                       |                                       | x      |
|      A6 | CALL_D              |                       |                       | call must land at the call dest for   | x      |
|         |                     |                       |                       | error checking like end branch        |        |
|         |                     |                       |                       | othewise noop                         |        |
|      A7 | JMP_INTC_EQ         | [SRC] CONST INT VALUE |                       | JUMPS TO SRC WHEN COND                | x      |
|      A8 | JMP_INTC_NEQ        | [SRC] CONST INT VALUE |                       | JUMPS TO SRC WHEN COND                | x      |
|      A9 | JMP_INTC_GTR        | [SRC] CONST INT VALUE |                       | JUMPS TO SRC WHEN COND                | x      |
|      AA | JMP_INTC_GEQ        | [SRC] CONST INT VALUE |                       | JUMPS TO SRC WHEN COND                | x      |
|      AB | JMP_INTC_LES        | [SRC] CONST INT VALUE |                       | JUMPS TO SRC WHEN COND                | x      |
|      AC | JMP_INTC_LEQ        | [SRC] CONST INT VALUE |                       | JUMPS TO SRC WHEN COND                | x      |
|      AD | JMP_INTR_EQ         | [SRC] REGISTER        |                       | JUMPS TO SRC WHEN COND                | x      |
|      AE | JMP_INTR_NEQ        | [SRC] REGISTER        |                       | JUMPS TO SRC WHEN COND                | x      |
|      AF | JMP_INTR_GTR        | [SRC] REGISTER        |                       | JUMPS TO SRC WHEN COND                | x      |
|      B0 | JMP_INTR_GEQ        | [SRC] REGISTER        |                       | JUMPS TO SRC WHEN COND                | x      |
|      B1 | JMP_INTR_LES        | [SRC] REGISTER        |                       | JUMPS TO SRC WHEN COND                | x      |
|      B2 | JMP_INTR_LEQ        | [SRC] REGISTER        |                       | JUMPS TO SRC WHEN COND                | x      |
|      B3 | JMP_INTR            | [SRC] REGISTER        |                       |                                       | x      |
|      B4 | JMP_INTC            | [SRC] CONST INT VALUE |                       |                                       | x      |
|      B5 | MEM_READ_INTR_INTC  | [DST] REGISTER        | [SRC] CONST INT       | READS FROM const -> register          | x      |
|      B6 | MEM_READ_INTR_INTR  | [DST] REGISTER        | [SRC] REGISTER        | READS FROM mem@regval -> register     | x      |
|      B7 | MEM_WRITE_INTR_INTC | [SRC] REGISTER [val]  | [DST] CONST INT [adr] | WRITES FROM register -> memAddress    | x      |
|      B8 | MEM_WRITE_INTR_INTR | [SRC] REGISTER        | [DST] REGISTER        | WRITES FROM register -> mem@regval    | x      |
|         |                     |                       |                       |                                       |        |
|      FF | HALT                |                       |                       |                                       | x      |

> a string const must end with null, ascii

| NEW OPCODE          | ARG 0                   | ARG 1            | NOTES                          | REGISTER 0      |
|---------------------|-------------------------|------------------|--------------------------------|-----------------|
| SETREG_STRR_STRC    | [DEST] STR REG          | SCR STR CONST    |                                |                 |
| SETREG_STRR_STRR    | [DEST] STR REG          | SRC STR REGISTER |                                |                 |
| CCAT_STRR_STRR      | [DEST] STR REG          | SRC STR REGISTER | DEST_REG = DEST_REG + SRC_REG  |                 |
| CCAT_STRR_STRC      | [DEST] STR REG          | SRC STR CONST    | DEST_REG = DEST_REG + SRC_STR  |                 |
| CMP_STRR_STRR       | [SRC] STR REG A         | SRC STR REG B    | sets FLAGS like CMP_INTR_INTR  |                 |
| CMP_STRR_STRC       | [SRC] STR REG A         | SRC STR CONST B  | sets flags like CMP_INTR_INTC  |                 |
| SUBSTR_STRR_INTC    | [DEST/SRC] STR TO SPLIT | CONST INT INDEX  | splits ARG0, saves res no ARG1 | str to split on |
| SUBSTR_STRR_INTR    | [DEST/SRC] STR TO SPLIT | CONST INT INDEX  | splits ARG0, saves res no ARG1 | str to split on |
| MEM_READ_STRR_INTC  | [DEST] DST TO READ TO   | CONST INT ADDR   | Reads str memory at index ARG1 |                 |
| MEM_READ_STRR_INTR  | [DEST] DST TO READ TO   | INT REG ADDR     | Reads str memory at index ARG1 |                 |
| MEM_WRITE_STRR_INTC | [SRC] SRC TO WRITE TO   | CONST INT ADDR   | Writes str from ARG0 into mem  |                 |
| MEM_WRITE_STRR_INTR | [SRC] SRC TO WRITE TO   | INT REG ADDR     | Writes str from ARG0 into mem  |                 |
| PUSH_STRR           | [SRC] VAL SRC           |                  |                                |                 |
| PUSH_STRC           | [SRC] VAL SRC           |                  |                                |                 |


### SAMPLE ASM

# FILESYSTEM

| EXTENTION | FULL NAME          | USE               |
|-----------|--------------------|-------------------|
| .HEC      | Haileys ExeCutable | Stores executable |

## EXECUTABLE FILE FORMAT (.HEC)

If i just put the code at the end of the metadata, i can never grow the metadata structure

|   | address       | data                    | use                                                         |
|---|---------------|-------------------------|-------------------------------------------------------------|
| F | 0x00          | FA DD ED D0 67 00 00 00 | MAGIC NUMBER                                  (FADDEDD0'G') |
| F | 0x08          | XX XX XX XX             | CODE SECTION END OFFSET FROM END 'CODE_END_OFFSET' uint32   |              
| F | 0x0B          |                         |                                                             |              
| E | CODE_E_OFFSET |                         |                                                             |
|   |               | TinyCpu Byte Code       | CODE FOR THE VM TO RUN                                      |
|   | END OF FILE   |                         |                                                             |

- f = file offset or offset from the file start
- e = end offset or offset from the end of the file

```
0xFA 0xDD 0xED 0xD0 0x67 0x00 0x00 0x00
0x0B 0x00 0x00 0x00                             //CODE_END_OFFSET 12
//CODE SECTION START 12
/*00:*/ 0x01, 0x05, 0x01, 0x00, 0x00, 0x00, // [SETREG_INTR_C] SETREG GP_I32_1 1
/*06:*/ 0xB7, 0x05, 0x01, 0x00, 0x00, 0x00, // [MEM_WRITE_INTR_C] MEM_WRITE GP_I32_1 1
/*0c:*/ 0xB8, 0x05, 0x04, // [MEM_WRITE_INTR_R] MEM_WRITE GP_I32_1 GP_I32_0
/*0f:*/ 0xB5, 0x05, 0x01, 0x00, 0x00, 0x00, // [MEM_READ_INTR_C] MEM_READ GP_I32_1 1
/*15:*/ 0xFF, // [HALT] HALT  
END OF FILE
```
