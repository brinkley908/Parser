using BenchmarkDotNet.Toolchains.CsProj;
using CommandLine;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using Parser.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Parser
{

    

    public class xParser
    {


        private const int MAX_FUNCS = 1000;
        private const int MAX_GLOBAL_VARS = 1000;
        private const int MAX_LOCAL_VARS = 2000;
        private const int MAX_BLOCK = 1000;
        private const int MAX_ID_LEN = 80;
        private const int MAX_FUNC_CALLS = 1000;
        private const int MAX_PARAMS = 32;
        private const int MAX_LOOP_NEST = 1000;


        protected readonly string main = "main";
        protected readonly string CRLF = "\r\n";
        protected readonly char CR = '\r';
        protected readonly char LF = '\n';
        protected readonly string _cnull = "\0";

        protected readonly char SingleQuote = '\'';
        protected readonly char DoubleQuote = '"';

        private const string EQ = "==";
        private const string NE = "!=";
        private const string LE = "<==";
        private const string LT = "<";
        private const string GE = ">=";
        private const string GT = ">";
        private const string AND = "&&";
        private const string OR = "||";
        private string[] RELOPS = new string[] { LT, LE, GT, GE, EQ, NE };

        protected string _code = string.Empty;
        protected unsafe char* prog;
        unsafe char* p_buf;
        unsafe char* cnull;
        protected char[] token = new char[80];
        // protected string _token = string.Empty;
        protected tokens tok;
        protected tok_type token_type = tok_type.NONE;

        protected int lvartos = 0;
        protected int functos = 0;
        protected object ret_value = 0;

        protected List<VarType> local_vars = new();
        protected List<VarType> global_vars = new();
        protected List<FuncItem> funcs = new();

        protected int[] call_stack = new int[MAX_FUNCS];
        protected VarType[] local_var_stack = new VarType[MAX_LOCAL_VARS];

        protected List<CommandItem> commands = new List<CommandItem>
        {
            new CommandItem("if", tokens.IF),
            new CommandItem("else", tokens.ELSE),
            new CommandItem("for", tokens.FOR),
            new CommandItem("do", tokens.DO),
            new CommandItem("while", tokens.WHILE),
            new CommandItem("char", tokens.CHAR),
            new CommandItem("int", tokens.INT),
            new CommandItem("return", tokens.RETURN),
            new CommandItem("end", tokens.END),
            new CommandItem("", tokens.END),
        };

        private List<VarType> internal_funcs = new()
        {
            new VarType{ VarName="consolelog", InternalFunc = intern_func.CONSOLELOG },
            new VarType{ VarName="printf", InternalFunc = intern_func.PRINTF }
        };


        private static string[] _errorMessages = new string[] 
        {  
            "syntax error",
            "unbalanced paranthesis",
            "unbalanced braces",
            "semicolon expected",
            "paranthesis expected",
            "equal sign expected",
            "type specifier expected",
            "while expected",
            "closing quote expected",
            "not a string",
            "too many local variables",
            "expression missing",
            "not a variable",
            "parameter error",
            "function nor defined",
            "too many nested functions",
            "return without a call"
        };


        #region Public Methods

        public bool LoadFile(string filename)
        {
            Console.WriteLine($"Loading {filename}...");

            if (!File.Exists(filename))
            {
                Console.WriteLine("Filename not found.");
                return false;
            }

            try
            {
                using StreamReader sr = new(filename);

                var data = sr.ReadToEnd();

                _code = data;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {filename} - {ex.Message}");
            }

            return false;
        }

        public void RunScript(string script)
        {
            _code = script;
            Run();
        }

        public void Run(string filename)
        {
            if (LoadFile(filename))
                Run();
        }

        public unsafe void Run()
        {
            if (string.IsNullOrEmpty(_code))
                return;


            fixed (char* cn = _cnull)
            {
                cnull = cn;

                fixed (char* t = _code)
                {
                    p_buf = t;
                    prog = p_buf;

                    Prescan();

                    lvartos = 0;
                    functos = 0;

                    prog = find_func(main);
                    prog--;
                    main.Copy(ref token);

                    Call();
                }
            }

        }

        #endregion

        #region GetToken
        protected unsafe tok_type GetToken()
        {
            fixed (char* t = token)
            {
                char* temp = t;
                token_type = tok_type.DELIMETER;
                tok = tokens.NONE;
                bool hasToken = false;

                while (IsWhiteSpace(*prog) && (prog != null || *prog != '\0')) ++prog;

                if (*prog == '\r' || *prog == '\n')
                {
                    prog += 2;
                    while ((IsWhiteSpace(*prog) || *prog=='\r' || *prog=='\n') && (prog != null || *prog != '\0')) ++prog;
                }

                if (*prog == '\0' || prog == null)
                {
                    tok = tokens.FINISHED;
                    token_type = tok_type.DELIMETER;
                    return token_type;
                }

                if ("{}".Contains(*prog))
                {
                    *temp = *prog;
                    temp++;
                    *temp = '\0';
                    prog++;
                    token_type = tok_type.BLOCK;
                    return token_type;
                }

                if (*prog == '/')
                {
                    if (*(prog + 1) == '*')
                    {
                        prog += 2;
                        do
                        {
                            while (*prog != '*') prog++;
                            prog++;
                        } while (*prog != '/');
                        prog++;
                    }

                    if(*(prog + 1) == '/')
                    {
                        do
                        {
                            prog++;
                        } while (*prog != '\r');
                        prog++;
                        if (*prog == '\n') prog++;
                    }
                }

                if ("!<>=".Contains(*prog))
                {
                    hasToken = false;
                    switch (*prog)
                    {
                        case '=':
                            {
                                if(*(prog+1) == '=')
                                {
                                    prog++; prog++;
                                    *temp = '=';
                                    temp++;
                                    *temp = '=';
                                    temp++;
                                    *temp = '\0';
                                    hasToken = true;
                                }

                                break;
                            }

                        case '!':
                            {
                                if (*(prog + 1) == '=')
                                {
                                    prog++; prog++;
                                    *temp = '!';
                                    temp++;
                                    *temp = '=';
                                    temp++;
                                    *temp = '\0';
                                    hasToken = true;
                                }

                                break;
                            }
                        case '<':
                            {
                                if (*(prog + 1) == '=')
                                {
                                    prog++; prog++;
                                    *temp = '<';
                                    temp++;
                                    *temp = '=';
                                }
                                else
                                {
                                    prog++;
                                    *temp = '<';
                                }
                                temp++;
                                *temp = '\0';
                                hasToken = true;

                                break;
                            }
                        case '>':
                            {
                                if (*(prog + 1) == '=')
                                {
                                    prog++; prog++;
                                    *temp = '>';
                                    temp++;
                                    *temp = '=';
                                }
                                else
                                {
                                    prog++;
                                    *temp = '>';
                                }
                                temp++;
                                *temp = '\0';
                                hasToken = true;

                                break;
                            }
                    }

                    if (hasToken)
                    {
                        token_type = tok_type.DELIMETER;
                        return token_type;
                    }
                }

                if ("+-*^/%=;(),'".Contains(*prog))
                {
                    *temp = *prog;
                    prog++;
                    temp++;
                    *temp = '\0';

                    token_type = tok_type.DELIMETER;
                    return token_type;
                }

                if (*prog == '"')
                {
                    prog++;
                    while(*prog != '"' && *prog != '\r') *temp++ = *prog++;
                   
                    if (*prog == '\r') SyntaxError(error_msg.SYNTAX);

                    prog++;
                    *temp = '\0';

                    token_type = tok_type.STRING;
                    return token_type;
                }

                if (IsDigit(*prog))
                {
                    while (!IsDelim(*prog)) *temp++ = *prog++;
                    *temp = '\0';

                    token_type = tok_type.NUMBER;
                    return token_type;
                }

                if (IsAlpha(*prog))
                {
                    while (!IsDelim(*prog))  *temp++ = *prog++;
                    *temp = '\0';

                    token_type = tok_type.TEMP;
                }


                *temp = '\0';

                if(token_type == tok_type.TEMP)
                {
                    tok = look_up();
                    if (tok != tokens.NONE) token_type = tok_type.KEYWORD;
                    else token_type = tok_type.IDENTIFIER;
                }

                return token_type;
            }
        }
        private tokens look_up() => commands.FirstOrDefault(x => x.Command.Equals(token.TokenValue(), StringComparison.CurrentCultureIgnoreCase))?.Token ?? tokens.NONE;

        private bool IsWhiteSpace(char c)
        {
            if (c == ' ' || c == '\t') return true;
            return false;
        }

        private bool IsDigit(char c) => "0123456789".Contains(c);
        private bool IsAlpha(char c) => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".Contains(c);
        private bool IsDelim(char c) => " !;,+-<>'/*%/^=()".Contains(c) || c == 9 || c == '\r' || c == 0;

        private unsafe void PutBack()
        {
            var t = token.TokenValue();

            for (var i = 0; i < t.Length; ++i) prog--;
        }
        #endregion

        #region Parser Core

        private unsafe void Prescan()
        {
            char* p = prog;
            char[] temp = new char[MAX_ID_LEN];
            int brace = 0;

            fixed (char* t = token)
            {

                do
                {

                    while (brace > 0)
                    {
                        GetToken();

                        if (*t == '{') brace++;
                        if (*t == '}') brace--;
                    }

                    GetToken();

                    if(tok == tokens.CHAR || tok == tokens.INT)
                    {
                        PutBack();
                        decl_global();
                    }

                    else if(token_type == tok_type.IDENTIFIER)
                    {
                        token.Copy(ref temp);

                        GetToken();

                        if(*t == '(')
                        {
                            funcs.Add(new FuncItem
                            {
                                Name = temp.TokenValue(),
                                Location = prog
                            });

                            while (*prog != ')') prog++;
                            prog++;
                        }
                        else
                            PutBack();

                    }
                    else if(*t == '{') brace++;

                } while (tok != tokens.FINISHED);

                prog = p;
            }
        }

        private unsafe void Call()
        {
            char* loc;
            char* temp;
            int lvartemp = lvartos;

            loc = find_func(token.TokenValue());

            if (loc == cnull)
                SyntaxError(error_msg.FUNC_UNDEF);

            get_args();

            temp = prog;

            func_push(lvartemp);

            prog = loc;

            get_params();

            interp_block();

            prog = temp;
            lvartos = func_pop();

        }

        private void interp_block()
        {
            object value = 0;

            int block = 0;

            do
            {

                token_type = GetToken();

                if(token_type == tok_type.IDENTIFIER)
                {
                    PutBack();
                    eval_exp(ref value);
                    if (token[0] != ';') SyntaxError(error_msg.SEMI_EXPECTED);
                }
                else if(token_type == tok_type.BLOCK)
                {
                    if (token[0] == '{') block = 1;
                    else return;
                }
                else
                {
                    switch (tok)
                    {
                        case tokens.CHAR:
                        case tokens.INT:
                            {
                                PutBack();
                                decl_local();
                                break;
                            }
                        case tokens.RETURN:
                            {
                                func_ret();
                                break;
                            }
                        case tokens.FOR:
                            {
                                exec_for();
                                break;
                            }
                    }
                }
                    


            } while (tok != tokens.FINISHED && block > 0);
        }

        private void find_eob()
        {
            int brace;

            GetToken();

            brace = 1;
            do
            {

                GetToken();
                if (token[0] == '{') brace++;
                if (token[0] == '}') brace--;
                        
            } while(brace> 0);
        }

        private unsafe void exec_for()
        {
            char* temp;
            char* temp2;
            object cond = false;
            int brace;

            GetToken();
            eval_exp(ref cond);

            if (token[0] != ';') SyntaxError(error_msg.SEMI_EXPECTED);

            prog++;

            temp = prog;

            for(; ; )
            {
                eval_exp(ref cond);
                if (token[0] != ';') SyntaxError(error_msg.SEMI_EXPECTED);

                prog++;
                temp2 = prog;

                brace = 1;

                while(brace> 0)
                {
                    GetToken();
                    if (token[0] == '(') brace++;
                    if (token[0] == ')') brace--;
                }

                if((bool)cond) interp_block();
                else
                {
                    find_eob();
                    return;
                }

                prog = temp2;
                eval_exp(ref cond);

                prog = temp;
            }
        }

        #endregion

        #region Evaluation

        private unsafe void eval_exp(ref object value)
        {
            GetToken();

            fixed (char* t = token)
            {
                if (t == cnull)
                    SyntaxError(error_msg.NO_EXP);

                if (*t == ';')
                {
                    value = 0;
                    return;
                }

                eval_exp0(ref value);

                PutBack();
            }

        }


        private unsafe void eval_exp0(ref object value)
        {
            char[] temp = new char[MAX_ID_LEN];
            tok_type temp_tok; 

            if(token_type == tok_type.IDENTIFIER)
            {
                if (is_var(token.TokenValue()))
                {
                    token.Copy(ref temp);
                    temp_tok = token_type;

                    GetToken();

                    fixed (char *t = token)
                    {
                        if(*t == '=')
                        {
                            GetToken();
                            eval_exp0(ref value);
                            assign_var(temp.TokenValue(), value);
                            return;
                        }
                        else
                        {
                            PutBack();
                            temp.Copy(ref token);
                            token_type = temp_tok;

                        }
                    }
                }

            }

            eval_exp1(ref value);
        }

        private unsafe void eval_exp1(ref object value)
        {
            object partial_value = 0;

            eval_exp2(ref value);

            var op = token.TokenValue();

            if (!RELOPS.Contains(op))
                return;

            GetToken();
            eval_exp2(ref partial_value);

            switch (op)
            {
                case LT:
                    {
                        value = IsLT(value, partial_value);
                        break;
                    }
                case LE:
                    {
                        value = IsLE(value, partial_value);
                        break;
                    }
                case GT:
                    {
                        value = IsGT(value, partial_value);
                        break;
                    }
                case GE:
                    {
                        value = IsGE(value, partial_value);
                        break;
                    }
                case EQ:
                    {
                        value = IsEQ(value, partial_value);
                        break;
                    }

                case NE:
                    {
                        value = IsNE(value, partial_value);
                        break;
                    }
            }
        }

        private unsafe void eval_exp2(ref object value)
        {
            object partial_value = 0;

            eval_exp3(ref value);


            fixed (char* t = token)
            {
                char op = *t;

                while (op == '+' || op == '-' )
                {
                    GetToken();

                    eval_exp3(ref partial_value);

                    switch (op)
                    {
                        case '+':
                            {
                                value = AddValues(value, partial_value);
                                break;
                            }
                        case '-':
                            {
                                value = SubtractValues(value, partial_value);
                                break;
                            }
                    }

                    op = *t;
                }
            }

        }

        private unsafe void eval_exp3(ref object value)
        {
            object partial_value = 0;

            eval_exp4(ref value);

            fixed(char *t = token)
            {
                char op = *t;

                while(op == '*' || op=='/' || op == '%')
                {
                    GetToken();

                    eval_exp4(ref partial_value);
                    switch (op)
                    {
                        case '*':
                            {
                                value = MultiplyValues(value, partial_value);
                                break;
                            }
                        case '/':
                            {
                                value = DivideValues(value, partial_value);
                                break;
                            }
                        case '%':
                            {
                                value = ModValues(value, partial_value);
                                break;
                            }
                    }

                    op = *t;
                }
            }

        }

        private unsafe void eval_exp4(ref object value)
        {
            char op = '\0';

            fixed (char *t = token)
            {
                if(*t == '+' || *t == '-')
                {
                    op = *t;
                    GetToken();
                }

                eval_exp5(ref value);

                if (op != '\0')
                {
                    if (op == '-') value = UnaryMinusValue(value);
                    if (op == '+') value = UnaryPlusValue(value);
                }
            }

        }

        private unsafe void eval_exp5(ref object value)
        {
            fixed (char* t = token)
            {
                if(*t == '(')
                {
                    GetToken();
                    eval_exp0(ref value);
                    if(*t != ')') 
                        SyntaxError(error_msg.PAREN_EXPECTED);

                    GetToken();
                    return;
                }

                atom(ref value);
            }
        }

        private  object UnaryMinusValue(object value)
        {

            if (value is int @int) return -@int;
            if (value is double @double) return -@double;
            if (value is Single @single) return -@single;
            if (value is long @long) return -@long;
            if (value is decimal @decimal) return -@decimal;
            if (value is short @short) return -@short;

            return value;
        }

        private object UnaryPlusValue(object value)
        {

            if (value is int @int) return Math.Abs(@int);
            if (value is double @double) return Math.Abs(@double);
            if (value is Single @single) return Math.Abs(@single);
            if (value is long @long) return Math.Abs(@long);
            if (value is decimal @decimal) return Math.Abs(@decimal);
            if (value is short @short) return Math.Abs(@short);

            return value;
        }

        private object AddValues(object value, object partial_value)
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

        private object SubtractValues(object value, object partial_value)
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


        private object MultiplyValues(object value, object partial_value)
        {
            if (value is int @int) return @int * (int)partial_value;
            if (value is double @double) return @double * (double)partial_value;
            if (value is Single @single) return @single * (Single)partial_value;
            if (value is long @long) return @long * (long)partial_value;
            if (value is decimal @decimal) return @decimal * (decimal)partial_value;
            if (value is short @short) return @short * (short)partial_value;

            return value;
        }

        private object DivideValues(object value, object partial_value)
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

        private object ModValues(object value, object partial_value)
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

        private bool IsLT(object value, object partial_value)
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

        private bool IsLE(object value, object partial_value)
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


        private bool IsGT(object value, object partial_value)
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

        private bool IsGE(object value, object partial_value)
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

        private bool IsEQ(object value, object partial_value)
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

        private bool IsNE(object value, object partial_value)
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

        private unsafe void atom(ref object value)
        {
            switch (token_type)
            {
                case tok_type.IDENTIFIER:
                    {
                        var ifunc = GetInternalFunc(token.TokenValue());
                        if (ifunc != intern_func.NONE)
                        {
                            value = CallInternalFunc(ifunc);
                            return;
                        }

                        if (find_func(token.TokenValue()) != cnull)
                        {
                            Call();
                            value = ret_value;
                            return;
                        }

                        value = find_var(token.TokenValue());
                        GetToken();
                        return;
                    }
                case tok_type.NUMBER:
                    {
                        var number = token.TokenValue();
                        value = 0;

                        if(int.TryParse(number, out int num))
                           value = num;

                        GetToken();
                        return;
                    }

                case tok_type.DELIMETER:
                    {

                        parseString(value);
                            

                        return;
                    }
                default:
                    {
                        if (token[0] == ')') return;

                        break;
                    }
            }

            SyntaxError(error_msg.SYNTAX);

        }

        private unsafe bool parseString(object value)
        {
            if (!(token[0] == SingleQuote || token[0] == DoubleQuote))
                return false;

            var delim = token[0] == SingleQuote ? SingleQuote : DoubleQuote;

            if (delim == SingleQuote)
            {

                value = *prog;
                prog++;
                if(*prog != SingleQuote) SyntaxError(error_msg.QUOTE_EXPECTED);

                prog++;
                GetToken();
                return true;

            }

            string sValue = "";

            while (*prog != DoubleQuote && *prog != '\0')
            {
                sValue += *prog;
                prog++;
            }

            if (*prog != DoubleQuote) SyntaxError(error_msg.QUOTE_EXPECTED);
            prog++;
            GetToken();

            return true;
        }


        #endregion

        #region Internal functions
        private int v = 0;
        private object CallInternalFunc(intern_func func)
        {
            switch (func)
            {
                case intern_func.PRINTF: return printf();
                default: return 0;
            }

        }

        private int printf()
        {

            object value = 0;
            GetToken();

            if (token[0] != '(') SyntaxError(error_msg.PAREN_EXPECTED);

            GetToken();

            if (token_type == tok_type.STRING)
            {
                Console.WriteLine(token.TokenValue());
            }
            else
            {
                PutBack();
                eval_exp(ref value);
                Console.WriteLine(value);
            }

            GetToken();

            if (token[0] != ')') SyntaxError(error_msg.PAREN_EXPECTED);

            GetToken();

            if (token[0] != ';') SyntaxError(error_msg.SEMI_EXPECTED);

            PutBack();

            return 0;
        }

        #endregion

        #region Arguments, Variables & Identifiers

        private unsafe void get_args()
        {
            object value = 0; 
            object[] temp = new object[MAX_PARAMS];
            int count = 0;

            VarType variable = new();

            GetToken();

            fixed(char *t = token)
            {
                if(*t != '(')
                    SyntaxError(error_msg.PAREN_EXPECTED);

                do
                {
                    eval_exp(ref value);

                    temp[count] = value;

                    GetToken();

                    count++;

                } while (*t == ',');

                count--;

                for(; count > 0; --count)
                {
                    variable.Value = temp[count];
                    variable.Type = tokens.ARG;
                    local_push(variable);
                }

            }

        }

        private unsafe void get_params()
        {
            VarType p;
            int i = lvartos - 1;

            fixed (char* t = token)
            {
                do
                {
                    GetToken();

                    if (*t == ')') break;

                    p = local_var_stack[i];

                    if (tok != tokens.INT && tok != tokens.CHAR) SyntaxError(error_msg.TYPE_EXPECTED);

                    p.Type = tok;

                    GetToken();

                    p.VarName = token.TokenValue();

                    GetToken();

                    i--;

                } while (*t != ',');

                if (*t != ')') SyntaxError(error_msg.PAREN_EXPECTED);
            }

        }

        private void local_push(VarType variable)
        {
            if (lvartos > MAX_LOCAL_VARS) SyntaxError(error_msg.TOO_MANY_LVARS);

            local_var_stack[lvartos] = variable;
            lvartos++;
        }

        private void func_push(int i)
        {
            if (functos > MAX_FUNCS) SyntaxError(error_msg.NEST_FUNC);

            call_stack[functos] = i;

            functos++;
        }

        private int func_pop()
        {
            functos--;
            if(functos <0) SyntaxError(error_msg.RET_NOCALL);
            return call_stack[functos];
        }

        private intern_func GetInternalFunc(string s) => internal_funcs.FirstOrDefault(x => x.VarName.Equals(s, StringComparison.CurrentCultureIgnoreCase))?.InternalFunc ?? Models.intern_func.NONE;
        
        private object? find_var(string s)
        {

            for (int i = lvartos - 1; i >= call_stack[functos - 1]; i--)
                if (local_var_stack[i].VarName == s)
                    return local_var_stack[i].Value;


            var item = global_vars.FirstOrDefault(x => x.VarName.Equals(s, StringComparison.CurrentCultureIgnoreCase));
            if (item != null)
                return item.Value;

            SyntaxError(error_msg.NOT_VAR);

            return null;

        }


        public unsafe char* find_func(string name)
        {
            var ret = funcs.FirstOrDefault(f => f.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (ret != null)
                return ret.Location;

            return cnull;
        }

        private unsafe void decl_global()
        {

            var toktype = tok;

            fixed (char* t = token)
            {
                GetToken();

                do
                {
                    GetToken();
                    global_vars.Add(new VarType
                    {
                        Type = toktype,
                        Value = null,
                        VarName = token.TokenValue()
                    });

                    GetToken();

                } while (*t == ',');

                if (*t != ';') SyntaxError(error_msg.SEMI_EXPECTED);
            }

        }

        private void decl_local()
        {

            GetToken();

            
            do
            {
                VarType variable = new()
                {
                    Type = tok,
                    Value = 0
                };


                GetToken();

                variable.VarName = token.TokenValue();

                local_push(variable);

                GetToken();
            } while (token[0] == ',');

            if (token[0] != ';') SyntaxError(error_msg.SEMI_EXPECTED);

        }

        private void func_ret()
        {
            object value = 0;

            eval_exp(ref value);

            ret_value = value;
        }

        private void assign_var(string name, object value)
        {
            int i;

            for(i=lvartos-1; i>= call_stack[functos-1]; i--)
                if (local_var_stack[i].VarName == name)
                {
                    local_var_stack[i].Value = value;
                    return;
                }

            if(i < call_stack[functos-1])
                for(i=0; i< MAX_GLOBAL_VARS; i++)
                    if (global_vars[i].VarName == name)
                    {
                        global_vars[i].Value = value;
                        return;
                    }

            SyntaxError(error_msg.NOT_VAR);

        }

        private bool is_var(string name)
        {
            int i;

            for (i = lvartos - 1; i >= call_stack[functos - 1]; i--)
                if (local_var_stack[i].VarName == name)
                    return true;

            for (i = 0; i < global_vars.Count; i++)
                if (global_vars[i].VarName == name)
                    return true;


            return false;
        }

        #endregion

        #region Error Handling

        protected unsafe string SyntaxError(error_msg error)
        {

            char* p = p_buf;
            int linecount = 0;

            while(p != prog)
            {
                p++;
                if (*p == '\r') linecount++;
            }

            string msg;
            string line;
            var lines = _code.Split("\r");
            if (linecount < lines.Length)
                line = lines[linecount];
            else
                line = lines[linecount - 1];

            if ((int)error >= _errorMessages.Length)
                msg = _errorMessages[0];
            else
                msg = _errorMessages[(int)error];


            Console.WriteLine($"{msg}{CRLF};line {linecount}{CRLF}{line}");
            //  throw new Exception($"{msg}{CRLF};line {linecount}{CRLF}{line}");
            return "";
        }

        #endregion

    }
}
