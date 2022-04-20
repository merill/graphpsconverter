using GraphPsConverter.Core.Model;
using System.Diagnostics;
using System.Management.Automation.Language;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("GraphPsConverter.Tests")]
namespace GraphPsConverter.Core
{
    public class Converter
    {
        private ParsedScript _parsedScript;

        public ParsedScript ConvertToGraphPowerShell(string sourceScript)
        {
            _parsedScript = new ParsedScript(sourceScript);

            Validate(sourceScript);
            if (_parsedScript.IsValid)
            {
                GenerateGraphPsScript(_parsedScript);
            }
            return _parsedScript;
        }

        private void GenerateGraphPsScript(ParsedScript parsedScript)
        {
            foreach (var aadCmd in parsedScript.Commands)
            {
                //var prms = aadCmd.FindAll(prm => prm is CommandParameterAst, true).Cast<CommandParameterAst>();
                List<Ast> children = aadCmd.FindAll(e => e.Parent == aadCmd, true).ToList();
                var convertedCommand = new ConvertedCommand();
                _parsedScript.ConvertedCommands.Add(convertedCommand);
                convertedCommand.SourceCommandAst = aadCmd;
                convertedCommand.CommandMapping = new CommandMapping();
                convertedCommand.CommandMapping.AadName = children[0].ToString();
                for (var index = 1; index < children.Count; index++)
                {
                    var child = children[index];

                    if (child is CommandParameterAst)
                    {
                        convertedCommand.CommandMapping.ParamMappings.Add(
                            new ParamMapping()
                            {
                                AadName = child.ToString()
                            });
                    }
                    else //Most probably a parameter value, let's set it as value for last parameter
                    {
                        convertedCommand.CommandMapping.ParamMappings.Last().Value = child.ToString();
                    }
                }

                var globalMappings = CommandMappingHelper.GetCommandMappings();

                var aadCommandMap = (from p in globalMappings
                                     where p.AadName.Equals(convertedCommand.CommandMapping.AadName)
                                     select p).FirstOrDefault();

                if (aadCommandMap != null)
                {
                    convertedCommand.CommandMapping.GraphName = aadCommandMap.GraphName;
                    foreach (var param in convertedCommand.CommandMapping.ParamMappings)
                    {
                        var aadParam = (from p in aadCommandMap.ParamMappings where p.AadName.Equals(param.AadName) select p).FirstOrDefault();
                        if (aadParam != null)
                        {
                            param.GraphName = aadParam.GraphName;
                        }
                    }

                    convertedCommand.ConvertedScript = convertedCommand.CommandMapping.GraphName;
                    foreach (var param in convertedCommand.CommandMapping.ParamMappings)
                    {
                        convertedCommand.ConvertedScript += " " + param.GraphName;
                        if (param.Value != null)
                        {
                            convertedCommand.ConvertedScript += " " + param.Value;
                        }
                    }
                }
                Debug.WriteLine(convertedCommand);
                //foreach (var child in children)
                //{
                //    Debug.WriteLine(child.ToString() + " " + child.GetType());
                //}
                //foreach (var prm in prms)
                //{
                //    Debug.WriteLine(prm.ParameterName);
                //}
            }
        }

        private void Validate(string sourceScript)
        {
            Token[] tokens;
            ParseError[] parseErrors;
            List<CommandAst> aadCmds = null;
            var ast = Parser.ParseInput(sourceScript, out tokens, out parseErrors);

            if (parseErrors == null || parseErrors.Length == 0)
            {
                aadCmds = GetAzureAdCommandsInScript(ast);
                if (aadCmds.Count > 0)
                {
                }
                else
                {
                    _parsedScript.Errors.Add(new ConverterParseError()
                    {
                        ErrorCode = "GPS03",
                        Message = String.Format("The script did not contain any Azure AD PowerShell commands.")
                    });
                }

            }
            else
            {
                LoadParseErrors(parseErrors);
            }

            _parsedScript.IsValid = _parsedScript.Errors.Count == 0;
            _parsedScript.Commands = aadCmds;
        }

        private static List<CommandAst> GetAzureAdCommandsInScript(ScriptBlockAst ast)
        {
            var cmds = ast.FindAll(cmd => cmd is CommandAst, true);

            var aadCmds = new List<CommandAst>();

            foreach (CommandAst cmd in cmds)
            {

                //check if the script starts with a command
                if (cmd.GetCommandName().Contains("-AzureAD", StringComparison.InvariantCultureIgnoreCase))
                {
                    aadCmds.Add(cmd);
                }
            }

            return aadCmds;
        }

        private void LoadParseErrors(ParseError[] parseErrors)
        {
            foreach (var error in parseErrors)
            {
                _parsedScript.Errors.Add(new ConverterParseError()
                {
                    ErrorCode = "GPS01",
                    Message = "Not a valid script. [" + error.ErrorId + "] " + error.Message
                });
            }
        }
    }
}