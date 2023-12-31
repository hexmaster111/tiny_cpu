// See https://aka.ms/new-console-template for more information

using System.Diagnostics;


internal class Program
{
    public static void Main(string[] args)
    {
        // CAsmAssembler.CAsmAssemblerMain(args);
        CAsmAssembler.AssembleFromFileLines(
            ("""
             LBL MAIN
                 SETREG GP_STR_0 "Some thing"
                 ;MEM_READ 0x00 GP_STR_1      ; READ STRING FROM STR_MEM_0 into string register 1
                 SETREG GP_STR_1 "USER HAILEY SAYS HELLO WORLD!"
                 SSPLIT GP_STR_1 4           ; GP_STR_1 becomes the first CLI argument
                 CMP GP_STR_1 "USER"         ; Compare the first arg to the string user
                 JMP_EQ PARSE_USER           ; if they where the same, 
                 CMP GP_STR_1 "FUNC"      ; Compare the first arg to the string func
                 JMP_EQ PARSE_FUNC           ; they where equle, jump to parse func
                 CMP GP_STR_1 "DATA"      ; Compare the first arg to the string data
                 JMP_EQ PARSE_DATA           ; If they where equle, jump to parse data
                 PUSH "INVALID ARG!"         ; push data onto the value stack
                 JMP ERROR_FUNC              ; HALT with error
                 
             LBL PARSE_DONE
                 HALT
             
             LBL PARSE_USER              ; First arg was USER
                 SETREG GP_STR_0 " "
                 MEM_READ GP_STR_1 0x00      ; READ STRING FROM STR_MEM_0 into string register 1
                 JMP PARSE_DONE
             
             LBL PARSE_FUNC              ; First arg was FUNC
                 JMP PARSE_DONE
             
             LBL PARSE_DATA              ; First arg was DATA
                 JMP PARSE_DONE
                 
             ; stack 0 ( str error )    
             LBL ERROR_FUNC
                 POP GP_STR_1            ; takes the error arg and puts it in str 0
                 HALT
             """)
            .Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
        );
        
        
    }
}