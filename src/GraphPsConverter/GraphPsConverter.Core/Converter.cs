namespace GraphPsConverter.Core
{
    public class Converter
    {
        public string? ConvertToGraphPowerShell(string? sourceScript)
        {
            if(sourceScript == null) return null;

            return sourceScript.Replace("get-azureaduser", "get-mguser");

        }
    }
}