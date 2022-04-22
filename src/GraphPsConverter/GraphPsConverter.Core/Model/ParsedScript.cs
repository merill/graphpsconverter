using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace GraphPsConverter.Core.Model
{
    public class ParsedScript
    {
        public string SourceScript { get; internal set; }
        public bool IsValid { get; internal set; }
        public string ConvertedScript { get; internal set; }
        public List<ConvertedCommand> ConvertedCommands { get; internal set; }
        public List<ConverterParseError> Errors { get; set; }
        public List<CommandAst> Commands { get; internal set; }


        public ParsedScript():this(null)
        {

        }
        public ParsedScript(string sourceScript)
        {
            SourceScript = sourceScript;
            Errors = new List<ConverterParseError>();
            ConvertedCommands = new List<ConvertedCommand>();
            IsValid = false;
            ConvertedScript = string.Empty;
        }
    }
}
