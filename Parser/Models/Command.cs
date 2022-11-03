using System;
using System.Collections.Generic;
using System.Text;

namespace Parser.Models
{


    public class CommandItem
    {
        public CommandItem() { }
        public CommandItem(string command, enCommandType token)
        {
            Command = command;
            Token = token;
        }

        public string Command { get; set; } = string.Empty;
        public enCommandType Token { get; set; } = enCommandType.None;
    }
}
