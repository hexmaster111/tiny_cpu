// See https://aka.ms/new-console-template for more information

using Spectre.Console;

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
}
*/