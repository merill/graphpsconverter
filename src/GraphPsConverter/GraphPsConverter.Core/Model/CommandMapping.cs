using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPsConverter.Core.Model
{
    public class CommandMapping
    {
        public string AadName { get; set; }
        public string GraphName { get; set; }
        public List<ParamMapping> ParamMappings{ get; set;}

        public CommandMapping()
        {
            ParamMappings = new List<ParamMapping>();
        }
    }
}
