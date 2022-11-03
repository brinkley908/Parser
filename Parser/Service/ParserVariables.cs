using Newtonsoft.Json;
using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        private Identifier[] _localVarStack = new Identifier[MAX_LOCAL_VARS];

        private List<Identifier> _globalVars = new();

        private List<FunctionItem> _functions = new();

        private int[] _callStack = new int[MAX_FUNCS];

        private int lvartos = 0;

        private int functos = 0;

        #region Arguments, Variables & Identifiers

        private void GetArgs()
        {
            object value = 0;
            int count = 0;
            object[] temp = new object[MAX_PARAMS];

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            do
            {
                EvaluateExpression(ref value);

                temp[count] = value;

                GetToken();

                count++;

            } while (Token == sCOMMA);

            count--;

            for (; count >= 0; --count)
            {
                Identifier identifier = new();

                identifier.Value = temp[count];

                identifier.CommandType = enCommandType.Arg;

                PushLocal(identifier);
            }
        }

        private void GetParams()
        {
            Identifier p;
            int i = lvartos - 1;

            do
            {
                GetToken();

                if (Token == sPAREN_CLOSE) break;

                p = _localVarStack[i];

                if (CommandType != enCommandType.Int
                    && CommandType != enCommandType.Json
                    && CommandType != enCommandType.Bool
                    && CommandType != enCommandType.String
                    && CommandType != enCommandType.Decimal
                    && CommandType != enCommandType.DateTime
                    && CommandType != enCommandType.Decimal) SyntaxError(enSyntaxError.TypeExpected);

                p.CommandType = CommandType;

                GetToken();

                p.Name = Token;

                GetToken();

                i--;

            } while (Token == sCOMMA);

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);
        }

        private bool IsVariable(string name)
        {
            int i;
            if (functos > 0)
                for (i = lvartos - 1; i >= _callStack[functos - 1]; i--)
                    if (_localVarStack[i].Name == name)
                        return true;

            return _globalVars.Any(x => x.Name == name);
        }

        private void AssignVariable(string name, object value)
        {
            int i = 0;

            if (functos > 0)
                for (i = lvartos - 1; i >= _callStack[functos - 1]; i--)
                {
                    var lvar = _localVarStack[i];
                    if (lvar.Name == name)
                    {
                        DetermineVarType(ref lvar, value);
                        lvar.Value = value;
                        return;
                    }
                }

            if (i == 0 || i < _callStack[functos - 1])
            {
                var g = _globalVars.FirstOrDefault(x => x.Name == name);
                if (g != null)
                {
                    DetermineVarType(ref g, value);
                    g.Value = value;
                    return;
                }
            }

            SyntaxError(enSyntaxError.NotVar);
        }

        private void DetermineVarType(ref Identifier identifier, object value)
        {
            if (identifier.CommandType == enCommandType.Var)
            {
                if (value is int) { identifier.CommandType = enCommandType.Int; return; }
                if (value is bool) { identifier.CommandType = enCommandType.Bool; return; }
                if (value is DateTime) { identifier.CommandType = enCommandType.DateTime; return; }
                if (value is decimal) { identifier.CommandType = enCommandType.Decimal; return; }

                if (value is string)
                {
                    try
                    {
                        var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>((string)value);
                        identifier.CommandType = enCommandType.Json;
                    }
                    catch
                    {
                        identifier.CommandType = enCommandType.String;
                    }
                }

                return;
            }

            CheckVarType(ref identifier, value);
        }

        private void CheckVarType(ref Identifier identifier, object value)
        {
            if (identifier.CommandType == enCommandType.Int)
                if (value is not int) SyntaxError(enSyntaxError.NotVarType, $"{identifier.Name}={value.ToString()} - Integer value expected");

            if (identifier.CommandType == enCommandType.String)
                if (value is not string) SyntaxError(enSyntaxError.NotVarType, $"{identifier.Name}={value.ToString()} - String value expected");

            if (identifier.CommandType == enCommandType.Bool)
                if (value is not bool) SyntaxError(enSyntaxError.NotVarType, $"{identifier.Name}={value.ToString()} - Boolean value expected");

            if (identifier.CommandType == enCommandType.DateTime)
                if (value is not DateTime) SyntaxError(enSyntaxError.NotVarType, $"{identifier.Name}={value.ToString()} - DateTime value expected");

            if (identifier.CommandType == enCommandType.Decimal)
                if (value is not decimal) SyntaxError(enSyntaxError.NotVarType, $"{identifier.Name}={value.ToString()} - Decimal value expected");

            if (identifier.CommandType == enCommandType.Json)
            {
                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>((string)value);
                }
                catch
                {
                    SyntaxError(enSyntaxError.NotVarType, $"{identifier.Name}={value.ToString()} - Json value expected");
                }
            }

        }


        private void PushFunc(int i)
        {
            if (functos > MAX_FUNCS) SyntaxError(enSyntaxError.NestedFuncs);

            _callStack[functos] = i;

            functos++;
        }

        private int PopFunc()
        {
            functos--;

            if (functos < 0) SyntaxError(enSyntaxError.NoRetCall);

            return _callStack[functos];
        }

        private void ClearLocals()
        {
            for (int i = lvartos; i < MAX_LOCAL_VARS; i++) _localVarStack[i] = null;
        }

        private void PushLocal(Identifier identifier)
        {
            if (lvartos > MAX_LOCAL_VARS) SyntaxError(enSyntaxError.TooManyLVars);

            _localVarStack[lvartos] = identifier;

            lvartos++;
        }

        public int FindFunc(string name)
        {
            var ret = _functions.FirstOrDefault(f => f.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

            if (ret != null)
                return ret.Location;

            return -1;
        }

       

        private object? FindVar(string s)
        {
            if (functos > 0)
                for (int i = lvartos - 1; i >= _callStack[functos - 1]; i--)
                    if (_localVarStack[i].Name == s)
                        return _localVarStack[i].Value;

            var item = _globalVars.FirstOrDefault(x => x.Name.Equals(s, StringComparison.CurrentCultureIgnoreCase));

            if (item is null) SyntaxError(enSyntaxError.NotVar);

            return item?.Value;
        }

        private Identifier? GetVar(string s)
        {
            if (functos > 0)
                for (int i = lvartos - 1; i >= _callStack[functos - 1]; i--)
                    if (_localVarStack[i].Name == s)
                        return _localVarStack[i];

            var item = _globalVars.FirstOrDefault(x => x.Name.Equals(s, StringComparison.CurrentCultureIgnoreCase));

            if (item is null) SyntaxError(enSyntaxError.NotVar);

            return item;
        }

        private void DeclareLocal()
        {
            GetToken();
            var commandType = CommandType;

            do
            {
                object value = 0;
                Identifier identifier = new()
                {
                    CommandType = commandType,
                    Value = 0
                };

                GetToken();

                identifier.Name = Token;

                PushLocal(identifier);

                EvaluateIdentifier(ref value, commandType);

                Rewind();

                GetToken();

            } while (Token == sCOMMA);

            if (Token != sSEMI_COLON) SyntaxError(enSyntaxError.SemiExpected);
        }

        private void DeclareGlobal()
        {
            var commandType = CommandType;

            GetToken();

            do
            {
                GetToken();

                object value = 0;
                var id = new Identifier
                {
                    CommandType = commandType,
                    Value = null,
                    Name = Token
                };

                _globalVars.Add(id);

                EvaluateIdentifier(ref value, commandType);

                Console.WriteLine($"Global Variable {id.Name} = {id.Value}");

                Rewind();

                GetToken();

            } while (Token == sCOMMA);

            if (Token != sSEMI_COLON) SyntaxError(enSyntaxError.SemiExpected);
        }

        #endregion


    }
}
