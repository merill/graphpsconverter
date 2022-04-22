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
            var csv = GetCsv("CommandMap.csv");

            var commandMaps = new Dictionary<string, CommandMap>();

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
                commandMaps.Add(GetCmdKey(commandMap.AadCmdName), commandMap);
            }
            return commandMaps;
        }

        private static Dictionary<string, ParamMap> GetParamMap()
        {
            var csv = GetCsv("ParamMap.csv");

            var paramMaps = new Dictionary<string, ParamMap>();

            foreach (var fields in csv)
            {
                var paramMap = new ParamMap()
                {
                    AadCmdName = fields[0],
                    AadModuleName= fields[1],
                    GraphCmdName = fields[2],
                    GraphModuleName= fields[3],
                    AadParamName = fields[4],
                    GraphParamName = fields[5]
                };
                paramMaps.Add(GetParamKey(paramMap.AadCmdName, paramMap.AadParamName), paramMap);
            }
            return paramMaps;
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
