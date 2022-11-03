using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public interface IParserService
    {

        public  ParserException Exception { get; set; }

        public bool LoadFile(string filename);
        public void RunScript(string script);
        public void Run(string filename);
        public void Run();


        public bool Compile(string filename);

    }
}
