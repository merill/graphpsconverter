using GraphPsConverter.Core.Model;

namespace GraphPsConverter.Core
{
    internal class CommandMappingHelper
    {
        private static List<CommandMapping> _mappings;

        public static List<CommandMapping> GetCommandMappings()
        {
            if (_mappings == null)
            {
                _mappings = new List<CommandMapping>();
                var getazureaduser = new CommandMapping()
                {
                    AadName = "Get-AzureADUser",
                    GraphName = "Get-MgUser",
                    ParamMappings = new List<ParamMapping>() { }
                };
                getazureaduser.ParamMappings.Add(new ParamMapping()
                {
                    AadName = "-All",
                    GraphName = "-All"
                });
                getazureaduser.ParamMappings.Add(new ParamMapping()
                {
                    AadName = "-Top",
                    GraphName = "-Top"
                });
                getazureaduser.ParamMappings.Add(new ParamMapping()
                {
                    AadName = "-Filter",
                    GraphName = "-Filter"
                });
                getazureaduser.ParamMappings.Add(new ParamMapping()
                {
                    AadName = "-SearchString",
                    GraphName = "-Search"
                });
                _mappings.Add(getazureaduser);
            }
            return _mappings;
        }
    }
}
