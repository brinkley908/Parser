using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Models
{
    public enum enSyntaxError
    {
        Syntax = 0,
        UnbalParens,
        NoExp,
        EqExpected,
        NotVar,
        ParamErr,
        SemiExpected,
        UnbalBraces,
        NoFunc,
        TypeExpected,
        NestedFuncs,
        NoRetCall,
        ParanExpected,
        WhileExpected,
        QuoteExpected,
        NoString,
        TooManyLVars,
        AndExpected,
        OrExpected,
        ImportInvalid,
        ImportNotFound,
        ImportEmpty,
        NotVarType,
        InvalidChar,
        InvalidDateTime,
        ArgumentExpected,
        ArgumentOutOfRange,
        ExtensionExpected,
        Null
    }
}
