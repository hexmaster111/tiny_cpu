
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