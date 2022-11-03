using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        #region Constants
        private const char CR = '\r';
        private const char LF = '\n';
        private readonly char[] CRLF = new char[2] { CR, LF };
        private const char SPACE = ' ';
        private const char TAB = '\t';
        private const char NULL = '\0';
        private const char FSLASH = '/';
        private const char ASTERIX = '*';
        private const char EQ = '=';
        private const char LT = '<';
        private const char GT = '>';
        private const char NOT = '!';
        private const char PLUS = '+';
        private const char MINUS = '-';
        private const char OR = '|';
        private const char AND = '&';

        private const string sCR = "\r";
        private const string sLF = "\n";
        private const string sCRLF = "\r\n";
        private const string sSPACE = " ";
        private const string sTAB = "\t";
        private const string sNULL = "\0";
        private const string sFSLASH = "/";
        private const string sASTERIX = "*";

        private const string sPROP = "->";
        private const string sINC = "++";
        private const string sDEC = "--";
        private const string sEQ = "=";
        private const string sLT = "<";
        private const string sGT = ">";
        private const string sNE = "!=";
        private const string sLE = "<=";
        private const string sGE = ">=";
        private const string sAND = "&&";
        private const string sOR = "||";
        private const string sEQEQ = "==";
        private const string sTrue = "true";
        private const string sFalse = "false";


        private string[] RELOPS = new string[] { sLT, sLE, sGT, sGE, sEQEQ, sNE };

        private const string sNOT = "!";
        private const string sPLUS = "+";
        private const string sMINUS = "-";
        private const string sDIVIDE = "/";
        private const string sMULTIPLY = "*";
        private const string sMOD = "%";


        private const char DOT = '.';
        private const char SEMI_COLON = ';';
        private const char COMMA = ',';
        private const char DQUOTE = '"';
        private const char SQUOTE = '\'';
        private const char BRACE_OPEN = '{';
        private const char BRACE_CLOSE = '}';
        private const char BRACKET_OPEN = '[';
        private const char BRACKET_CLOSE = ']';
        private const char PAREN_OPEN = '(';
        private const char PAREN_CLOSE = ')';

        private const string sDOT = ".";
        private const string sSEMI_COLON = ";";
        private const string sCOMMA = ",";
        private const string sDQUOTE = "\"";
        private const string sSQUOTE = "'";
        private const string sBRACE_OPEN = "{";
        private const string sBRACE_CLOSE = "}";
        private const string sBRACKET_OPEN = "[";
        private const string sBRACKET_CLOSE = "]";
        private const string sPAREN_OPEN = "(";
        private const string sPAREN_CLOSE = ")";
        #endregion

        private int Pos { get; set; } = 0;
        private bool EOF => Pos >= _source.Length;
        private string Token { get; set; } = string.Empty;
        private char Tok
        {
            get
            {
                if (EOF) return NULL;

                ReadOnlySpan<char> span = _source;
                return span.Slice(Pos, 1)[0];
            }
        }

        private void Peddle(int steps = 1) => Pos += steps;

        private void Reset(bool clearVars = true)
        {
            Pos = 0;
            lvartos = 0;
            functos = 0;

            if (clearVars)
            {
                _globalVars.Clear();
                ClearLocals();
                _functions.Clear();
            }
        }

        private char TokNext()
        {
            ReadOnlySpan<char> span = _source;
            return span.Slice(Pos + 1, 1)[0];
        }

        #region GetToken

        public enTokenType GetToken()
        {
            TokenType = enTokenType.None;
            CommandType = enCommandType.None;

            if (string.IsNullOrEmpty(_source))
            {
                TokenState = enTokenState.Finished;
                return enTokenType.None;
            }

            Token = string.Empty;

            while (IsWhiteSpace()) ++Pos;

            if (Tok == CR || Tok == LF)
            {
                Pos += 2;

                while (IsWhiteSpace(CRLF)) ++Pos;
            }

            if (EOF || Tok == NULL)
            {
                TokenState = enTokenState.Finished;

                TokenType = enTokenType.Delimeter;

                return TokenType;
            }

            if ("{}".Contains(Tok))
            {
                Token += Tok;

                Pos++;

                TokenType = enTokenType.Block;
                return TokenType;
            }

            if ("[]".Contains(Tok))
            {
                Token += Tok;

                Pos++;

                TokenType = enTokenType.Delimeter;
                return TokenType;
            }

            SkipComments();

            //if (Tok == FSLASH) // '/'
            //{
            //    if (TokNext() == ASTERIX) // '*'
            //    {
            //        _pos += 2;

            //        do
            //        {
            //            while (Tok != ASTERIX) _pos++;

            //            _pos++;

            //        } while (Tok != FSLASH);

            //        _pos++;
            //    }

            //    if (TokNext() == FSLASH) // '/'
            //    {
            //        _pos += 2;

            //        do
            //        {
            //            _pos++; ;

            //        } while (Tok != CR && Tok != LF);

            //        _pos++;
            //    }

            //    while (IsWhiteSpace(CRLF)) _pos++; ;
            //}

            if ("|&".Contains(Tok))
            {
                if (Tok == OR && TokNext() == OR)
                {
                    Pos += 2;

                    Token = sOR;

                    TokenType = enTokenType.Delimeter;
                    return TokenType;
                }

                if (Tok == AND && TokNext() == AND)
                {
                    Pos += 2;

                    Token = sAND;

                    TokenType = enTokenType.Delimeter;
                    return TokenType;
                }

                SyntaxError(Tok == OR ? enSyntaxError.OrExpected : enSyntaxError.AndExpected);
            }

            if ("!<>=".Contains(Tok))
            {
                var hasToken = false;

                switch (Tok)
                {
                    case EQ:
                        {
                            if (TokNext() == EQ)
                            {
                                Pos += 2;

                                Token = "==";

                                hasToken = true;
                            }
                            break;
                        }

                    case NOT:
                        {
                            if (TokNext() == EQ)
                            {
                                Pos += 2;

                                Token = "!=";

                                hasToken = true;
                            }
                            break;
                        }

                    case LT:
                        {
                            if (TokNext() == EQ)
                            {
                                Pos += 2;

                                Token = "<=";
                            }
                            else
                            {
                                Pos++;

                                Token = "<";
                            }

                            hasToken = true;
                            break;
                        }

                    case GT:
                        {

                            if (TokNext() == EQ)
                            {
                                Pos += 2;

                                Token = ">=";

                            }
                            else
                            {
                                Pos++;

                                Token = ">";
                            }

                            hasToken = true;
                            break;
                        }
                }

                if (hasToken)
                {
                    TokenType = enTokenType.Delimeter;
                    return TokenType;
                }
            }

            if (Tok == MINUS && TokNext() == GT)
            {
                Pos += 2;

                Token = sPROP;

                TokenType = enTokenType.Delimeter;
                return TokenType;
            }

            if ("+-*^/%=;:(),'".Contains(Tok))
            {
                if (Tok == PLUS && TokNext() == PLUS)
                {
                    Pos += 2;

                    Token = sINC;
                }
                else if (Tok == MINUS && TokNext() == MINUS)
                {
                    Pos += 2;

                    Token = sDEC;
                }
                else
                {
                    Token += Tok;

                    Pos++;
                }

                TokenType = enTokenType.Delimeter;
                return TokenType;
            }

            if (Tok == DQUOTE || Tok == DOT)
            {
                Token += Tok;

                Pos++;

                TokenType = enTokenType.Delimeter;
                return TokenType;
            }

            if (IsDigit(Tok))
            {
                Token += Tok;

                Pos++;

                while (!IsDelim(Tok))
                {
                    Token += Tok;

                    Pos++;
                }

                TokenType = enTokenType.Number;
                return TokenType;
            }

            if (IsAlpha(Tok))
            {
                Token += Tok;

                Pos++;

                while (!IsDelim(Tok, new char[1] {'.'}))
                {
                    Token += Tok;

                    Pos++;
                }

                TokenType = enTokenType.Temp;
            }

            if (TokenType == enTokenType.Temp)
            {
                CommandType = LookUp();

                if (CommandType != enCommandType.None) TokenType = enTokenType.Keyword;
                else TokenType = enTokenType.Identifier;
            }

            return TokenType;
        }
        private bool IsWhiteSpace(char[]? include = null) => (Tok == SPACE || Tok == TAB || (include != null && include.Contains(Tok))) && !EOF;
        private bool IsDigit(char c) => "0123456789".Contains(c);
        private bool IsAlpha(char c) => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c);
        private bool IsDelim(char c, char[]? include = null) => " !;:,+-<>'/*%/^=(){}".Contains(c) || c == 9 || c == '\r' || c == 0 || (include != null && include.Contains(Tok));
        private enCommandType LookUp() => _commands.FirstOrDefault(x => x.Command.Equals(Token, StringComparison.CurrentCultureIgnoreCase))?.Token ?? enCommandType.None;
        private void Rewind() => Pos -= Token.Length;

        private bool SkipComments()
        {
            var ret = false;

            if (Tok == FSLASH) // '/'
            {
                if (TokNext() == ASTERIX) // '*'
                {
                    ret = true;

                    Pos += 2;

                    do
                    {
                        while (Tok != ASTERIX) Pos++;

                        Pos++;

                    } while (Tok != FSLASH);

                    Pos++;
                }

                if (TokNext() == FSLASH) // '/'
                {
                    ret = true;
                    Pos += 2;

                    do
                    {
                        Pos++; ;

                    } while (Tok != CR && Tok != LF);

                    Pos++;
                }

                while (IsWhiteSpace(CRLF)) Pos++;
            }

            return ret;
        }

        #endregion

    }
}
