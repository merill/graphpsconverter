using Microsoft.Management.Infrastructure.Generic;
using System.Management.Automation.Language;
using System.Text;

namespace GraphPsConverter.Core.Model
{
    public class ConvertedCommand
    {
        public CommandAst CommandAst { get; private set; }

        public string? ConvertedScript { get; private set; }

        public bool HasGraphCommand { get; private set; }

        public CommandMap CommandMap { get; private set; }

        public string AadCmdName { get; private set; }
        public string GraphCmdName { get; private set; }

        public string AadCmdDocLink { 
            get 
            {
                return string.Format("https://docs.microsoft.com/en-us/powershell/module/{0}/{1}", CommandMap.AadModuleName, CommandMap.AadCmdName);
            } 
        }

        public string GraphCmdDocLink { 
            get
            {
                return HasGraphCommand 
                    ? string.Format("https://docs.microsoft.com/en-us/powershell/module/{0}/{1}", CommandMap.GraphModuleName, CommandMap.GraphCmdName) 
                    : "https://docs.microsoft.com/en-us/powershell/microsoftgraph/azuread-msoline-cmdlet-map";
            }
        }

        public string GraphCmdDisplayName
        {
            get
            {
                return HasGraphCommand ? GraphCmdName : "This command is not yet available in Microsoft Graph";
            }
        }
        public ConvertedCommand(CommandAst sourceCommandAst)
        {
            CommandAst = sourceCommandAst;
            List<Ast> children = InitAst();

            ConvertedScript = GetConvertedScript(children);
        }
        
        public string? GetConvertedScript(List<Ast> children)
        {
            if (!HasGraphCommand) return null;
            var convertedScript = new StringBuilder();

            var commandMap = CommandMappingHelper.GetCommandMap(AadCmdName);

            if(!string.IsNullOrEmpty(commandMap.GraphModuleName))
            {
                convertedScript.AppendFormat("Import-Module {0}\r\n", commandMap.GraphModuleName);
            }
            convertedScript.Append(GraphCmdName);

            for (var index = 1; index < children.Count; index++)
            {
                var child = children[index];

                if (child is CommandParameterAst)
                {
                    var aadParamName = child.ToString();                    
                    var paramMap = CommandMappingHelper.GetParamMap(AadCmdName, aadParamName);

                    var graphParamName = paramMap.GraphParamName;

                    if (!string.IsNullOrEmpty(graphParamName))
                    {
                        convertedScript.AppendFormat(" -{0}", graphParamName);
                    }
                    //Append all the param values until we get to the next command
                    while (index + 1 < children.Count && children[index + 1] is not CommandParameterAst)
                    {
                        index++;
                        var paramValue = children[index].ToString();
                        convertedScript.AppendFormat(" {0}", paramValue);
                    }
                }
            }

            return convertedScript.ToString();

        }

        private List<Ast> InitAst()
        {
            //Get the command and it's parameters as a list
            List<Ast> children = CommandAst.FindAll(e => e.Parent == CommandAst, true).ToList();

            AadCmdName = children[0].ToString();
            CommandMap = CommandMappingHelper.GetCommandMap(AadCmdName);
            GraphCmdName = CommandMap.GraphCmdName;

            HasGraphCommand = !string.IsNullOrEmpty(GraphCmdName);
            return children;
        }
    }
}
