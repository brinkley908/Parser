using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public enum enInternalFunction
    {
        None,
        Abs,
        Printf,
        Now,
        Date,
        DateAdd,
        DateDiff
    }

    public partial class ParserService
    {

        private string _validDateTimeChars = "-/: .";

        private List<Identifier> _internalFuncs = new()
        {
            new Identifier{ Name="abs", InternalFunction = enInternalFunction.Abs },
            new Identifier{ Name="printf", InternalFunction = enInternalFunction.Printf },
            new Identifier{ Name="Now", InternalFunction = enInternalFunction.Now },
            new Identifier{ Name="Date", InternalFunction = enInternalFunction.Date },
            new Identifier{ Name="DateAdd", InternalFunction = enInternalFunction.DateAdd },
            new Identifier{ Name="DateDiff", InternalFunction = enInternalFunction.DateDiff }
        };


        #region Internal functions

        private enInternalFunction GetInternalFunc(string s) => _internalFuncs.FirstOrDefault(x => x.Name.Equals(s, StringComparison.CurrentCultureIgnoreCase))?.InternalFunction ?? enInternalFunction.None;


        private object CallInternalFunc(enInternalFunction f)
        {
            switch (f)
            {
                case enInternalFunction.Abs: return Call_Abs();

                case enInternalFunction.Printf: return Call_Printf();

                case enInternalFunction.Now: return Call_Now();

                case enInternalFunction.Date: return Call_Date();

                case enInternalFunction.DateAdd: return Call_DateAdd();

                case enInternalFunction.DateDiff: return Call_DateDiff();

                default: return 0;
            }
        }
        private bool ValidDateTok(char tok) => IsDigit(tok) || _validDateTimeChars.Contains(tok);


        private int Call_Printf()
        {
            var line = string.Empty;
            object value = 0;
            var temp = Pos;

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            do
            {
                GetToken();

                if (TokenType == enTokenType.String)
                {
                    line += Token;
                }
                else
                {
                    Rewind();
                    EvaluateExpression(ref value);
                    line += value?.ToString() ?? string.Empty;
                }

                GetToken();

            } while (Token == sCOMMA);

            Console.WriteLine(line);

            Rewind();

            GetToken();

            if (Token != sPAREN_CLOSE)
            {
                Pos = temp;
                SyntaxError(enSyntaxError.ParanExpected);
            }

            GetToken();

            if (Token != sSEMI_COLON) SyntaxError(enSyntaxError.SemiExpected);

            Rewind();

            return 0;
        }

        #endregion

    }
}
