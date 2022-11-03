using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        private object _returnValue = 0;

        private void Prescan()
        {
            int brace = 0;
            string temp;

            Reset();

            do
            {
                while (brace > 0)
                {
                    GetToken();

                    if (Token == sBRACE_OPEN) brace++;

                    if (Token == sBRACE_CLOSE) brace--;
                }

                GetToken();

                if (CommandType == enCommandType.Json
                    || CommandType == enCommandType.Int
                    || CommandType == enCommandType.String
                    || CommandType == enCommandType.Bool
                    || CommandType == enCommandType.Var
                    || CommandType == enCommandType.DateTime
                    || CommandType == enCommandType.Decimal)
                {
                    Rewind();

                    DeclareGlobal();
                }

                else if (TokenType == enTokenType.Identifier)
                {
                    temp = Token;

                    GetToken();

                    if (Token == sPAREN_OPEN)
                    {
                        var func = new FunctionItem
                        {
                            Name = temp,

                            Location = Pos,
                        };

                        _functions.Add(func);
                        Console.WriteLine($"Function Name = {func.Name}, Location = {func.Location}");

                        while (Tok != PAREN_CLOSE) Pos++;

                        Pos++;
                    }

                    else Rewind();
                }

                else if (TokenType == enTokenType.Keyword)
                {
                    if (CommandType == enCommandType.Import)
                    {
                        Import();
                    }
                }

                else if (Token == sBRACE_OPEN) brace++;

            } while (CommandType != enCommandType.Finished && !EOF);

            Pos = 0;
        }

        private void Call()
        {
            int temp = Pos;
            int lvartemp = lvartos;
            var loc = FindFunc(Token);

            if (loc < 0)
            {
                Pos = temp;
                SyntaxError(enSyntaxError.NoFunc);
            }

            GetArgs();

            temp = Pos;

            PushFunc(lvartemp);

            Pos = loc;

            GetParams();

            InterperetBlock();

            Pos = temp;

            lvartos = PopFunc();

            ClearLocals();
        }

        private void InterperetBlock()
        {
            object value = 0;
            int block = 0;
            int temp;

            do
            {
                TokenType = GetToken();

                if (TokenType == enTokenType.Identifier)
                {
                    Rewind();

                    temp = Pos;

                    EvaluateExpression(ref value);

                    if (Token != sSEMI_COLON)
                    {
                        Pos = temp;
                        SyntaxError(enSyntaxError.SemiExpected);
                    }
                }
                else if (TokenType == enTokenType.Delimeter)
                {
                    if (Token == sINC || Token == sDEC)
                    {
                        Rewind();

                        EvaluateExpression(ref value);
                    }
                }
                else if (TokenType == enTokenType.Block)
                {
                    if (Token == sBRACE_OPEN) block = 1;

                    else return;
                }
                else
                {
                    switch (CommandType)
                    {
                        case enCommandType.Json:
                        case enCommandType.Int:
                        case enCommandType.Var:
                        case enCommandType.Bool:
                        case enCommandType.DateTime:
                        case enCommandType.String:
                        case enCommandType.Decimal:
                            {
                                Rewind();

                                DeclareLocal();

                                break;
                            }

                        case enCommandType.Return:
                            {
                                FuncReturn();

                                break;
                            }

                        case enCommandType.For:
                            {
                                ExecuteForLoop();

                                break;
                            }

                        case enCommandType.If:
                            {
                                ExecuteIf();

                                break;
                            }

                        case enCommandType.Else:
                            {
                                FindEndOfBlock();

                                break;
                            }

                        case enCommandType.While:
                            {
                                ExecuteWhile();

                                break;
                            }

                        case enCommandType.Do:
                            {
                                ExecuteDo();

                                break;
                            }

                        case enCommandType.End:
                            {
                                CommandType = enCommandType.Finished;

                                block = 0;

                                break;

                            }
                    }
                }

            } while (CommandType != enCommandType.Finished && block > 0);
        }



        private void FindEndOfBlock()
        {
            int brace;

            GetToken();

            brace = 1;

            do
            {
                GetToken();

                if (Token == sBRACE_OPEN) brace++;

                if (Token == sBRACE_CLOSE) brace--;

            } while (brace > 0);
        }

        private void ExecuteForLoop()
        {
            int loc;
            int temp;
            int temp2;
            object cond = false;
            int brace;

            GetToken();

            loc = Pos;
            EvaluateExpression(ref cond);

            if (Token != sSEMI_COLON)
            {
                Pos = loc;
                SyntaxError(enSyntaxError.SemiExpected);
            }

            Pos++;

            temp = Pos;

            for (; ; )
            {
                loc = Pos;
                EvaluateExpression(ref cond);

                if (Token != sSEMI_COLON)
                {
                    Pos = loc;
                    SyntaxError(enSyntaxError.SemiExpected);
                }

                Pos++;

                temp2 = Pos;

                brace = 1;

                while (brace > 0)
                {
                    GetToken();

                    if (Token == sPAREN_OPEN) brace++;

                    if (Token == sPAREN_CLOSE) brace--;
                }

                if ((bool)cond)
                {
                    InterperetBlock();
                }
                else
                {
                    FindEndOfBlock();

                    return;
                }

                Pos = temp2;

                EvaluateExpression(ref cond);

                Pos = temp;
            }
        }

        private void ExecuteIf()
        {
            object cond = false;

            EvaluateExpression(ref cond);

            if ((bool)cond)
            {
                InterperetBlock();
            }
            else
            {
                FindEndOfBlock();

                GetToken();

                if (CommandType != enCommandType.Else)
                {
                    Rewind();

                    return;
                }

                InterperetBlock();
            }
        }

        private void ExecuteWhile()
        {
            object cond = false;
            int temp;

            Rewind();

            temp = Pos;

            GetToken();

            EvaluateExpression(ref cond);

            if ((bool)cond)
            {
                InterperetBlock();
            }
            else
            {
                FindEndOfBlock();

                return;
            }

            Pos = temp;
        }

        private void ExecuteDo()
        {
            object cond = false;
            int temp;

            Rewind();

            temp = Pos;

            GetToken();

            InterperetBlock();

            GetToken();

            if (CommandType != enCommandType.While) SyntaxError(enSyntaxError.WhileExpected);

            EvaluateExpression(ref cond);

            if ((bool)cond) Pos = temp;
        }

        private void FuncReturn()
        {
            object value = 0;

            EvaluateExpression(ref value);

            _returnValue = value;
        }
    }
}
