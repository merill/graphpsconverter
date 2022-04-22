using GraphPsConverter.Core.Model;
using Microsoft.VisualBasic.FileIO;

namespace GraphPsConverter.Core
{
    internal class CommandMappingHelper
    {
        private static Dictionary<string, CommandMap> _commandMaps;
        private static Dictionary<string, ParamMap> _paramMaps;

        public static void Init()
        {
            if(_commandMaps == null || _paramMaps == null) //Load if it is first time
            {
                _commandMaps = GetCommandMap();
                _paramMaps = GetParamMap();
            }
        }

        public static bool IsAadCommand(string aadCmdName)
        {
            Init();

            return _commandMaps.ContainsKey(GetCmdKey(aadCmdName));
        }

        public static CommandMap GetCommand(string aadCmdName)
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
            var cmd = GetCommand(aadCmdName);
            return cmd?.GraphCmdName;
        }

        public static string GetGraphParam(string aadCmdName, string aadParamName)
        {
            Init();

            var paramName = aadParamName.StartsWith("-") ? aadParamName.Substring(1) : aadParamName; //Remove hyphen
            var paramKey = GetParamKey(aadCmdName, paramName);
            if (_paramMaps.ContainsKey(paramKey))
            {
                return _paramMaps[paramKey].GraphParamName;
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
                    GraphModuleName = fields[3]
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
