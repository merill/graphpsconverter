using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GraphPsConverter.Core;

namespace GraphPsConverter.Tests
{
    [TestClass]
    public class Converter_IsValidPowerShellScript
    {
        private readonly Converter _converter = new Converter();

        [TestMethod]
        public void IsValid_InputIsNotAzureADCommand_ReturnFalse()
        {
            var ps = "Get-PnPApp -Scope Site";
            var parsedScript = _converter.ConvertToGraphPowerShell(ps);
            Assert.IsFalse(parsedScript.IsValid);
        }

        [TestMethod]
        public void IsValid_InputIsAzureADCommandSingleCmd_ReturnTrue()
        {
            var ps = @"Get-AzureADUser -All -Filter 'name eq 34'";
            var parsedScript = _converter.ConvertToGraphPowerShell(ps);
            Assert.IsTrue(parsedScript.IsValid);
        }

        [TestMethod]
        public void IsValid_InputIsAzureADCommandMultiCmd_ReturnTrue()
        {
            var ps = @"Get-AzureADUser -All -Filter 'name eq 34'
Get-PnPApp -Scope Site
";
            var parsedScript = _converter.ConvertToGraphPowerShell(ps);
            Assert.IsTrue(parsedScript.IsValid);
        }

        [TestMethod]
        public void IsValid_InputIsAzureADCommandWithAssignment_ReturnTrue()
        {
            var ps = "$u = Get-AzureADUser -All -Filter 'name eq 34'";
            var parsedScript = _converter.ConvertToGraphPowerShell(ps);
            Assert.IsTrue(parsedScript.IsValid);
        }

        [TestMethod]
        public void IsValid_InputIsAzureADCommandWithParamVariable_ReturnTrue()
        {
            var ps = "Get-AzureADUser -All -Filter \"$name eq 34\"";
            var parsedScript = _converter.ConvertToGraphPowerShell(ps);
            Assert.IsTrue(parsedScript.IsValid);
        }

        [TestMethod]
        public void IsValid_InputIsAzureADCommandWithNestedCmd_ReturnTrue()
        {
            var ps = "Get-AzureADUser -All -Filter \"$name eq (Get-AzureADUser -Top 1)\"";
            var parsedScript = _converter.ConvertToGraphPowerShell(ps);
            Assert.IsTrue(parsedScript.IsValid);
        }

        [TestMethod]
        public void IsValid_GetAzureAdUser_All_ReturnMatch()
        {
            var aadPs = "Get-AzureADUser -All";
            var expected = "Get-MgUser -All";
            var actual = _converter.ConvertToGraphPowerShell(aadPs);
            Assert.AreEqual(expected, actual.ConvertedCommands[0].ConvertedScript);
        }
    }

}