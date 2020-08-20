using System;
using Amdocs.Ginger.Plugin.Core;

namespace GingerRubyPluginConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Ruby Plugin");

            using (GingerNodeStarter gingerNodeStarter = new GingerNodeStarter())
            {
                if (args.Length > 0)
                {
                    gingerNodeStarter.StartFromConfigFile(args[0]);
                }
                else
                {
                    gingerNodeStarter.StartNode("Ruby Script Execution Service", new RubyScriptExecuterService());                    
                }
                gingerNodeStarter.Listen();
            }
        }
    }
}
