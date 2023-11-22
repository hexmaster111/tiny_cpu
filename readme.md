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
