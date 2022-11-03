using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public class ParserException : Exception
    {
        public string Token { get; set; }
        public enCommandType CommandType { get; set; }
        public enTokenType TokenType { get; set; }
        public enTokenState TokenState { get; set; }
        public int LineNumber { get; set; } = 0;

        public char Tok { get; set; }

        public bool EOF { get; set; }

        public ParserException() { }
        public ParserException(string message):base(message) { }
    }
}
