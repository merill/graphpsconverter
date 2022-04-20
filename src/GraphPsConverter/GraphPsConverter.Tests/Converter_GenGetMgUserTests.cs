﻿using GraphPsConverter.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphPsConverter.Tests
{
    [TestClass]
    internal class Converter_GenGetMgUserTests
    {
        private readonly Converter _converter = new Converter();

        [TestMethod]
        public void IsValid_GetAzureAdUser_All_ReturnMatch()
        {
            var aadPs = "Get-AzureADUser -All";
            var expected = "Get-MgUser -All";
            var actual = _converter.ConvertToGraphPowerShell(aadPs);
            Assert.AreEqual(expected, actual);
        }
    }
}
