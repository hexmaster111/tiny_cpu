| LOCATION | Register Name | Use                                      |
| -------: | ------------- | ---------------------------------------- |
|       00 | INST_PTR      | PTR TO NEXT INSTRUCTION TO BE EXECUTED   |
|       01 | GP_I32_0      | GENERAL  SIGNED INT 32 REGISTER NUMBER 0 |


| OPPCODE | NAME       | ARG 0                | ARG 1                | NOTES                                | IMPLED |
| ------: | ---------- | -------------------- | -------------------- | ------------------------------------ | ------ |
|      00 | NOOP       |                      |                      |                                      | x      |
|      01 | SETREG_R_C | [DST] REGISTER       | [SRC] CONSTANT VALUE | SETS REGISTER TO CONSTANT            | x      |
|      02 | SETREG_R_R | [DST] REGISTER 1     | [SRC] REGISTER  0    | MOVES VAL IN R0 INTO R1              |
|      03 | ADD_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | ADDS CONST TO VALUE REG STO IN REG   |
|      04 | ADD_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | ADDS R0 TO R1 AND STORE IN R1        |
|      05 | MUL_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | MULT CONST TO VALUE REG STO IN REG   |
|      06 | MUL_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | MULT R0 TO R1 AND STORE IN R1        |
|      07 | SUB_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | SUBS CONST FROM VALUE REG STO IN REG |
|      08 | SUB_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | SUBS R0 FROM R1 AND STORE IN R1      |
|      09 | DIV_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | DIVS CONST BY VALUE REG STO IN REG   |
|      0A | DIV_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | DIVS R0 BY R1 AND STORE IN R1        |
|      0B |            |                      |                      | RESERVED                             |
|      0C |            |                      |                      | RESERVED                             |
|      0D |            |                      |                      | RESERVED                             |
|      0E |            |                      |                      | RESERVED                             |
|      0F |            |                      |                      | RESERVED                             |
|      A0 | PUSH C     | [SRC] CONSTANT VALUE |                      | pushes the value into stack          |
|      A1 | PUSH R     | [SRC] REGISTER       |                      | pushes the value into stack          |
|      A2 | POP R      | [DST] REGISTER       |                      | pops the current value into register |
|      A3 | CALL       | [SRC] CONSTANT VALUE |                      | calls                                |
|      A4 | RET        |                      |                      |                                      |


```ASM
0: 00            ; NOOP
1: 01 01 01      ; SETREG_R_C GP_I32_0 CONST_1
```