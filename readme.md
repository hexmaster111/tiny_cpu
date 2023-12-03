# Tiny Cpu!~

It has registers!
It has instructions!
It has bytecodes!
It has Assembly!

## Tiny Debugger!

```
┌───────────────────────────┬────────────────────────────────────────────────────┐
│ Registers                 │ De-comp                                            │
├───────────────────────────┼────────────────────────────────────────────────────┤
│ ┌────────────┬──────────┐ │ ┌────┬────────┬───────────────┬──────────┬───────┐ │
│ │ Register   │ Value    │ │ │ PC │ ADDR   │ OPCODE        │ ARG 0    │ ARG 1 │ │
│ ├────────────┼──────────┤ │ ├────┼────────┼───────────────┼──────────┼───────┤ │
│ │ INST_PTR   │ 0000000C │ │ │    │ 0000:  │ SETREG_R_C    │ GP_I32_1 │ 255   │ │
│ │ FLAGS_0    │ 00000000 │ │ │    │ 0006:  │ MEM_WRITE_R_C │ GP_I32_1 │ 0     │ │
│ │ RESERVED_0 │ 00000000 │ │ │ >  │ 000C:  │ HALT          │          │       │ │
│ │ RESERVED_1 │ 00000000 │ │ └────┴────────┴───────────────┴──────────┴───────┘ │
│ │ GP_I32_0   │ 00000000 │ │                                                    │
│ │ GP_I32_1   │ 000000FF │ │                                                    │
│ │ GP_I32_2   │ 00000000 │ │                                                    │
│ └────────────┴──────────┘ │                                                    │
└───────────────────────────┴────────────────────────────────────────────────────┘
```

## Tiny Compiller!

```
┌─────────────────────────────┐  ┌─────────────────────────┐  ┌──────┬────────────┬──────────────┐  ├── Program Syntax Translation
│ int globalVarA;             │  │ SETREG GP_I32_0 0x00    │  │ char │ Code       │ Words        │  │   └── .:: Everything in the program
│ fn main():void {            │  │ MEM_WRITE GP_I32_0 0x00 │  ├──────┼────────────┼──────────────┤  │       ├── .::VAR DEF | globalVarA : int = (nothing)
│     globalVarA = 42;        │  │ LBL .::main             │  │ 3    │ int        │ Type         │  │       ├── .::fn main (void ) => void
│     int other = 420;        │  │ SETREG GP_I32_0 0x2A    │  │ 14   │ globalVarA │ VarName      │  │       │   ├── fn_main::VAR ASSIGNMENT | globalVarA = Const Expr (42)
│     int lv_demo=globalVarA; │  │ MEM_WRITE GP_I32_0 0x00 │  │ 15   │ ;          │ EndLine      │  │       │   ├── fn_main::VAR DEF | other : int = Const Expr (420)
│     other_func();           │  │ SETREG GP_I32_0 0x1A4   │  │ 19   │ fn         │ Function     │  │       │   ├── fn_main::VAR DEF | lv_demo : int = VarExp (globalVarA)
│ }                           │  │ MEM_WRITE GP_I32_0 0x01 │  │ 24   │ main       │ VarName      │  │       │   └── fn_main::other_func (void)
│ fn other_func ( ) : void {  │  │ MEM_READ GP_I32_0 0x00  │  │ 25   │ (          │ OpenParen    │  │       └── .::fn other_func (void ) => void
│     int x=20;               │  │ MEM_WRITE GP_I32_0 0x02 │  │ 26   │ )          │ CloseParen   │  │           ├── fn_other_func::VAR DEF | x : int = Const Expr (20)
│     int y;                  │  │ CALL .::other_func      │  │ 27   │ :          │ OfType       │  │           └── fn_other_func::VAR DEF | y : int = (nothing)
│ }                           │  │ RET                     │  │ 31   │ void       │ Type         │  ├── Var Table
└─────────────────────────────┘  │ SETREG GP_I32_0 0x2A    │  │ 33   │ {          │ OpenBracket  │  │   └── ┌──────────────────┬──────────┐
                                 │ MEM_WRITE GP_I32_0 0x00 │  │ 49   │ globalVarA │ VarName      │  │       │ Fullname         │ Var Slot │
                                 │ SETREG GP_I32_0 0x1A4   │  │ 51   │ =          │ Assignment   │  │       ├──────────────────┼──────────┤
                                 │ MEM_WRITE GP_I32_0 0x01 │  │ 54   │ 42         │ TypedValue   │  │       │ .::globalVarA    │ 000      │
                                 │ MEM_READ GP_I32_0 0x00  │  │ 55   │ ;          │ EndLine      │  │       │ fn_main::other   │ 001      │
                                 │ MEM_WRITE GP_I32_0 0x02 │  │ 64   │ int        │ Type         │  │       │ fn_main::lv_demo │ 002      │
                                 │ CALL .::other_func      │  │ 70   │ other      │ VarName      │  │       │ fn_other_func::x │ 003      │
                                 │ LBL .::other_func       │  │ 72   │ =          │ Assignment   │  │       │ fn_other_func::y │ 004      │
                                 │ SETREG GP_I32_0 0x14    │  │ 76   │ 420        │ TypedValue   │  │       └──────────────────┴──────────┘
                                 │ MEM_WRITE GP_I32_0 0x03 │  │ 77   │ ;          │ EndLine      │  ├── Function Table
                                 │ SETREG GP_I32_0 0x00    │  │ 86   │ int        │ Type         │  │   └── ┌───────────┬────────────┐
                                 │ MEM_WRITE GP_I32_0 0x04 │  │ 94   │ lv_demo    │ VarName      │  │       │ NAMESPACE │ FN NAME    │
                                 │ RET                     │  │ 95   │ =          │ Assignment   │  │       ├───────────┼────────────┤
                                 │ SETREG GP_I32_0 0x14    │  │ 105  │ globalVarA │ VarName      │  │       │ .         │ main       │
                                 │ MEM_WRITE GP_I32_0 0x03 │  │ 106  │ ;          │ EndLine      │  │       │ .         │ other_func │
                                 │ SETREG GP_I32_0 0x00    │  │ 122  │ other_func │ VarName      │  │       └───────────┴────────────┘
                                 │ MEM_WRITE GP_I32_0 0x04 │  │ 123  │ (          │ OpenParen    │  └── Statement List
                                 │                         │  │ 124  │ )          │ CloseParen   │      └── ASM
                                 └─────────────────────────┘  │ 125  │ ;          │ EndLine      │          └── TokenInfo
                                                              │ 128  │ }          │ CloseBracket │              ├──  Everything in the program
                                                              │ 132  │ fn         │ Function     │              │   └── STATEMENTS:
                                                              │ 143  │ other_func │ VarName      │              ├── VAR DEF | globalVarA : int = (nothing)
                                                              │ 145  │ (          │ OpenParen    │              │   └── STATEMENTS:
                                                              │ 147  │ )          │ CloseParen   │              │       ├── SETREG GP_I32_0 0x00
                                                              │ 149  │ :          │ OfType       │              │       └── MEM_WRITE GP_I32_0 0x00
                                                              │ 154  │ void       │ Type         │              ├── fn main (void ) => void
                                                              │ 156  │ {          │ OpenBracket  │              │   └── STATEMENTS:
                                                              │ 165  │ int        │ Type         │              │       ├── LBL .::main
                                                              │ 167  │ x          │ VarName      │              │       ├── SETREG GP_I32_0 0x2A
                                                              │ 168  │ =          │ Assignment   │              │       ├── MEM_WRITE GP_I32_0 0x00
                                                              │ 170  │ 20         │ TypedValue   │              │       ├── SETREG GP_I32_0 0x1A4
                                                              │ 171  │ ;          │ EndLine      │              │       ├── MEM_WRITE GP_I32_0 0x01
                                                              │ 180  │ int        │ Type         │              │       ├── MEM_READ GP_I32_0 0x00
                                                              │ 182  │ y          │ VarName      │              │       ├── MEM_WRITE GP_I32_0 0x02
                                                              │ 183  │ ;          │ EndLine      │              │       ├── CALL .::other_func
                                                              │ 186  │ }          │ CloseBracket │              │       └── RET
                                                              └──────┴────────────┴──────────────┘              ├── VAR ASSIGNMENT | globalVarA = Const Expr (42)
                                                                                                                │   └── STATEMENTS:
                                                                                                                │       ├── SETREG GP_I32_0 0x2A
                                                                                                                │       └── MEM_WRITE GP_I32_0 0x00
                                                                                                                ├── VAR DEF | other : int = Const Expr (420)
                                                                                                                │   └── STATEMENTS:
                                                                                                                │       ├── SETREG GP_I32_0 0x1A4
                                                                                                                │       └── MEM_WRITE GP_I32_0 0x01
                                                                                                                ├── VAR DEF | lv_demo : int = VarExp (globalVarA)
                                                                                                                │   └── STATEMENTS:
                                                                                                                │       ├── MEM_READ GP_I32_0 0x00
                                                                                                                │       └── MEM_WRITE GP_I32_0 0x02
                                                                                                                ├── other_func (void)
                                                                                                                │   └── STATEMENTS:
                                                                                                                │       └── CALL .::other_func
                                                                                                                ├── fn other_func (void ) => void
                                                                                                                │   └── STATEMENTS:
                                                                                                                │       ├── LBL .::other_func
                                                                                                                │       ├── SETREG GP_I32_0 0x14
                                                                                                                │       ├── MEM_WRITE GP_I32_0 0x03
                                                                                                                │       ├── SETREG GP_I32_0 0x00
                                                                                                                │       ├── MEM_WRITE GP_I32_0 0x04
                                                                                                                │       └── RET
                                                                                                                ├── VAR DEF | x : int = Const Expr (20)
                                                                                                                │   └── STATEMENTS:
                                                                                                                │       ├── SETREG GP_I32_0 0x14
                                                                                                                │       └── MEM_WRITE GP_I32_0 0x03
                                                                                                                └── VAR DEF | y : int = (nothing)
                                                                                                                    └── STATEMENTS:
                                                                                                                        ├── SETREG GP_I32_0 0x00
                                                                                                                        └── MEM_WRITE GP_I32_0 0x04
Compiled Ok!
Verifying Assembly
........................................................................................................................................................Assembly ok
/*00:*/ 0xA3, 0x12, 0x00, 0x00, 0x00, // [CALL_C] CALL .::main
/*05:*/ 0xFF, // [HALT] HALT
/*06:*/ 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x00
/*0c:*/ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x00
/*12:*/ 0xA6, // [CALL_D] LBL .::main
/*13:*/ 0x01, 0x04, 0x2A, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x2A
/*19:*/ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x00
/*1f:*/ 0x01, 0x04, 0xA4, 0x01, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x1A4
/*25:*/ 0xB7, 0x04, 0x01, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x01
/*2b:*/ 0xB5, 0x04, 0x00, 0x00, 0x00, 0x00, // [MEM_READ_R_C] MEM_READ GP_I32_0 0x00
/*31:*/ 0xB7, 0x04, 0x02, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x02
/*37:*/ 0xA3, 0x66, 0x00, 0x00, 0x00, // [CALL_C] CALL .::other_func
/*3c:*/ 0xA5, // [RET] RET
/*3d:*/ 0x01, 0x04, 0x2A, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x2A
/*43:*/ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x00
/*49:*/ 0x01, 0x04, 0xA4, 0x01, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x1A4
/*4f:*/ 0xB7, 0x04, 0x01, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x01
/*55:*/ 0xB5, 0x04, 0x00, 0x00, 0x00, 0x00, // [MEM_READ_R_C] MEM_READ GP_I32_0 0x00
/*5b:*/ 0xB7, 0x04, 0x02, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x02
/*61:*/ 0xA3, 0x66, 0x00, 0x00, 0x00, // [CALL_C] CALL .::other_func
/*66:*/ 0xA6, // [CALL_D] LBL .::other_func
/*67:*/ 0x01, 0x04, 0x14, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x14
/*6d:*/ 0xB7, 0x04, 0x03, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x03
/*73:*/ 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x00
/*79:*/ 0xB7, 0x04, 0x04, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x04
/*7f:*/ 0xA5, // [RET] RET
/*80:*/ 0x01, 0x04, 0x14, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x14
/*86:*/ 0xB7, 0x04, 0x03, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x03
/*8c:*/ 0x01, 0x04, 0x00, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 0x00
/*92:*/ 0xB7, 0x04, 0x04, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0x04
Assembled Ok!
```

# CuteC Debugger! 

```
┌─────────────────────────────────────────┬────────────────────────────┐
│ VAR TBL                                 │ FUNC TBL                   │
├─────────────────────────────────────────┼────────────────────────────┤
│ ┌──────────┬──────────────────┬───────┐ │ ┌───────────┬────────────┐ │
│ │ Var Slot │ Fullname         │ Value │ │ │ NAMESPACE │ FN NAME    │ │
│ ├──────────┼──────────────────┼───────┤ │ ├───────────┼────────────┤ │
│ │ 000      │ .::globalVarA    │ 0     │ │ │ .         │ main       │ │
│ │ 001      │ fn_main::other   │ 0     │ │ │ .         │ other_func │ │
│ │ 002      │ fn_main::lv_demo │ 0     │ │ └───────────┴────────────┘ │
│ │ 003      │ fn_other_func::x │ 0     │ │                            │
│ │ 004      │ fn_other_func::y │ 0     │ │                            │
│ └──────────┴──────────────────┴───────┘ │                            │
└─────────────────────────────────────────┴────────────────────────────┘
┌─────────────────────────────────┬────────────────────────────────────────────────────────────────────────────────────────────────────────┐
│ REGISTERS                       │ PROGRAM                                                                                                │
├─────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────────────────┤
│ ┌────────────┬─────┬──────────┐ │ ┌───┬─────┬──────────────────────────────────────┬───────────────┬───────────┬───────────────┬───────┐ │
│ │ REG        │ VAL │ HEX      │ │ │ I │ BS  │ INST BYTES                           │ OP            │ ASM       │ ARG 0         │ ARG 1 │ │
│ ├────────────┼─────┼──────────┤ │ ├───┼─────┼──────────────────────────────────────┼───────────────┼───────────┼───────────────┼───────┤ │
│ │ INST_PTR   │ 0   │ 00000000 │ │ │ > │ 00: │ 0xA3, 0x12, 0x00, 0x00, 0x00,        │ CALL_C        │ CALL      │ .::main       │       │ │
│ │ FLAGS_0    │ 0   │ 00000000 │ │ │   │ 05: │ 0xFF,                                │ HALT          │ HALT      │               │       │ │
│ │ RESERVED_0 │ 0   │ 00000000 │ │ │   │ 06: │ 0x01, 0x04, 0x00, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x00  │ │
│ │ RESERVED_1 │ 0   │ 00000000 │ │ │   │ 0c: │ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x00  │ │
│ │ GP_I32_0   │ 0   │ 00000000 │ │ │   │ 12: │ 0xA6,                                │ CALL_D        │ LBL       │ .::main       │       │ │
│ │ GP_I32_1   │ 0   │ 00000000 │ │ │   │ 13: │ 0x01, 0x04, 0x2A, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x2A  │ │
│ │ GP_I32_2   │ 0   │ 00000000 │ │ │   │ 19: │ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x00  │ │
│ └────────────┴─────┴──────────┘ │ │   │ 1f: │ 0x01, 0x04, 0xA4, 0x01, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x1A4 │ │
│                                 │ │   │ 25: │ 0xB7, 0x04, 0x01, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x01  │ │
│                                 │ │   │ 2b: │ 0xB5, 0x04, 0x00, 0x00, 0x00, 0x00,  │ MEM_READ_R_C  │ MEM_READ  │ GP_I32_0      │ 0x00  │ │
│                                 │ │   │ 31: │ 0xB7, 0x04, 0x02, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x02  │ │
│                                 │ │   │ 37: │ 0xA3, 0x66, 0x00, 0x00, 0x00,        │ CALL_C        │ CALL      │ .::other_func │       │ │
│                                 │ │   │ 3c: │ 0xA5,                                │ RET           │ RET       │               │       │ │
│                                 │ │   │ 3d: │ 0x01, 0x04, 0x2A, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x2A  │ │
│                                 │ │   │ 43: │ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x00  │ │
│                                 │ │   │ 49: │ 0x01, 0x04, 0xA4, 0x01, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x1A4 │ │
│                                 │ │   │ 4f: │ 0xB7, 0x04, 0x01, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x01  │ │
│                                 │ │   │ 55: │ 0xB5, 0x04, 0x00, 0x00, 0x00, 0x00,  │ MEM_READ_R_C  │ MEM_READ  │ GP_I32_0      │ 0x00  │ │
│                                 │ │   │ 5b: │ 0xB7, 0x04, 0x02, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x02  │ │
│                                 │ │   │ 61: │ 0xA3, 0x66, 0x00, 0x00, 0x00,        │ CALL_C        │ CALL      │ .::other_func │       │ │
│                                 │ │   │ 66: │ 0xA6,                                │ CALL_D        │ LBL       │ .::other_func │       │ │
│                                 │ │   │ 67: │ 0x01, 0x04, 0x14, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x14  │ │
│                                 │ │   │ 6d: │ 0xB7, 0x04, 0x03, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x03  │ │
│                                 │ │   │ 73: │ 0x01, 0x04, 0x00, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x00  │ │
│                                 │ │   │ 79: │ 0xB7, 0x04, 0x04, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x04  │ │
│                                 │ │   │ 7f: │ 0xA5,                                │ RET           │ RET       │               │       │ │
│                                 │ │   │ 80: │ 0x01, 0x04, 0x14, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x14  │ │
│                                 │ │   │ 86: │ 0xB7, 0x04, 0x03, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x03  │ │
│                                 │ │   │ 8c: │ 0x01, 0x04, 0x00, 0x00, 0x00, 0x00,  │ SETREG_R_C    │ SETREG    │ GP_I32_0      │ 0x00  │ │
│                                 │ │   │ 92: │ 0xB7, 0x04, 0x04, 0x00, 0x00, 0x00,  │ MEM_WRITE_R_C │ MEM_WRITE │ GP_I32_0      │ 0x04  │ │
│                                 │ └───┴─────┴──────────────────────────────────────┴───────────────┴───────────┴───────────────┴───────┘ │
└─────────────────────────────────┴────────────────────────────────────────────────────────────────────────────────────────────────────────┘
STEP CONTINUE
```