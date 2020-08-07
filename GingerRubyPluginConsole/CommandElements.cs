using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace GingerRubyPluginConsole
{
    public class CommandElements
    {
        public string WorkingFolder;
        public string ExecuterFilePath;
        public string Arguments;

        public string FullCommand
        {
            get
            {
                if (WorkingFolder == null)
                {
                    return string.Format("{0}{1}", ExecuterFilePath, Arguments);
                }
                else
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return string.Format("{0}>{1} {2}", WorkingFolder, ExecuterFilePath, Arguments);
                    }
                    else//Linux
                    {
                        return string.Format("[user@server {0}]$ {1} {2}", WorkingFolder, ExecuterFilePath, Arguments);
                    }
                }
            }
        }
    }
}
