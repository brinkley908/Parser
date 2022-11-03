using Parser.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parser.Models
{
    public class Identifier
    {
        public string Name { get; set; } = string.Empty;
        public enCommandType CommandType { get; set; } = enCommandType.None;
        public object? Value { get; set; } = null;

        public enInternalFunction InternalFunction { get; set; } = enInternalFunction.None;
    }

}
