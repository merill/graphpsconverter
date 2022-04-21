using System.Management.Automation.Language;
using System.Text;

namespace GraphPsConverter.Core.Model
{
    public class ConvertedCommand
    {
        private CommandAst _sourceCommandAst { get; set; }

        public string? ConvertedScript { get; private set; }

        public bool HasGraphCommand { get; private set; }

        public ConvertedCommand(CommandAst sourceCommandAst)
        {
            _sourceCommandAst = sourceCommandAst;

            ConvertedScript = GetConvertedScript();
        }
        
        public string? GetConvertedScript()
        {
            //Get the command and it's parameters as a list
            List<Ast> children = _sourceCommandAst.FindAll(e => e.Parent == _sourceCommandAst, true).ToList();

            var aadCmdName = children[0].ToString();
            var graphCmdName = CommandMappingHelper.GetGraphCommand(aadCmdName);
            HasGraphCommand = !string.IsNullOrEmpty(graphCmdName);

            if (!HasGraphCommand) return null;

            var convertedScript = new StringBuilder(graphCmdName);

            for (var index = 1; index < children.Count; index++)
            {
                var child = children[index];

                if (child is CommandParameterAst)
                {
                    var aadParamName = child.ToString();
                    var graphParamName = CommandMappingHelper.GetGraphParam(aadCmdName, aadParamName);

                    if (!string.IsNullOrEmpty(graphParamName))
                    {
                        convertedScript.AppendFormat(" -{0}", graphParamName);
                    }
                    //Append all the param values until we get to the next command
                    while(index + 1 < children.Count && children[index + 1] is not CommandParameterAst)
                    {
                        index++;
                        var paramValue = children[index].ToString();
                        convertedScript.AppendFormat(" {0}", paramValue);
                    }
                }
            }

            return convertedScript.ToString();

        }
    }
}
