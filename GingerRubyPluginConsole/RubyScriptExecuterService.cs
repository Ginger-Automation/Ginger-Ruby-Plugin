using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Plugin.Core;

namespace GingerRubyPluginConsole
{
    [GingerService("RubyScriptExecuter", "Execute Ruby Script via Ginger")]
    public class RubyScriptExecuterService
    {
        /// <summary>
        /// ExecuteRubyScriptFile
        /// </summary>
        /// <param name="GA">IGingerAction</param>
        /// <param name="string">Delimeter</param>
        /// <param name="RubyScriptPath">RubyScriptPath</param>
        /// <param name="RubyPrameters">RubyPrameters</param>
        [GingerAction("ExecuteRubyScriptFile", "Execute Ruby Script File")]
        public void ExecuteRubyScriptFile(IGingerAction GA,
            [Browse(true)]
            [BrowseType(BrowseType =BrowseTypeAttribute.eBrowseType.File)]
            [FileType("rb")]
            [Label("Ruby Script Path")]
            string RubyScriptPath,
            [Default("=")]
            [Label("Delimeter (Split the console output param/value)")]
            [Tooltip("Split the console output param/value")]
            string Delimeter,
            [Label("Ruby Parameters")]
            List<RubyPrameters> RubyPrameters)
        {
            RubyExecution rubyExecution = new RubyExecution();
            rubyExecution.ExecutionMode = RubyExecution.eExecutionMode.ScriptPath;
            rubyExecution.GingerAction = GA;            
            rubyExecution.RubyScriptPath = RubyScriptPath;
            rubyExecution.Delimeter = Delimeter;
            rubyExecution.RubyPrameters  = RubyPrameters;
            rubyExecution.Execute();
        }

        /// <summary>
        /// ExecuteRubyScript
        /// </summary>
        /// <param name="GA">IGingerAction</param>
        /// <param name="string">Delimeter</param>
        /// <param name="RubyScriptContent">RubyScriptContent</param>
        /// <param name="RubyPrameters">RubyPrameters</param>
        [GingerAction("ExecuteRubyScript", "Execute Ruby Script")]
        public void ExecuteRubyScript(IGingerAction GA,
            [Label("Ruby Script Content")]
            string RubyScriptContent,
            [Default("=")]
            [Label("Delimeter (Split the console output param/value)")]
            [Tooltip("Split the console output param/value")]
            string Delimeter,
            [Label("Ruby Parameters")]
            List<RubyPrameters> RubyPrameters)
        {
            RubyExecution rubyExecution = new RubyExecution();
            rubyExecution.ExecutionMode = RubyExecution.eExecutionMode.ScriptPath;
            rubyExecution.GingerAction = GA;            
            rubyExecution.SetContent(RubyScriptContent);
            rubyExecution.Delimeter = Delimeter;
            rubyExecution.RubyPrameters = RubyPrameters;
            rubyExecution.Execute();
        }
    }
}
