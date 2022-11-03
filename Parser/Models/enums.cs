using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Models
{
    
    public enum tok_type
    {
        NONE,
        DELIMETER,
        IDENTIFIER,
        NUMBER,
        KEYWORD,
        TEMP,
        STRING,
        BLOCK
    }

    public enum double_ops
    {
        LT=1, 
        LE, 
        GT, 
        GE, 
        EQ, 
        NE
    }

    public enum tokens
    {
        NONE,
        ARG,
        CHAR,
        INT,
        IF,
        ELSE,
        FOR,
        DO,
        WHILE,
        SWITCH,
        RETURN,
        EOL,
        FINISHED,
        END

    }


    public enum error_msg
    {
        SYNTAX = 0,

        UNBAL_PARENS,
        UNBAL_BRACES,

        SEMI_EXPECTED,
        PAREN_EXPECTED,
        EQUALS_EXPECTED,
        TYPE_EXPECTED,
        WHILE_EXPECTED,
        QUOTE_EXPECTED,

        NOT_TEMP,
        TOO_MANY_LVARS,
        NO_EXP,
        NOT_VAR,
        PARAM_ERR,
        FUNC_UNDEF,
        NEST_FUNC,
        RET_NOCALL,
    }

    public enum intern_func
    {
        NONE,
        CONSOLELOG,
        PRINTF

    }

}
