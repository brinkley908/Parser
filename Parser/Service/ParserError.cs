using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        private Dictionary<enSyntaxError, string> _errors = new Dictionary<enSyntaxError, string>
        {
            { enSyntaxError.Syntax, "Syntax Error" },
            { enSyntaxError.UnbalParens, "Unbalanced parenthesis"},
            { enSyntaxError.NoExp, "No expression present"},
            { enSyntaxError.EqExpected, "Equals sign expected"},
            { enSyntaxError.NotVar, "Unknown variable"},
            { enSyntaxError.ParamErr, "Parameter error"},
            { enSyntaxError.SemiExpected, "Semicolon expected"},
            { enSyntaxError.UnbalBraces, "Unbalanced braces"},
            { enSyntaxError.NoFunc, "Unknown function"},
            { enSyntaxError.TypeExpected, "Type specifier expected"},
            { enSyntaxError.NestedFuncs, "Too many nested function calls"},
            { enSyntaxError.NoRetCall, "return without call"},
            { enSyntaxError.ParanExpected, "Parantheses expected"},
            { enSyntaxError.WhileExpected, "While expected"},
            { enSyntaxError.QuoteExpected, "Closing quote expected"},
            { enSyntaxError.NoString, "Not a string"},
            { enSyntaxError.TooManyLVars, "Too many local variables"},
            { enSyntaxError.AndExpected, "& sign expected"},
            { enSyntaxError.OrExpected, "| sign expected"},
            { enSyntaxError.ImportInvalid, "Invalid import position, place imports at top of the code file"},
            { enSyntaxError.ImportNotFound, "Import file not found"},
            { enSyntaxError.ImportEmpty, "Import file was empty"},
            { enSyntaxError.NotVarType, "Invalid type assignment"},
            { enSyntaxError.InvalidChar, "Invalid character found"},
            { enSyntaxError.InvalidDateTime, "Invalid date/time format"},
            { enSyntaxError.ArgumentExpected, "Argument expected"},
            { enSyntaxError.ArgumentOutOfRange, "Argument out of range"},
            { enSyntaxError.ExtensionExpected, "Extension method expected"},
            { enSyntaxError.Null, "Object reference not set to an instance"},
        };

        #region Syntax Error
        private void SyntaxError(enSyntaxError err, string? extra = null)
        {
            string msg = $"Syntax Error: {_errors[err] ?? _errors[0]}{sCRLF}";
            int lineCount = 0;
            var temp = Pos;
            string code = string.Empty;

            if (!string.IsNullOrEmpty(extra)) msg += $"{extra}{sCRLF}";

            do
            {
                Pos--;
            } while (Tok != CR && Tok != LF && Tok != SEMI_COLON && Pos >= 0);

            if (Tok == CR || Tok == LF || Tok == SEMI_COLON) Pos++;

            do
            {
                code += Tok;
                Pos++;
            } while (Tok != CR && Tok != LF && Tok != SEMI_COLON && Tok != NULL && !EOF);


            if ((_source.Contains(sCR) || _source.Contains(sLF)))
            {
                Pos = 0;
                do
                {
                    Pos++;
                    if (Tok == CR)
                    {
                        lineCount++;

                    }


                } while (Pos != temp && Tok != NULL && !EOF);
            }

            msg += $"{code}{sCRLF}Line Number: {lineCount}{sCRLF}";

            throw new ParserException(msg) { Token = Token, CommandType = CommandType, TokenType = TokenType, TokenState = TokenState, Tok = Tok, EOF = EOF, LineNumber = lineCount };
        }
        #endregion
    }
}
