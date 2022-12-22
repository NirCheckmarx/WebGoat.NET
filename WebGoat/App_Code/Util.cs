using System;
using System.Diagnostics;
using log4net;
using System.Reflection;
using System.IO;
using System.Threading;

namespace OWASP.WebGoat.NET.App_Code
{
    public class Util
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public static int RunProcessWithInput(string cmd, string args, string input)
        {
            string sanitizedCmd = SanitizeInput(cmd, "cmd");
            string sanitizedArgs = SanitizeInput(args, "args");
            string sanitizedInput = SanitizeInput(input, "input");


            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Settings.RootDir,
                FileName = sanitizedCmd,
                Arguments = sanitizedArgs,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            using (Process process = new Process())
            {
                process.EnableRaisingEvents = true;
                process.StartInfo = startInfo;

                process.OutputDataReceived += (sender, e) => {
                    if (e.Data != null)
                        log.Info(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        log.Error(e.Data);
                };

                AutoResetEvent are = new AutoResetEvent(false);

                process.Exited += (sender, e) => 
                {
                    Thread.Sleep(1000);
                    are.Set();
                    log.Info("Process exited");

                };

                process.Start();

                using (StreamReader reader = new StreamReader(new FileStream(sanitizedInput, FileMode.Open)))
                {
                    string line;
                    string replaced;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                            replaced = line.Replace("DB_Scripts/datafiles/", "DB_Scripts\\\\datafiles\\\\");
                        else
                            replaced = line;

                        log.Debug("Line: " + replaced);

                        process.StandardInput.WriteLine(replaced);
                    }
                }
    
                process.StandardInput.Close();
    

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
    
                //NOTE: Looks like we have a mono bug: https://bugzilla.xamarin.com/show_bug.cgi?id=6291
                //have a wait time for now.
                
                are.WaitOne(10 * 1000);

                if (process.HasExited)
                    return process.ExitCode;
                else //WTF? Should have exited dammit!
                {
                    process.Kill();
                    return 1;
                }
            }
        }
        private static string SanitizeInput(string input, string inputType)
        {
                if (inputType == "cmd")
                {
                    
                    switch (input)
                    {
                        case "ls":
                        return "ls";
                        case "mkdir":
                        return "mkdir";
                        
                        return null;
                    }
                }
                else if (inputType =="args")
                {
                    switch (input)
                    {
                        case "-l":
                        return "-l";
                        case "-p":
                        return "-p";
                        
                        return null;
                    }

                }
                else if(inputType == "input")
                {
                    switch (input)
                    {
                        case "/tmp/file1.txt":
                        return "/tmp/file1.txt";
                        case "/tmp/file2.txt":
                        return "/tmp/file2.txt";
                        
                        return null;
                    }

                }
                return null; 
            }   
        }
    }

