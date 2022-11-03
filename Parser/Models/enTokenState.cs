using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Models
{
    public enum enTokenState
    {
        Start,
        Arg,
        Char,
        Int,
        If,
        Else,
        For,
        Do,
        While,
        Switch,
        Return,
        EOL,
        Finished,
        End
    }
}
