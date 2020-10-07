using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerRubyPluginConsole;
using Amdocs.Ginger.Plugin.Core;
using System.Threading;
using System.Collections.Generic;
using GingerTestHelper;

namespace GingerRubyPluginTest
{
    [TestClass]
    public class RubyExecutionTest
    {
        [TestMethod]
        public void SimpleScriptFileTest()
        {
            //Arrange
            RubyScriptExecuterService rubyScriptExecuterService = new RubyScriptExecuterService();
            GingerAction GA = new GingerAction();

            List<RubyPrameters> rubyPrameters = new List<RubyPrameters>();
            rubyPrameters.Add(new RubyPrameters() { Param = "Param 1", Value = "10" });
            rubyPrameters.Add(new RubyPrameters() { Param = "Param 2", Value = "20" });
            //Act

            rubyScriptExecuterService.ExecuteRubyScriptFile(GA, TestResources.GetTestResourcesFile("test.rb"), "=", rubyPrameters);

            //Assert
            string str = string.Empty;
            Assert.AreEqual((GA.Output != null && GA.Output.OutputValues.Count > 0), true, "Execution Output values found validation");
            foreach (IGingerActionOutputValue s in GA.Output.OutputValues)
            {
                str = s.Value.ToString();
            }
            Assert.AreEqual(str.Contains("30"), true);
        }


        [TestMethod]
        public void SimpleScriptContentTest()
        {
            //Arrange
            RubyScriptExecuterService rubyScriptExecuterService = new RubyScriptExecuterService();
            GingerAction GA = new GingerAction();
            List<RubyPrameters> rubyPrameters = new List<RubyPrameters>();
            rubyPrameters.Add(new RubyPrameters() { Param = "Param 1", Value = "10" });
            rubyPrameters.Add(new RubyPrameters() { Param = "Param 2", Value = "20" });

            string script = @"sum=ARGV[0].to_i+ARGV[1].to_i
                            puts ""Result : #{sum}""";
            //Act
            rubyScriptExecuterService.ExecuteRubyScript(GA, script, ":", rubyPrameters);

            //Assert
            string str = string.Empty;
            foreach (IGingerActionOutputValue s in GA.Output.OutputValues)
            {
                str = s.Value.ToString();
            }
            Assert.AreEqual(str.Contains("30"), true);
        }

        [TestMethod]
        public void SimpleScriptFileTestOnLinux()
        {
            //Arrange
            RubyScriptExecuterService rubyScriptExecuterService = new RubyScriptExecuterService();
            GingerAction GA = new GingerAction();

            List<RubyPrameters> rubyPrameters = new List<RubyPrameters>();
            rubyPrameters.Add(new RubyPrameters() { Param = "Param 1", Value = "10" });
            rubyPrameters.Add(new RubyPrameters() { Param = "Param 2", Value = "20" });
            //Act
            rubyScriptExecuterService.ExecuteRubyScriptFile(GA, TestResources.GetTestResourcesFile("test.rb"), "=", rubyPrameters);

            //Assert
            string str = string.Empty;
            Assert.AreEqual((GA.Output != null && GA.Output.OutputValues.Count > 0), true, "Execution Output values found validation");
            foreach (IGingerActionOutputValue s in GA.Output.OutputValues)
            {
                str = s.Value.ToString();
            }
            Assert.AreEqual(str.Contains("30"), true);
        }
    }
}
