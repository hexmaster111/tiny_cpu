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
| MEM_READ    | DEST      | ADDR  | Reads from mem addres -> dest reg |
| MEM_WRITE   | VAL       | ADDR  | Writes val -> address             |

```
; This program tries to read from mem address 0x00 for the value 0x42, if the value is not in mem, retry max of 5 times
LBL RETRY_READ
MEM_READ GP_I32_0 0x0 ;Read from virtual ram address 0 and store it in GP_Reg 0
CMP GP_I32_0 0x42     ;compare the value from mem[0] to 0x42
JMP_EQ VALUE_REACHED
INC GP_I32_1          ; We didnt rech the value inc try counter
CMP GP_I32_1 0x5      ;Compare how many times we have retryed to 5
JMP_GEQ FAIL_TO_GET_VALUE
JMP RETRY_READ

LBL VALUE_REACHED 
SETREG GP_I32_2 0x1
HALT

LBL FAIL_TO_GET_VALUE
SETREG GP_I32_2 0xFA
HALT
```

## VM BYTECODE

REGISTER TABLE

| LOCATION | Register Name | Use                                    |
|---------:|---------------|----------------------------------------|
|       00 | INST_PTR      | PTR TO NEXT INSTRUCTION TO BE EXECUTED |
|       01 | FLAGS_0       | SEE FLAGS_0 USAGE TABLE                |
|       02 | RESERVED      | RESERVED                               |
|       03 | RESERVED      | RESERVED                               |
|       04 | GP_I32_0      | GENERAL USE                            |
|       05 | GP_I32_1      | GENERAL USE                            |
|       06 | GP_I32_2      | GENERAL USE                            |
|       07 | GP_I32_3      | GENERAL USE                            |
|       08 |               |                                        |

FLAGS_0 USAGE TABLE

| BIT | NAME | USE                 |
|----:|------|---------------------|
|  00 | HALT | TRUE AFTER HALT RAN |
|  01 | EQ   | = A==B              |
|  02 | NEQ  | = A!=B              |
|  03 | GTR  | = A>B               |
|  04 | GEQ  | = A>=B              |
|  05 | LES  | = A<B               |
|  06 | LEQ  | = A<=B              |

| OPPCODE | NAME          | ARG 0                | ARG 1                | NOTES                                | IMPLED |
|--------:|---------------|----------------------|----------------------|--------------------------------------|--------|
|      00 | NOOP          |                      |                      |                                      | x      |
|      01 | SETREG_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | SETS REGISTER TO CONSTANT            | x      |
|      02 | SETREG_R_R    | [DST] REGISTER 1     | [SRC] REGISTER  0    | MOVES VAL IN R0 INTO R1              | x      |
|      03 | ADD_R_C       | [DST] REGISTER       | [SRC] CONSTANT VALUE | ADDS CONST TO VALUE REG STO IN REG   | x      |
|      04 | ADD_R_R       | [DST] REGISTER 1     | [SRC] REGISTER 0     | ADDS R0 TO R1 AND STORE IN R1        | x      |
|      05 | MUL_R_C       | [DST] REGISTER       | [SRC] CONSTANT VALUE | MULT CONST TO VALUE REG STO IN REG   | x      |
|      06 | MUL_R_R       | [DST] REGISTER 1     | [SRC] REGISTER 0     | MULT R0 TO R1 AND STORE IN R1        | x      |
|      07 | SUB_R_C       | [DST] REGISTER       | [SRC] CONSTANT VALUE | SUBS CONST FROM VALUE REG STO IN REG | x      |
|      08 | SUB_R_R       | [DST] REGISTER 1     | [SRC] REGISTER 0     | SUBS R0 FROM R1 AND STORE IN R1      | x      |
|      09 | DIV_R_C       | [DST] REGISTER       | [SRC] CONSTANT VALUE | DIVS CONST BY VALUE REG STO IN REG   | x      |
|      0A | DIV_R_R       | [DST] REGISTER 1     | [SRC] REGISTER 0     | DIVS R0 BY R1 AND STORE IN R1        | x      |
|      0B | INC           | [DST] REGISTER       |                      | Inc register by 1                    | x      |
|      0C | DEC           | [DST] REGISTER       |                      | Dec register by 1                    | x      |
|      0D | CMP_R_C       | [DST] REGISTER A     | [SRC] CONST    B     | A>B     A<B    A==B                  | x      |
|      0E | CMP_R_R       | [DST] REGISTER A     | [SRC] REGISTER B     | A>B     A<B    A==B                  | x      |
|      0F |               |                      |                      | RESERVED                             |        |
|      A0 | PUSH C        | [SRC] CONSTANT VALUE |                      | pushes the value into stack          | x      |
|      A1 | PUSH R        | [SRC] REGISTER       |                      | pushes the value into stack          | x      |
|      A2 | POP R         | [DST] REGISTER       |                      | pops the current value into register | x      |
|      A3 | CALL          | [SRC] CONSTANT VALUE |                      | calls the constants offset in memory | x      |
|      A4 | CALL          | [SRC] ADDR IN REG    |                      | calls the constants offset in memory | x      |
|      A5 | RET           |                      |                      |                                      | x      |
|      A6 | CALL_D        |                      |                      | call must land at the call dest for  | x      |
|         |               |                      |                      | error checking like end branch       |        |
|         |               |                      |                      | othewise noop                        |        |
|      A7 | JMP_C_EQ      | [SRC] CONSTANT VALUE |                      | JUMPS TO SRC WHEN COND               | x      |
|      A8 | JMP_C_NEQ     | [SRC] CONSTANT VALUE |                      | JUMPS TO SRC WHEN COND               | x      |
|      A9 | JMP_C_GTR     | [SRC] CONSTANT VALUE |                      | JUMPS TO SRC WHEN COND               | x      |
|      AA | JMP_C_GEQ     | [SRC] CONSTANT VALUE |                      | JUMPS TO SRC WHEN COND               | x      |
|      AB | JMP_C_LES     | [SRC] CONSTANT VALUE |                      | JUMPS TO SRC WHEN COND               | x      |
|      AC | JMP_C_LEQ     | [SRC] CONSTANT VALUE |                      | JUMPS TO SRC WHEN COND               | x      |
|      AD | JMP_R_EQ      | [SRC] REGISTER       |                      | JUMPS TO SRC WHEN COND               | x      |
|      AE | JMP_R_NEQ     | [SRC] REGISTER       |                      | JUMPS TO SRC WHEN COND               | x      |
|      AF | JMP_R_GTR     | [SRC] REGISTER       |                      | JUMPS TO SRC WHEN COND               | x      |
|      B0 | JMP_R_GEQ     | [SRC] REGISTER       |                      | JUMPS TO SRC WHEN COND               | x      |
|      B1 | JMP_R_LES     | [SRC] REGISTER       |                      | JUMPS TO SRC WHEN COND               | x      |
|      B2 | JMP_R_LEQ     | [SRC] REGISTER       |                      | JUMPS TO SRC WHEN COND               | x      |
|      B3 | JMP_R         | [SRC] REGISTER       |                      |                                      | x      |
|      B4 | JMP_C         | [SRC] CONSTANT VALUE |                      |                                      | x      |
|      B5 | MEM_READ_R_C  | [DST] REGISTER       | [SRC] CONSTANT       | READS FROM const -> register         |        |
|      B6 | MEM_READ_R_R  | [DST] REGISTER       | [SRC] REGISTER       | READS FROM mem@regval -> register    |        |
|      B7 | MEM_WRITE_R_C | [DST] REGISTER [val] | [SRC] CONSTANT [adr] | WRITES FROM register -> memAddress   |        |
|      B8 | MEM_WRITE_R_R | [DST] REGISTER       | [SRC] REGISTER       | WRITES FROM register -> mem@regval   |        |

| FF | HALT | | | | x |

```asm

CALL CALL_DEMO
ADD GP_I32_0 0x04
SUB GP_I32_0 0x01
DIV GP_I32_0 0x01
MUL GP_I32_0 0x04
HALT           ;halt the cpu",
LBL CALL_DEMO
SETREG GP_I32_0 0x69
RET
```

```TCPU byte code
var prog = new byte[]{
/*00:*/ 0x01, 0x69, 0x00, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 69
/*06:*/ 0xA3, 0x24, 0x00, 0x00, 0x00, // [CALL_C] CALL CALL_DEMO 
/*0b:*/ 0x03, 0x04, 0x04, 0x00, 0x00, 0x00, // [ADD_R_C] ADD GP_I32_0 4
/*11:*/ 0x07, 0x04, 0x01, 0x00, 0x00, 0x00, // [SUB_R_C] SUB GP_I32_0 1
/*17:*/ 0x09, 0x04, 0x01, 0x00, 0x00, 0x00, // [DIV_R_C] DIV GP_I32_0 1
/*1d:*/ 0x05, 0x04, 0x04, 0x00, 0x00, 0x00, // [MUL_R_C] MUL GP_I32_0 4
/*23:*/ 0xFF, // [HALT] HALT  
/*24:*/ 0xA6, // [CALL_D] LBL CALL_DEMO 
/*25:*/ 0xA5, // [RET] RET  
};
```