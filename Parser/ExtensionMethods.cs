using BenchmarkDotNet.Characteristics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public static class ExtensionMethods
    {
        public static string TokenValue(this char[] value)
        {
            var ret = "";

            if (value is null || value.Length == 0)
                return ret;

            foreach (char c in value)
                if (c != '\0')
                    ret += c;
                else
                    break;

            return ret;
        }

        public static void Copy(this char[] source, ref char[] dest)
        {
            if(source is null || source.Length==0)
                return;

            int i = 0;

            for (i = 0; i < dest.Length; i++) dest[i] = '\0';

            i = 0;
            foreach (var c in source)
            {

                dest[i++] = c;

                if (i >= dest.Length)
                    break;
            }

        }

        public static void Copy(this string source, ref char[] dest)
        {
            if (source is null || source.Length == 0)
                return;

            int i = 0;
            for (i = 0; i<dest.Length; i++) dest[i] = '\0';

            i = 0;
            foreach (var c in source)
            {

                dest[i++] = c;

                if (i >= dest.Length)
                    break;
            }


        }

        public static bool IsUnary(this object value)
        {
            if(value is null) return false;

            if (value is int) return true;
            if (value is double) return true;
            if (value is Single) return true;
            if (value is long) return true;
            if (value is decimal) return true;
            if (value is short) return true;

            return false;
        }

    }
}
