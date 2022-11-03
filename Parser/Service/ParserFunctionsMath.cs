using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        private int Call_Abs()
        {
            var temp = Pos;
            object value = 0;

            GetToken();
            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            Rewind();
            EvaluateExpression(ref value);


            GetToken();
            if (Token != sPAREN_CLOSE)
            {
                Pos = temp;
                SyntaxError(enSyntaxError.ParanExpected);
            }

            GetToken();
            GetToken();

            if (Token != sSEMI_COLON) SyntaxError(enSyntaxError.SemiExpected);

            Rewind();

            return Math.Abs((int)value);
        }
    }
}
