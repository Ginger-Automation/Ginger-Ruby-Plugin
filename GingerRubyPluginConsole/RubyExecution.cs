﻿using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GingerRubyPluginConsole
{
    public class RubyExecution
    {
        IGingerAction mGingerAction = null;
        public IGingerAction GingerAction
        {
            get
            {
                return mGingerAction;
            }
            set
            {
                mGingerAction = value;
                if (mGingerAction != null)
                {
                    mGingerAction.AddExInfo("\n");
                }
            }
        }

        string mOutputs = string.Empty;

        public enum eExecutionMode { ScriptPath, FreeCommand }
        public eExecutionMode ExecutionMode;

        public List<RubyPrameters> RubyPrameters = new List<RubyPrameters>();

        static string mCommandOutputErrorBuffer = string.Empty;
        static string mCommandOutputBuffer = string.Empty;

        string mRubyScriptPath = null;
        public string RubyScriptPath
        {
            get
            {
                return mRubyScriptPath;
            }
            set
            {
                mRubyScriptPath = value;
            }
        }
        string mDelimeter = null;
        public string Delimeter
        {
            get
            {
                return mDelimeter;
            }
            set
            {
                mDelimeter = value;
            }
        }
        public void Execute()
        {
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%% Execution Started %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
            try
            {
                switch(ExecutionMode)
                {
                    case eExecutionMode.ScriptPath:
                        CommandElements command = new CommandElements();                       
                        command = PrepareCommand();
                        ExecuteCommand(command);
                        ParseCommandOutput();
                        break;
                }
            }
            catch(Exception ex)
            {
                GingerAction.AddError("Error while executing script : " + ex.ToString());
            }
        }

        public static bool OutputParamExist(GingerAction GA, string paramName, string paramValue = null)
        {
            IGingerActionOutputValue val = null;
            if (paramValue == null)
            {
                val = GA.Output.OutputValues.Where(x => x.Param == paramName).FirstOrDefault();
            }
            else
            {
                val = GA.Output.OutputValues.Where(x => x.Param == paramName && x.Value.ToString() == paramValue).FirstOrDefault();
            }

            if (val == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        private CommandElements PrepareCommand()
        {
            string Arguments = string.Empty;
            CommandElements command = new CommandElements();            
            command.ExecuterFilePath = @"ruby";  
            command.WorkingFolder = Path.GetDirectoryName(RubyScriptPath);
            Arguments += string.Format("\"{0}\"", RubyScriptPath) ;
            if(RubyPrameters != null)
            {
                foreach (RubyPrameters rp in RubyPrameters)
                {
                    Arguments += " " + rp.Value;
                }
            }            
            command.Arguments = Arguments;
            return command;
        }


        public void SetContent(String content)
        {
            try
            {
                RubyScriptPath = System.IO.Path.GetTempFileName().Replace(".tmp", ".rb");
                StreamWriter sw = new StreamWriter(RubyScriptPath);
                sw.Write(content);
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create ruby file");
                throw e;
            }            
        }

        static protected void AddCommandOutput(string output)
        {
            mCommandOutputBuffer += output + "\n";
            Console.WriteLine(output);
        }
        static protected void AddCommandOutputError(string error)
        {
            mCommandOutputErrorBuffer += error + "\n";
            Console.WriteLine(error);
        }
        protected void Process_Exited(object sender, EventArgs e)
        {           
            Console.WriteLine("Command Execution Ended");
        }
        private void ParseCommandOutput()
        {
            try
            {
                //Error
                if (!string.IsNullOrEmpty(mCommandOutputErrorBuffer.Trim().Trim('\n')))
                {                                        
                    GingerAction.AddError(string.Format("Console Errors: \n{0}", mCommandOutputErrorBuffer));                    
                }

                //Output values
                Regex rg = new Regex(@"Microsoft.*\n.*All rights reserved.");
                string stringToProcess = rg.Replace(mCommandOutputBuffer, "");
                string[] values = stringToProcess.Split('\n');
                foreach (string dataRow in values)
                {
                    if (dataRow.Length > 0) // Ignore empty lines
                    {
                        string param;
                        string value;
                        int signIndex = -1;
                        if (string.IsNullOrEmpty(mDelimeter))
                        {
                            signIndex = dataRow.IndexOf("=");
                        }
                        else
                        {
                            signIndex = dataRow.IndexOf(mDelimeter);
                        }
                        if (signIndex > 0)
                        {
                            param = dataRow.Substring(0, signIndex);
                            //the rest is the value
                            value = dataRow.Substring(param.Length + 1);
                            GingerAction.AddOutput(param, value, "Console Output");
                        }
                        else 
                        {
                            GingerAction.AddOutput("?????", dataRow, "Console Output");
                        }
                        //TODO: Add the result to Action output values
                    }
                }
            }
            catch (Exception ex)
            {                
                GingerAction.AddError(string.Format("Failed to parse all command console outputs, Error:'{0}'", ex.Message));
            }
        }
        
        public void ExecuteCommand(object commandVal)
        {
            try
            {
                CommandElements commandVals = (CommandElements)commandVal;
                Task.Run(() =>
                {
                    Process process = new Process();
                    if (commandVals.WorkingFolder != null)
                    {
                        process.StartInfo.WorkingDirectory = commandVals.WorkingFolder;
                    }

                    process.StartInfo.FileName = commandVals.ExecuterFilePath;
                    process.StartInfo.Arguments = commandVals.Arguments;

                    //process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    mCommandOutputBuffer = string.Empty;
                    mCommandOutputErrorBuffer = string.Empty;
                    process.OutputDataReceived += (proc, outLine) => { AddCommandOutput(outLine.Data); };
                    process.ErrorDataReceived += (proc, outLine) => { AddCommandOutputError(outLine.Data); };
                    process.Exited += Process_Exited;
                    Console.Write("--Staring process");
                    process.Start();
                    Console.WriteLine(process.Id);
                    //Stopwatch stopwatch = Stopwatch.StartNew();
                    process.BeginOutputReadLine();

                    process.BeginErrorReadLine();

                    int maxWaitingTime = 1000 * 60 * 60;//TODO: User defined
                    process.PriorityBoostEnabled = true;

                    process.WaitForExit();
                    Console.Write("--Process done");
                    //stopwatch.Stop();

                    //if (stopwatch.ElapsedMilliseconds >= maxWaitingTime)
                    //{
                    //    GingerAction.AddError("Command processing timeout has reached!");
                    //}
                }).Wait();

                //CommandElements commandVals = (CommandElements)commandVal;

                //using (Process compiler = new Process())
                //{
                //    //compiler.StartInfo.FileName = @"C:\Groovy\Groovy-2.5.5\bin\groovy.bat";
                //    //compiler.StartInfo.Arguments = "\"C:\\Work\\Scripts\\BasicScript.groovy\" 10 20 30";
                //    compiler.StartInfo.FileName = @"C:\Ruby27\bin\ruby.exe";
                //    compiler.StartInfo.Arguments = "\"C:\\Work\\Scripts\\test.rb\" 10 22";
                //    compiler.StartInfo.WorkingDirectory = "C:\\Work\\Scripts\\";
                //    compiler.StartInfo.UseShellExecute = false;
                //    compiler.StartInfo.RedirectStandardOutput = true;
                //    //compiler.StartInfo.RedirectStandardError = true;
                //    compiler.StartInfo.CreateNoWindow = true;

                //    compiler.Start();
                //    string output = compiler.StandardOutput.ReadToEnd();
                //    AddCommandOutput(output);
                //    //compiler.WaitForExit();
                //}
            }
            catch (Exception ex)
            {
                GingerAction.AddError("Failed to execute the command, Error is: '{0}'" + ex.Message);
                Console.Write(ex.Message);
            }
            finally
            {
                GingerAction.AddExInfo("--Exiting execute command");
            }
            
        }
        
        private void Process_ErrorDataReceivedAsync(object sender, DataReceivedEventArgs e)
        {
            mOutputs += e.Data + System.Environment.NewLine;
        }

        private void Process_OutputDataReceivedAsync(object sender, DataReceivedEventArgs e)
        {
            mOutputs += e.Data + System.Environment.NewLine;
        }
    }
}
