using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public enum enExtensionMethods
    {
        None,
        Trim,
        ToString,
        Days,
        TotalDays,
        Hours,
        TotalHours,
        Minutes,
        TotalMinutes,
        Seconds,
        TotalSeconds,
        Milliseconds,
        TotalMilliseconds
    }

    public partial class ParserService
    {
        private List<ExtensionMethod> _extensions = new()
        {
            new ExtensionMethod{ Name = "Trim", Type = enExtensionMethods.Trim },
            new ExtensionMethod{ Name = "ToString", Type = enExtensionMethods.ToString },
            new ExtensionMethod{ Name = "Days", Type = enExtensionMethods.Days },
            new ExtensionMethod{ Name = "TotalDays", Type = enExtensionMethods.TotalDays },
            new ExtensionMethod{ Name = "Hours", Type = enExtensionMethods.Hours },
            new ExtensionMethod{ Name = "TotalHours", Type = enExtensionMethods.TotalHours },
            new ExtensionMethod{ Name = "Minutes", Type = enExtensionMethods.Minutes },
            new ExtensionMethod{ Name = "TotalMinnutes", Type = enExtensionMethods.TotalMinutes},
            new ExtensionMethod{ Name = "Seconds", Type = enExtensionMethods.Seconds },
            new ExtensionMethod{ Name = "TotalSeconds", Type = enExtensionMethods.TotalSeconds },
            new ExtensionMethod{ Name = "Milliseconds", Type = enExtensionMethods.Milliseconds },
            new ExtensionMethod{ Name = "TotalMilliseconds", Type = enExtensionMethods.TotalMilliseconds },
        };

        private enExtensionMethods GetExtensionMethod(string s) => _extensions.FirstOrDefault(x => x.Name == s)?.Type ?? enExtensionMethods.None;

        private object CallExtensionMethod(enExtensionMethods ex, ref object value)
        {
            switch (ex)
            {
                case enExtensionMethods.Trim: return Call_Ext_Trim(ref value);
                case enExtensionMethods.ToString: return Call_Ext_ToString(ref value);
                case enExtensionMethods.Days: return Call_Ext_Days(ref value);
                case enExtensionMethods.TotalDays: return Call_Ext_TotalDays(ref value);
                case enExtensionMethods.Hours: return Call_Ext_Hours(ref value);
                case enExtensionMethods.TotalHours: return Call_Ext_TotalHours(ref value);
                case enExtensionMethods.Minutes: return Call_Ext_Minutes(ref value);
                case enExtensionMethods.TotalMinutes: return Call_Ext_TotalMinutes(ref value);
                case enExtensionMethods.Seconds: return Call_Ext_Seconds(ref value);
                case enExtensionMethods.TotalSeconds: return Call_Ext_TotalSeconds(ref value);
                case enExtensionMethods.Milliseconds: return Call_Ext_Milliseconds(ref value);
                case enExtensionMethods.TotalMilliseconds: return Call_Ext_TotalMilliseconds(ref value);
                default: return 0;
            }
        }

      
        private string Call_Ext_Trim(ref object value)
        {
            if (value is null) return string.Empty;
            if (value is not string) SyntaxError(enSyntaxError.NotVarType, "string expression expected");

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();


            return ((string)value).Trim();
        }

        private string Call_Ext_ToString(ref object value)
        {
            if (value is null) return string.Empty;

            GetToken();

            if (Token != sPAREN_OPEN) SyntaxError(enSyntaxError.ParanExpected);

            GetToken();

            if (Token != sPAREN_CLOSE) SyntaxError(enSyntaxError.ParanExpected);

            Peddle();

            return value.ToString();
        }


    }
}
