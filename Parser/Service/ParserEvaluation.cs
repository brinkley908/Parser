using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        

        #region Evaluation

        private void EvaluateExpression(ref object value, enCommandType commandType = enCommandType.None) //eval_exp
        {
            GetToken();

            if (string.IsNullOrEmpty(Token)) SyntaxError(enSyntaxError.NoExp);

            if (Token == sSEMI_COLON)
            {
                value = 0;

                return;
            }

            EvaluateIdentifier(ref value, commandType);

            Rewind();
        }

        private void EvaluateAndOr(ref object value, enCommandType commandType = enCommandType.None)
        {

            var result = value;
            object cond = false;

            if (Token == sOR)
            {
                EvaluateExpression(ref cond, commandType);

                value = (bool)value || (bool)cond;

                GetToken();

                EvaluateAndOr(ref value, commandType);

                return;
            }

            if (Token == sAND)
            {
                EvaluateExpression(ref cond, commandType);

                value = (bool)value && (bool)cond;

                GetToken();

                EvaluateAndOr(ref value, commandType);

                return;
            }
        }

        private void EvaluateIdentifier(ref object value, enCommandType commandType = enCommandType.None) //eval_exp0
        {
            string temp;
            enTokenType temp_tok;

            if (TokenType == enTokenType.Identifier)
            {
                if (IsVariable(Token))
                {
                    temp = Token;
                    temp_tok = TokenType;

                    GetToken();

                    if (Token == sEQ)
                    {
                        GetToken();

                        EvaluateIdentifier(ref value, commandType);

                        AssignVariable(temp, value);

                        return;
                    }

                    else if (Token == sINC)
                    {
                        value = FindVar(temp);

                        var newVal = value.AddValues(1);

                        AssignVariable(temp, newVal);

                        GetToken();
                    }

                    else if (Token == sDEC)
                    {
                        value = FindVar(temp);

                        var newVal = value.SubtractValues(1);

                        AssignVariable(temp, newVal);

                        GetToken();
                    }

                    else if (Token == sPROP)
                    {
                        var identifier = GetVar(temp);

                        value = identifier.Value;

                        commandType = identifier.CommandType;
                    }

                    else
                    {
                        Rewind();

                        Token = temp;

                        TokenType = temp_tok;
                    }
                }
            }

            EvaluateCondition(ref value, commandType);
        }

        private void EvaluateCondition(ref object value, enCommandType commandType = enCommandType.None) //eval_exp1
        {
            object partial_value = 0;

            EvaluateAddSubract(ref value, commandType);

            var op = Token;

            if (!RELOPS.Contains(op)) return;

            GetToken();

            EvaluateAddSubract(ref partial_value, commandType);

            switch (op)
            {
                case sLT:
                    {
                        value = value.IsLT(partial_value);

                        break;
                    }

                case sLE:
                    {
                        value = value.IsLE(partial_value);

                        break;
                    }

                case sGT:
                    {
                        value = value.IsGT(partial_value);

                        break;
                    }
                case sGE:
                    {
                        value = value.IsGE(partial_value);

                        break;
                    }

                case sEQEQ:
                    {
                        value = value.IsEQ(partial_value);

                        break;
                    }

                case sNE:
                    {
                        value = value.IsNE(partial_value);

                        break;
                    }
            }
        }

        private void EvaluateAddSubract(ref object value, enCommandType commandType = enCommandType.None) //eval_exp2
        {
            EvaluateDivMultMod(ref value, commandType);

            object partial_value = 0;
            var op = Token;
            int temp;


            if (op == sPLUS || op == sMINUS)
            {
                while (op == sPLUS || op == sMINUS)
                {
                    GetToken();

                    EvaluateDivMultMod(ref partial_value, commandType);

                    switch (op)
                    {
                        case sPLUS:
                            {
                                value = value.AddValues(partial_value);

                                break;
                            }

                        case sMINUS:
                            {
                                value = value.SubtractValues(partial_value);

                                break;
                            }
                    }

                    op = Token;
                }

                return;
            }

            if (op == sINC)
            {
                temp = Pos;

                GetToken();

                value = FindVar(Token);

                value = value.AddValues(1);

                AssignVariable(Token, value);

                GetToken();

                return;
            }

            if (op == sDEC)
            {
                temp = Pos;

                GetToken();

                value = FindVar(Token);

                value = value.SubtractValues(1);

                AssignVariable(Token, value);

                GetToken();

                return;
            }

        }

        private void EvaluateDivMultMod(ref object value, enCommandType commandType = enCommandType.None) //eval_exp3
        {
            EvaluateUnary(ref value, commandType);

            object partial_value = 0;
            var op = Token;

            while (op == sASTERIX || op == sFSLASH || op == sMOD)
            {
                GetToken();

                EvaluateUnary(ref partial_value);

                switch (op)
                {
                    case sASTERIX:
                        {
                            value = value.MultiplyValues(partial_value);

                            break;
                        }

                    case sFSLASH:
                        {
                            value = value.DivideValues(partial_value);

                            break;
                        }

                    case sMOD:
                        {
                            value = value.ModValues(partial_value);

                            break;
                        }
                }

                op = Token;
            }
        }

        private void EvaluateUnary(ref object value, enCommandType commandType = enCommandType.None) //eval_exp4
        {
            var op = string.Empty;

            if (Token == sPLUS || Token == sMINUS)
            {
                op = Token;

                GetToken();
            }

            EvaluateParanthesis(ref value, commandType);

            if (!string.IsNullOrEmpty(op))
            {
                if (op == sMINUS) value = value.UnaryMinusValue();

                if (op == sPLUS) value = value.UnaryPlusValue();
            }
        }

        private void EvaluateParanthesis(ref object value, enCommandType commandType = enCommandType.None) //eval_exp5
        {
            if (Token == sPAREN_OPEN)
            {
                GetToken();

                int loc = Pos;

                EvaluateIdentifier(ref value, commandType);

                EvaluateAndOr(ref value, commandType);

                if (Token != sPAREN_CLOSE)
                {
                    Pos = loc;
                    SyntaxError(enSyntaxError.ParanExpected);
                }

                GetToken();

                return;
            }

            Atom(ref value, commandType);
        }

        private void Atom(ref object value, enCommandType commandType = enCommandType.None)
        {
            switch (TokenType)
            {
                case enTokenType.Identifier:
                    {
                        var f = GetInternalFunc(Token);
                        if (f != enInternalFunction.None)
                        {
                            value = CallInternalFunc(f);

                            return;
                        }

                        var ext = GetExtensionMethod(Token);
                        if (ext != enExtensionMethods.None)
                        {
                            value = CallExtensionMethod(ext, ref value);

                            return;
                        }

                        if (FindFunc(Token) >= 0)
                        {
                            Call();

                            value = _returnValue;

                            if (!EvaluateExtension(ref value))
                                GetToken();

                            return;
                        }

                        value = FindVar(Token);

                        if (!EvaluateExtension(ref value))
                            GetToken();

                        return;
                    }

                case enTokenType.Number:
                    {
                        var number = Token;
                        value = 0;

                        if (int.TryParse(number, out int num)) value = num;
                        else if (decimal.TryParse(number, out decimal dec)) value = dec;

                        GetToken();

                        return;
                    }

                case enTokenType.Delimeter:
                    {
                        ProcessDelimeter(ref value, commandType);

                        return;
                    }

                case enTokenType.Block:
                    {
                        if (commandType == enCommandType.Json)
                        {
                            ParseJson(ref value);

                            return;
                        }
                        break;
                    }

                case enTokenType.Keyword:
                    {
                        if (Token == sTrue || Token == sFalse)
                        {
                            value = Token == sTrue ? true : false;

                            GetToken();
                            return;
                        }
                        break;
                    }

                default:
                    {
                        if (Token == sPAREN_CLOSE) return;

                        break;
                    }
            }

            SyntaxError(enSyntaxError.Syntax);
        }

      
        private bool EvaluateExtension(ref object value)
        {
            GetToken();
                
            if (Token != sDOT)
            {
                Rewind();
                return false;
            }

            GetToken();

            var ext = GetExtensionMethod(Token);

            if (ext != enExtensionMethods.None)
            {
                value = CallExtensionMethod(ext, ref value);

                return true;
            }

            return false;
        }

        private void ProcessDelimeter(ref object value, enCommandType commandType)
        {
            switch (commandType)
            {
                case enCommandType.Json:
                    {
                        GetJson(ref value, commandType);

                        return;
                    }

                default:
                    {
                        ParseString(ref value);

                        return;
                    }
            }
        }

        private bool ParseString(ref object value)
        {
            if (!(Token == sSQUOTE || Token == sDQUOTE))
                return false;

            int loc = Pos;
            var delim = Token == sSQUOTE ? sSQUOTE : sDQUOTE;

            if (delim == sSQUOTE)
            {
                GetToken();

                value = Token;

                GetToken();

                if (Token != sSQUOTE)
                {
                    Pos = loc;
                    SyntaxError(enSyntaxError.QuoteExpected);
                }

                GetToken();

                return true;
            }

            string sValue = "";

            while (Tok != DQUOTE && Tok != NULL && !EOF)
            {
                sValue += Tok;
                Pos++;
            }

            value = sValue;

            GetToken();

            if (Token != sDQUOTE)
            {
                Pos = loc;
                SyntaxError(enSyntaxError.QuoteExpected);
            }

            GetToken();

            return true;
        }

        private void ParseJson(ref object value)
        {
            string sValue = string.Empty;
            int brace = 1;
            int loc = Pos;

            sValue += Token;

            do
            {
                GetToken();

                sValue += Token;

                if (Token == sBRACE_OPEN) brace++;
                if (Token == sBRACE_CLOSE) brace--;

            } while (brace > 0 && !EOF);

            if (EOF)
            {
                Pos = loc;
                SyntaxError(enSyntaxError.UnbalBraces);
            }

            GetToken();

            if (Token != sSEMI_COLON) SyntaxError(enSyntaxError.SemiExpected);

            value = sValue;
        }

        private bool GetJson(ref object value, enCommandType commandType)
        {
            dynamic? ret = null;
            int loc = Pos;
            try
            {

                var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>((string)value);

                do
                {
                    GetToken();

                    ret = ret == null
                        ? dict[Token]
                        : ret[Token];

                    GetToken();
                } while (Token == sPROP);

                value = ret is JValue ? ret.Value : ret;

            }
            catch
            {
                Pos = loc;
                SyntaxError(enSyntaxError.NotVarType);
            }
            return false;
        }

        #endregion

    }
}
