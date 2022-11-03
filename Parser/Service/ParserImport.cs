using Parser.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Service
{
    public partial class ParserService
    {
        private List<string> _imports = new();

        private void Import()
        {
            string filename = string.Empty;

            if (_imports.Any(x => x == filename))
            {
                Console.WriteLine($"Skiping Duplicated Import {filename}");
                return;
            }

            GetToken();

            if (Token != sDQUOTE) SyntaxError(enSyntaxError.QuoteExpected);


            do
            {
                filename += Tok;
                Pos++;

            } while (Tok != DQUOTE);

            GetToken();
            GetToken();

            if (Token != sSEMI_COLON) SyntaxError(enSyntaxError.SemiExpected);

            if (!_fileIO.ReadFile(filename)) SyntaxError(enSyntaxError.ImportNotFound, filename);

            if (string.IsNullOrEmpty(_fileIO.Contents)) SyntaxError(enSyntaxError.ImportEmpty, filename);

            Console.WriteLine($"Imported {filename}");

            _imports.Add(filename);

            _source += _fileIO.Contents;

        }
    }
}
