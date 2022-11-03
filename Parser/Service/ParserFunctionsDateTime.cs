using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        private object Call_DateDiff()
        {
            DateTime? ret = null;
            object value = 0;
            var temp = Pos;

            DateTime dat1;
            DateTime dat2;
            enDateAdd type;

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            EvaluateExpression(ref value);

            if (value is not DateTime) SyntaxError(enSyntaxError.NotVarType);

            dat1 = (DateTime)value;

            GetToken();

            if (Token != sCOMMA) SyntaxError(enSyntaxError.ArgumentExpected, "datetime value expected");

            EvaluateExpression(ref value);

            if (value is not DateTime) SyntaxError(enSyntaxError.NotVarType);

            dat2 = (DateTime)value;

            GetToken();

            if (Token != sCOMMA) SyntaxError(enSyntaxError.ArgumentExpected, "Increment type expected");

            EvaluateExpression(ref value);

            if (!Enum.IsDefined(typeof(enDateAdd), value)) SyntaxError(enSyntaxError.ArgumentOutOfRange, "Invalid date/time increment type");

            type = (enDateAdd)value;

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            GetToken();

            if (Token != sDOT) SyntaxError(enSyntaxError.ExtensionExpected, "dateteime extension method expected");

            object results = (dat1 - dat2);
            
            EvaluateExpression(ref results);

            GetToken();

            return results;
        }

        private DateTime Call_DateAdd()
        {
            DateTime? ret = null;
            object value = 0;
            DateTime dat;
            int inc;
            enDateAdd type;

            GetToken();
            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);


            EvaluateExpression(ref value);

            if (value is not DateTime) SyntaxError(enSyntaxError.NotVarType);

            dat = (DateTime)value;

            GetToken();

            if (Token != sCOMMA) SyntaxError(enSyntaxError.ArgumentExpected, "Increment value expected");

            EvaluateExpression(ref value);

            if (!int.TryParse(value.ToString(), out _)) SyntaxError(enSyntaxError.NotVarType);

            inc = (int)value;

            GetToken();

            if (Token != sCOMMA) SyntaxError(enSyntaxError.ArgumentExpected, "Increment type expected");

            EvaluateExpression(ref value);

            if (!Enum.IsDefined(typeof(enDateAdd), value)) SyntaxError(enSyntaxError.ArgumentOutOfRange, "Invalid date/time increment type");

            type = (enDateAdd)value;

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();


            switch (type)
            {
                case enDateAdd.Day: return dat.AddDays(inc);
                case enDateAdd.Month: return dat.AddMonths(inc);
                case enDateAdd.Year: return dat.AddYears(inc);
                case enDateAdd.Hour: return dat.AddHours(inc);
                case enDateAdd.Minute: return dat.AddMinutes(inc);
                case enDateAdd.Second: return dat.AddSeconds(inc);
                case enDateAdd.Ms: return dat.AddMilliseconds(inc);
            }

            SyntaxError(enSyntaxError.ArgumentOutOfRange);

            return ret ?? DateTime.Now;
        }
        private DateTime Call_Date()
        {
            DateTime? ret = null;

            GetToken();
            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();
            if (Token == sDQUOTE) ret = Call_DateTimeFromString();
            else ret = Call_DateTime();


            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            return ret ?? DateTime.Now;
        }

        private DateTime Call_DateTimeFromString()
        {
            var d = string.Empty;
            do
            {
                if (!ValidDateTok(Tok)) SyntaxError(enSyntaxError.InvalidChar, $"{Tok} not allowed for dateime");

                d += Tok;
                Pos++;
            } while (Tok != DQUOTE && !EOF);

            GetToken();

            if (DateTime.TryParse(d, out DateTime result))
                return result;

            SyntaxError(enSyntaxError.InvalidDateTime);

            return DateTime.Now;
        }

        private DateTime Call_DateTime()
        {
            int[] dParts = new int[] { 0, 0, 0, 0, 0, 0 };
            int i = 0;
            object value = 0;
            int temp = Pos;

            Rewind();

            do
            {
                EvaluateExpression(ref value);

                if (!int.TryParse(value.ToString(), out _)) SyntaxError(enSyntaxError.InvalidDateTime);

                dParts[i++] = (int)value;

                GetToken();

            } while (Token == "," && !EOF);

            Rewind();

            return (i > 1)
                ? new DateTime(dParts[0], dParts[1], dParts[2], dParts[3], dParts[4], dParts[5])
                : new DateTime(dParts[0]);
        }


        private DateTime Call_Now()
        {
            GetToken();
            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();
            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            return DateTime.Now;
        }


    }
}
