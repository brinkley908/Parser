using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Models
{
    public enum enCommandType
    {
        None,
        Import,
        Arg,
        Var,
        Int,
        Bool,
        Decimal,
        DateTime,
        String,
        Json,
        If,
        Else,
        For,
        ForEach,
        In,
        Do,
        While,
        Switch,
        Return,
        EOL,
        True,
        False,
        Finished,
        End
    }
}
