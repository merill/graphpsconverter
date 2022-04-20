using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Text;
using System.Threading.Tasks;

namespace GraphPsConverter.Core.Model
{
    public class ConvertedCommand
    {
        internal CommandAst SourceCommandAst { get; set; }
        public string ConvertedScript { get; internal set; }
        public CommandMapping CommandMapping { get; internal set; }

    }
}
