using GraphPsConverter.Core.Model;
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
            //Read through the script and create a tree of all the AADcommands that were found.
            foreach (var aadCmd in parsedScript.Commands)
            {
                var convertedCommand = new ConvertedCommand(aadCmd);
                _parsedScript.ConvertedCommands.Add(convertedCommand);
            }
        }

        private void Validate(string sourceScript)
        {
            Token[] tokens;
            ParseError[] parseErrors;
            List<CommandAst> aadCmds = null;

            if (string.IsNullOrEmpty(sourceScript))
            {
                _parsedScript.Errors.Add(GetErrorNoAadPowerShellCommands());
            }
            else
            {
                var ast = Parser.ParseInput(sourceScript, out tokens, out parseErrors);

                if (parseErrors == null || parseErrors.Length == 0)
                {
                    aadCmds = GetAzureAdCommandsInScript(ast);
                    if (aadCmds.Count > 0)
                    {
                    }
                    else
                    {
                        _parsedScript.Errors.Add(GetErrorNoAadPowerShellCommands());
                    }
                }
                else
                {
                    LoadParseErrors(parseErrors);
                }
            }

            _parsedScript.IsValid = _parsedScript.Errors.Count == 0;
            _parsedScript.Commands = aadCmds;
        }

        private static ConverterParseError GetErrorNoAadPowerShellCommands()
        {
            return new ConverterParseError()
            {
                ErrorCode = "GPS03",
                Message = String.Format("The script did not contain any Azure AD PowerShell commands.")
            };
        }

        private static List<CommandAst> GetAzureAdCommandsInScript(ScriptBlockAst ast)
        {
            var cmds = ast.FindAll(cmd => cmd is CommandAst, true);

            var aadCmds = new List<CommandAst>();

            foreach (CommandAst cmd in cmds)
            {

                //check if the script starts with a command
                if (CommandMappingHelper.IsAadCommand(cmd.GetCommandName()))
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