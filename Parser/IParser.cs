using System;
using System.Collections.Generic;
using System.Text;

namespace Parser
{
    public interface IParser
    {
        public void Prescan();
        public string Run(string expression);
        public string RunScript(string filename);
    }
}
