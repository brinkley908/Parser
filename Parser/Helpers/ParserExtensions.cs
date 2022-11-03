using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public static class ParserExtensions
    {
        public static bool IsLT(this object value, object partial_value)
        {
            if (value is int @int) return @int < (int)partial_value;
            if (value is double @double) return @double < (double)partial_value;
            if (value is Single @single) return @single < (Single)partial_value;
            if (value is long @long) return @long < (long)partial_value;
            if (value is decimal @decimal) return @decimal < (decimal)partial_value;
            if (value is short @short) return @short < (short)partial_value;

            if (value is DateTime && partial_value is DateTime)
                return (DateTime)value < (DateTime)partial_value;

            if (value is string && partial_value is string)
                return string.Compare(value.ToString(), partial_value.ToString()) < 0;

            return false;
        }

        public static bool IsLE(this object value, object partial_value)
        {
            if (value is int @int) return @int <= (int)partial_value;
            if (value is double @double) return @double <= (double)partial_value;
            if (value is Single @single) return @single <= (Single)partial_value;
            if (value is long @long) return @long <= (long)partial_value;
            if (value is decimal @decimal) return @decimal <= (decimal)partial_value;
            if (value is short @short) return @short <= (short)partial_value;

            if (value is DateTime && partial_value is DateTime)
                return (DateTime)value <= (DateTime)partial_value;

            if (value is string @val && partial_value is string @part)
                return string.Compare(@val, @part) < 0 || val == @part;

            return false;
        }

        public static bool IsGT(this object value, object partial_value)
        {
            if (value is int @int) return @int > (int)partial_value;
            if (value is double @double) return @double > (double)partial_value;
            if (value is Single @single) return @single > (Single)partial_value;
            if (value is long @long) return @long > (long)partial_value;
            if (value is decimal @decimal) return @decimal > (decimal)partial_value;
            if (value is short @short) return @short > (short)partial_value;

            if (value is DateTime && partial_value is DateTime)
                return (DateTime)value > (DateTime)partial_value;

            if (value is string @val && partial_value is string @part)
                return string.Compare(@val, @part) > 0;

            return false;
        }

        public static bool IsGE(this object value, object partial_value)
        {
            if (value is int @int) return @int >= (int)partial_value;
            if (value is double @double) return @double >= (double)partial_value;
            if (value is Single @single) return @single >= (Single)partial_value;
            if (value is long @long) return @long >= (long)partial_value;
            if (value is decimal @decimal) return @decimal >= (decimal)partial_value;
            if (value is short @short) return @short >= (short)partial_value;

            if (value is DateTime && partial_value is DateTime)
                return (DateTime)value >= (DateTime)partial_value;

            if (value is string @val && partial_value is string @part)
                return string.Compare(@val, @part) > 0 || val == @part;

            return false;
        }

        public static bool IsEQ(this object value, object partial_value)
        {
            if (value is int @int) return @int == (int)partial_value;
            if (value is double @double) return @double == (double)partial_value;
            if (value is Single @single) return @single == (Single)partial_value;
            if (value is long @long) return @long == (long)partial_value;
            if (value is decimal @decimal) return @decimal == (decimal)partial_value;
            if (value is short @short) return @short == (short)partial_value;

            if (value is DateTime && partial_value is DateTime)
                return (DateTime)value == (DateTime)partial_value;

            if (value is string @val && partial_value is string @part)
                return @val == @part;

            return false;
        }

        public static bool IsNE(this object value, object partial_value)
        {
            if (value is int @int) return @int != (int)partial_value;
            if (value is double @double) return @double != (double)partial_value;
            if (value is Single @single) return @single != (Single)partial_value;
            if (value is long @long) return @long != (long)partial_value;
            if (value is decimal @decimal) return @decimal != (decimal)partial_value;
            if (value is short @short) return @short != (short)partial_value;

            if (value is DateTime && partial_value is DateTime)
                return (DateTime)value != (DateTime)partial_value;

            if (value is string @val && partial_value is string @part)
                return @val != @part;

            return false;
        }


        public static object UnaryMinusValue(this object value)
        {

            if (value is int @int) return -@int;
            if (value is double @double) return -@double;
            if (value is Single @single) return -@single;
            if (value is long @long) return -@long;
            if (value is decimal @decimal) return -@decimal;
            if (value is short @short) return -@short;

            return value;
        }

        public static object AddValues(this object value, object partial_value)
        {
            if (value is int @int) return @int + (int)partial_value;
            if (value is double @double) return @double + (double)partial_value;
            if (value is Single @single) return single + (Single)partial_value;
            if (value is long @long) return @long + (long)partial_value;
            if (value is decimal @decimal) return @decimal + (decimal)partial_value;
            if (value is short @short) return @short + (short)partial_value;
            if (value is string @string) return @string + (string)partial_value;

            return value;
        }

        public static object SubtractValues(this object value, object partial_value)
        {
            if (value is int @int) return @int - (int)partial_value;
            if (value is double @double) return @double - (double)partial_value;
            if (value is Single @single) return @single - (Single)partial_value;
            if (value is long @long) return @long - (long)partial_value;
            if (value is decimal @decimal) return @decimal - (decimal)partial_value;
            if (value is short @short) return @short - (short)partial_value;
            if (value is string @string) return @string.Replace((string)partial_value, string.Empty);

            return value;
        }

        public static object UnaryPlusValue(this object value)
        {

            if (value is int @int) return Math.Abs(@int);
            if (value is double @double) return Math.Abs(@double);
            if (value is Single @single) return Math.Abs(@single);
            if (value is long @long) return Math.Abs(@long);
            if (value is decimal @decimal) return Math.Abs(@decimal);
            if (value is short @short) return Math.Abs(@short);

            return value;
        }

        public static object MultiplyValues(this object value, object partial_value)
        {
            if (value is int @int) return @int * (int)partial_value;
            if (value is double @double) return @double * (double)partial_value;
            if (value is Single @single) return @single * (Single)partial_value;
            if (value is long @long) return @long * (long)partial_value;
            if (value is decimal @decimal) return @decimal * (decimal)partial_value;
            if (value is short @short) return @short * (short)partial_value;

            return value;
        }

        public static object DivideValues(this object value, object partial_value)
        {
            if (value.IsUnary() && (int)value == 0)
                return 0;

            if (partial_value.IsUnary() && (int)partial_value == 0)
                return 0;

            if (value is int @int) return @int / (int)partial_value;
            if (value is double @double) return @double / (double)partial_value;
            if (value is Single @single) return @single / (Single)partial_value;
            if (value is long @long) return @long / (long)partial_value;
            if (value is decimal @decimal) return @decimal / (decimal)partial_value;
            if (value is short @short) return @short / (short)partial_value;

            return value;
        }

        public static object ModValues(this object value, object partial_value)
        {
            if (value.IsUnary() && (int)value == 0)
                return 0;

            if (partial_value.IsUnary() && (int)partial_value == 0)
                return 0;

            if (value is int @int) return @int % (int)partial_value;
            if (value is double @double) return @double % (double)partial_value;
            if (value is Single @single) return @single % (Single)partial_value;
            if (value is long @long) return @long % (long)partial_value;
            if (value is decimal @decimal) return @decimal % (decimal)partial_value;
            if (value is short @short) return @short % (short)partial_value;

            return value;
        }

    }
}
