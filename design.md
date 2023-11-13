
REGISTER TABLE

| LOCATION | Register Name | Use                                    |
|---------:|---------------|----------------------------------------|
|       00 | INST_PTR      | PTR TO NEXT INSTRUCTION TO BE EXECUTED |
|       01 | FLAGS_0       | SEE FLAGS_0 USAGE TABLE                |
|       02 | RESERVED      | RESERVED                               |
|       03 | RESERVED      | RESERVED                               |
|       04 | GP_I32_0      | GENERAL SIGNED INT 32 REGISTER         |
|       05 | GP_I32_1      | GENERAL SIGNED INT 32 REGISTER         |
|       06 | GP_I32_2      | GENERAL SIGNED INT 32 REGISTER         |
|          |               |                                        |



FLAGS_0 USAGE TABLE

| BIT | NAME | USE                 |
|----:|------|---------------------|
|  00 | HALT | TRUE AFTER HALT RAN |



| OPPCODE | NAME       | ARG 0                | ARG 1                | NOTES                                | IMPLED |
|--------:|------------|----------------------|----------------------|--------------------------------------|--------|
|      00 | NOOP       |                      |                      |                                      | x      |
|      01 | SETREG_R_C | [DST] REGISTER       | [SRC] CONSTANT VALUE | SETS REGISTER TO CONSTANT            | x      |
|      02 | SETREG_R_R | [DST] REGISTER 1     | [SRC] REGISTER  0    | MOVES VAL IN R0 INTO R1              | x      |
|      03 | ADD_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | ADDS CONST TO VALUE REG STO IN REG   | x      |
|      04 | ADD_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | ADDS R0 TO R1 AND STORE IN R1        | x      |
|      05 | MUL_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | MULT CONST TO VALUE REG STO IN REG   | x      |
|      06 | MUL_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | MULT R0 TO R1 AND STORE IN R1        | x      |
|      07 | SUB_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | SUBS CONST FROM VALUE REG STO IN REG | x      |
|      08 | SUB_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | SUBS R0 FROM R1 AND STORE IN R1      | x      |
|      09 | DIV_R_C    | [DST] REGISTER       | [SRC] CONSTANT VALUE | DIVS CONST BY VALUE REG STO IN REG   | x      |
|      0A | DIV_R_R    | [DST] REGISTER 1     | [SRC] REGISTER 0     | DIVS R0 BY R1 AND STORE IN R1        | x      |
|      0B |            |                      |                      | RESERVED                             |
|      0C |            |                      |                      | RESERVED                             |
|      0D |            |                      |                      | RESERVED                             |
|      0E |            |                      |                      | RESERVED                             |
|      0F |            |                      |                      | RESERVED                             |
|      A0 | PUSH C     | [SRC] CONSTANT VALUE |                      | pushes the value into stack          |
|      A1 | PUSH R     | [SRC] REGISTER       |                      | pushes the value into stack          |
|      A2 | POP R      | [DST] REGISTER       |                      | pops the current value into register |
|      A3 | CALL       | [SRC] CONSTANT VALUE |                      | calls the constants offset in memory |
|      A4 | CALL       | [SRC] ADDR IN REG    |                      | calls the constants offset in memory |
|      A5 | RET        |                      |                      |                                      |
|      A6 | CALL_D     |                      |                      | call must land at the call dest for  |
|         |            |                      |                      | error checking like end branch       |
|         |            |                      |                      | othewise noop                        |        |
|      FF | HALT       |                      |                      |                                      |


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

```ASM
/*00*/ 0x01, 0x05, 0x01, 0x00, 0x00, 0x00, //SETREG_R_C GP_I_1 1
/*06*/ 0xA6,                               //CALL_TARGET
/*07*/ 0x00,                               //NOOP
/*08*/ 0x03, 0x05, 0x02, 0x00, 0x00, 0x00, //ADD_R_C GP_I_1 2
/*0E*/ 0xA3, 0x06, 0x00, 0x00, 0x00,       //CALL 0x06
/*13*/ 0xA5                                //HALT
```

