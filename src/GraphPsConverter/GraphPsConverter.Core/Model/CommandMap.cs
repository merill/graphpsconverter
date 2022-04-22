﻿namespace GraphPsConverter.Core.Model
{
    public class CommandMap
    {
        public string AadCmdName { get; set; }
        public string GraphCmdName { get; set; }

        public string AadModuleName { get; set; }
        public string GraphModuleName { get; set; }

        public string GraphCmdScope { get; set; }
        public string GraphUri { get; set; }
    }
}
