using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public interface IFileIOService
    {
        public string? Contents { get; set; }
        public string ErrorMessage { get; set; }
        public bool ReadFile(string filename);
        public bool WriteFile(string filename, string contents);
    }
}
