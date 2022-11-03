using Iced.Intel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parser.Models;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

namespace Parser.Service
{
    public partial class ParserService : IParserService
    {
        private readonly IFileIOService _fileIO;

        public ParserService(IFileIOService fileIO)
        {
            _fileIO = fileIO;
        }

        private const string main = "main";

        private const int MAX_FUNCS = 1000;
        private const int MAX_GLOBAL_VARS = 1000;
        private const int MAX_LOCAL_VARS = 2000;
        private const int MAX_BLOCK = 1000;
        private const int MAX_ID_LEN = 80;
        private const int MAX_FUNC_CALLS = 1000;
        private const int MAX_PARAMS = 32;
        private const int MAX_LOOP_NEST = 1000;

        private enCommandType CommandType = enCommandType.None;

        private enTokenState TokenState = enTokenState.Start;

        private enTokenType TokenType = enTokenType.None;

        private List<CommandItem> _commands = new List<CommandItem>
        {
            new CommandItem("import", enCommandType.Import),
            new CommandItem("if", enCommandType.If),
            new CommandItem("else", enCommandType.Else),
            new CommandItem("for", enCommandType.For),
            new CommandItem("foreach", enCommandType.ForEach),
            new CommandItem("in", enCommandType.In),
            new CommandItem("do", enCommandType.Do),
            new CommandItem("while", enCommandType.While),
            new CommandItem("var", enCommandType.Var),
            new CommandItem("int", enCommandType.Int),
            new CommandItem("string", enCommandType.String),
            new CommandItem("bool", enCommandType.Bool),
            new CommandItem("datetime", enCommandType.DateTime),
            new CommandItem("decimal", enCommandType.Decimal),
            new CommandItem("json", enCommandType.Json),
            new CommandItem("return", enCommandType.Return),
            new CommandItem("end", enCommandType.End),
            new CommandItem("true", enCommandType.True),
            new CommandItem("false", enCommandType.False),
            new CommandItem("", enCommandType.End),
        };
        

       

        private string _source = string.Empty;
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
            }
        }

        #region Public Methods
        public ParserException? Exception { get; set; }

        public bool LoadFile(string filename)
        {
            Console.WriteLine($"Loading {filename}...");

            if (!_fileIO.ReadFile(filename))
            {
                Console.WriteLine("Filename not found.");
                return false;
            }

            if (string.IsNullOrEmpty(_fileIO.Contents))
            {
                Console.WriteLine("File was empty");
                return false;
            }

            _source = _fileIO.Contents;

            return true;

        }

        public bool Minify(string filename)
        {
            Reset();

            if (!LoadFile(filename)) return false;

            var temp = string.Empty;

            while(Pos < _source.Length)
            {
                GetToken();

                if (Token == sDQUOTE)
                {
                    temp += Token;
                    do { temp += Tok; Pos++; } while (Tok != DQUOTE);
                    GetToken();
                    temp += Token;
                }

                else if (TokenType == enTokenType.Keyword) temp += Token + " ";

                else if (Token == sAND || Token == sOR) temp += $" {Token} ";

                else temp += Token;
            }

            filename = filename.Replace(".cs", ".min.cs");

            _fileIO.WriteFile(filename, temp);

            return true;
        }

        public bool Compile(string filename)
        {
            Reset();

            if (!LoadFile(filename)) return false;

            Prescan();

            Reset();

            var temp = string.Empty;

            while (Pos < _source.Length)
            {
                GetToken();

                if(SkipComments()) continue;

                if(Token == "import")
                {
                    while(Tok != CR && Tok != LF) Pos++;
                    while(!IsWhiteSpace(CRLF)) Pos++;
                }

                else if (Token == sDQUOTE)
                {
                    temp += Token;
                    do { temp += Tok; Pos++; } while (Tok != DQUOTE);
                    GetToken();
                    temp += Token;
                }

                else if (TokenType == enTokenType.Keyword) temp += Token + " ";

                else if (Token == sAND || Token == sOR) temp += $" {Token} ";

                else temp += Token;
            }

            filename = filename.Replace(".cs", ".min.cs");

            _fileIO.WriteFile(filename, temp);

            return true;

        }

        public void RunScript(string script)
        {
            _source = script;

            Run();
        }

        public void Run(string filename)
        {
            if (LoadFile(filename)) Run();
        }

        public void Run()
        {
            if (string.IsNullOrEmpty(_source)) return;

            try
            {

                Console.WriteLine("Prescan\r\n");
                Prescan();
                Console.WriteLine("\r\n");

                lvartos = 0;
                functos = 0;

                Pos = FindFunc(main);

                if (Pos < 0) SyntaxError(enSyntaxError.NoFunc);

                Pos--;

                Token = main;

                Console.WriteLine("Start Execution\r\n");
                Call();
                Console.WriteLine("\r\n");

            }
            catch (ParserException ex)
            {
                Exception = ex;
                throw;
            }
            catch
            {
                SyntaxError(enSyntaxError.Syntax);
            }
        }

        #endregion

 


    }

}

