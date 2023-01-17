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

        public string GraphApiUri { get; private set; }
        public string GraphApiDocUri { get; private set; }
        public string GraphCmdScope
        {
            get
            {
                var scopes = CommandMap.GraphCmdScope;
                if(string.IsNullOrEmpty(scopes)) return null;

                return scopes.Split(",")[0];
            }
        }
        public string AadCmdDocLink { 
            get 
            {
                return string.Format("https://learn.microsoft.com/en-us/powershell/module/{0}/{1}", CommandMap.AadModuleName, CommandMap.AadCmdName);
            } 
        }

        public string GraphCmdDocLink { 
            get
            {
                return HasGraphCommand 
                    ? string.Format("https://learn.microsoft.com/en-us/powershell/module/{0}/{1}", CommandMap.GraphModuleName, CommandMap.GraphCmdName) 
                    : "https://learn.microsoft.com/en-us/powershell/microsoftgraph/azuread-msoline-cmdlet-map";
            }
        }


        public string GraphCmdDisplayName
        {
            get
            {
                return HasGraphCommand ? GraphCmdName : $"A Graph PowerShell command for {AadCmdName} is not yet available.";
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

            if (!string.IsNullOrEmpty(GraphCmdScope))
            {
                convertedScript.AppendFormat("Connect-Graph {0}\r\n", GraphCmdScope);
            }

            convertedScript.Append(GraphCmdName);

            for (var index = 1; index < children.Count; index++)
            {
                var child = children[index];

                if (child is CommandParameterAst)
                {
                    var aadParamName = child.ToString();                    
                    var paramMap = CommandMappingHelper.GetParamMap(AadCmdName, aadParamName);

                    var graphParamName = paramMap == null ? null : paramMap.GraphParamName;

                    if (!string.IsNullOrEmpty(graphParamName))
                    {
                        convertedScript.AppendFormat(" -{0}", graphParamName);
                    }
                    //Append all the param values until we get to the next command
                    while (index + 1 < children.Count && children[index + 1] is not CommandParameterAst)
                    {
                        index++;
                        if (!string.IsNullOrEmpty(graphParamName)) //Don't include param values if we don't have a corresponding param in Graph
                        {
                            var paramValue = children[index].ToString();
                            convertedScript.AppendFormat(" {0}", paramValue);
                        }
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
            GraphApiUri = CommandMap.GraphUri;
            GraphApiDocUri = CommandMappingHelper.GetGraphApiDocUri(AadCmdName);
            return children;
        }
    }
}
