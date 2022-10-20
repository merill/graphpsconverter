using GraphPsConverter.Core.Model;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;

namespace GraphPsConverter.Core
{
    internal class CommandMappingHelper
    {
        private static Dictionary<string, CommandMap> _commandMaps;
        private static Dictionary<string, ParamMap> _paramMaps;
        private static Dictionary<string, string> _graphDocUri;
        public static void Init()
        {
            if(_commandMaps == null || _paramMaps == null) //Load if it is first time
            {
                _commandMaps = GetCommandMap();
                _paramMaps = GetParamMap();
                LoadPermissionMap();
            }
        }

        private static void LoadPermissionMap()
        {
            var permissionsCsvUri = "https://github.com/merill/graphpermissions.github.io/raw/main/permissions.csv";

            WebClient client = new WebClient();
            var content = client.DownloadString(permissionsCsvUri);


            _graphDocUri = new Dictionary<string, string>();
            using (var parser = new TextFieldParser(new StringReader(content)))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); //Skip header

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    var apiUri = fields[4];
                    var docUri = fields[5];
                    var cleanGraphUri = GetCleanGraphUri(apiUri);
                    if (!_graphDocUri.ContainsKey(cleanGraphUri))
                    {
                        _graphDocUri.Add(cleanGraphUri, docUri);
                    }
                    
                }
            }
        }

        private static string GetCleanGraphUri(string apiUri)
        {
            
            var uri = Regex.Replace(apiUri, @" ?\{.*?\}", string.Empty);
            return uri;
        }

        public static string GetGraphApiDocUri(string aadCmdName)
        {
            var cmdMap = GetCommandMap(aadCmdName);
            var cmdGraphUri = GetCleanGraphUri(cmdMap.GraphUri);
            string graphApiDocUri;
            _graphDocUri.TryGetValue(cmdGraphUri, out graphApiDocUri);
            return graphApiDocUri;
        }

        public static bool IsAadCommand(string aadCmdName)
        {
            Init();

            return _commandMaps.ContainsKey(GetCmdKey(aadCmdName));
        }

        public static CommandMap GetCommandMap(string aadCmdName)
        {
            Init();
            var aadCmdKey = GetCmdKey(aadCmdName);
            if (_commandMaps.ContainsKey(aadCmdKey))
            {
                return _commandMaps[aadCmdKey];
            }
            else
            {
                return null;
            }
        }

        public static string GetGraphCommandName(string aadCmdName)
        {
            var cmd = GetCommandMap(aadCmdName);
            return cmd?.GraphCmdName;
        }

        public static ParamMap GetParamMap(string aadCmdName, string aadParamName)
        {
            Init();

            var paramName = aadParamName.StartsWith("-") ? aadParamName.Substring(1) : aadParamName; //Remove hyphen
            var paramKey = GetParamKey(aadCmdName, paramName);
            if (_paramMaps.ContainsKey(paramKey))
            {
                return _paramMaps[paramKey];
            }
            else
            {
                return null;
            }
        }


        private static Dictionary<string, CommandMap> GetCommandMap()
        {


            var commandMaps = new Dictionary<string, CommandMap>();

            //Sort by module name so AzureADPreview comes after AzureAD module
            var sortedCommandMapList = GetCommandMapSortedByModuleName();
            foreach (var commandMap in sortedCommandMapList)
            {
                var key = GetCmdKey(commandMap.AadCmdName);
                //If the key is already there it's most probably AzureADPreview that is a duplicate of Azure AD so we skip
                if (!commandMaps.ContainsKey(key))
                {
                    commandMaps.Add(key, commandMap);
                }
            }

            return commandMaps;
        }

        private static IOrderedEnumerable<CommandMap> GetCommandMapSortedByModuleName()
        {
            var commandMapList = new List<CommandMap>();
            var csv = GetCsv("CommandMap.csv");
            foreach (var fields in csv)
            {
                var commandMap = new CommandMap()
                {
                    AadCmdName = fields[0],
                    AadModuleName = fields[1],
                    GraphCmdName = fields[2],
                    GraphModuleName = fields[3],
                    GraphCmdScope = fields[4],
                    GraphUri = fields[5],
                };
                commandMapList.Add(commandMap);
            }
            var sortedCommandMapList = from p in commandMapList orderby p.AadModuleName select p;
            return sortedCommandMapList;
        }

        private static Dictionary<string, ParamMap> GetParamMap()
        {

            var paramMaps = new Dictionary<string, ParamMap>();

            //Sort by module name so AzureADPreview comes after AzureAD module
            var sortedList = GetParamMapSortedByModuleName();
            foreach (var paramMap in sortedList)
            {
                var key = GetParamKey(paramMap.AadCmdName, paramMap.AadParamName);
                //If the key is already there it's most probably AzureADPreview that is a duplicate of Azure AD so we skip
                if (!paramMaps.ContainsKey(key))
                {
                    paramMaps.Add(key, paramMap);
                }
                
            }
            
            return paramMaps;
        }

        private static IOrderedEnumerable<ParamMap> GetParamMapSortedByModuleName()
        {
            var paramMapList = new List<ParamMap>();
            var csv = GetCsv("ParamMap.csv");

            foreach (var fields in csv)
            {
                var paramMap = new ParamMap()
                {
                    AadCmdName = fields[0],
                    AadModuleName = fields[1],
                    GraphCmdName = fields[2],
                    GraphModuleName = fields[3],
                    AadParamName = fields[4],
                    GraphParamName = fields[5]
                };
                paramMapList.Add(paramMap);
            }

            var sortedList = from p in paramMapList orderby p.AadModuleName select p;
            return sortedList;
        }

        public static string GetCmdKey(string aadCmdName)
        {
            return aadCmdName.ToLower();
        }

        public static string GetParamKey(string aadCmdName, string aadParamName)
        {
            return string.Format("{0};{1}", aadCmdName, aadParamName).ToLower(); 
        }

        private static List<string[]> GetCsv(string fileName)
        {
            var assembly = typeof(CommandMappingHelper).Assembly;
            var stream = assembly.GetManifestResourceStream("GraphPsConverter.Core.Data." + fileName);

            var csv = new List<string[]>();
            using (var parser = new TextFieldParser(stream))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.ReadFields(); //Skip header

                while (!parser.EndOfData)
                {
                    csv.Add(parser.ReadFields());
                }
            }
            return csv;

        }
    }
}
