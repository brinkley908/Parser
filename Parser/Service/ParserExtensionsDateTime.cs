using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{

    public partial class ParserService
    {

        private int Call_Ext_Days(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (int)((TimeSpan)value).Days;
        }

        private int Call_Ext_TotalDays(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (int)((TimeSpan)value).TotalDays;
        }

        private int Call_Ext_Hours(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (int)((TimeSpan)value).Hours;
        }

        private decimal Call_Ext_TotalHours(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (decimal)((TimeSpan)value).TotalHours;
        }

        private int Call_Ext_Minutes(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (int)((TimeSpan)value).Minutes;
        }

        private decimal Call_Ext_TotalMinutes(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (decimal)((TimeSpan)value).TotalMinutes;
        }

        private int Call_Ext_Seconds(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return ((TimeSpan)value).Seconds;
        }

        private decimal Call_Ext_TotalSeconds(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (decimal)((TimeSpan)value).TotalSeconds;
        }

        private int Call_Ext_Milliseconds(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return ((TimeSpan)value).Milliseconds;
        }

        private decimal Call_Ext_TotalMilliseconds(ref object value)
        {
            if (value is null) SyntaxError(enSyntaxError.Null);
            if (value is not TimeSpan) SyntaxError(enSyntaxError.NotVarType, "TimeSpan required");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return (decimal)((TimeSpan)value).TotalMilliseconds;
        }

    }
}
