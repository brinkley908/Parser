using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Parser.Service
{
    public class FileIOService : IFileIOService
    {
        public string? Contents { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public bool ReadFile(string filename)
        {
            if (!File.Exists(filename)) return false;

            try
            {
                using StreamReader sr = new(filename);

                Contents = sr.ReadToEnd();

                return true;
            }
            catch (Exception ex)
            {
               ErrorMessage = ex.Message;
            }

            return false;
        }

        public bool WriteFile(string filename, string contents)
        {
            try
            {

                if (File.Exists(filename))
                    File.Delete(filename);

                using StreamWriter sw = new(filename);

                sw.Write(contents);

                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
