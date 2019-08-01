using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StackWrapper
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]

        private static extern int GetShortPathName(String pathName, StringBuilder shortName, int cbShortName);
        private const string StackPath = "STACK_PATH";
        private const string StackLocalAppData = "STACK_LOCALAPPDATA";
        private const string LocalAppData = "LOCALAPPDATA";

        static void Main(string[] args)
        {
            // determine environment
            string stackPath = EnvVar(StackPath);
            string stackLocalAppData = EnvVar(StackLocalAppData);
            string localAppData = EnvVar(LocalAppData);

            // get the short name for LocalAppData
            string localAppDataShort = ShortPathName(localAppData);

            // construct the arguments
            string stackArgs = "";
            foreach (string arg in args)
            {
                stackArgs += $" {arg}";
            }
            stackArgs = stackArgs.Replace(localAppData, localAppDataShort);
            // Console.WriteLine($"stackArgs: {stackArgs}");

            // create the Process
            Process process = new Process();
            process.StartInfo.FileName = stackPath;
            process.StartInfo.Arguments = stackArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.Environment.Add(LocalAppData, stackLocalAppData);
            // set output and error (asynchronous) handlers
            process.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            process.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            // start process and handlers
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
        static void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }

        private static string EnvVar(string name)
        {
            string value = System.Environment.GetEnvironmentVariable(name);
            if (value == null)
            {
                Console.WriteLine($"Environment variable ${value} not set");
                System.Environment.Exit(1);
            }
            // Console.WriteLine($"Environment variable {name} = {value}");
            return value;
        }

        private static string ShortPathName(string longPath)
        {
            const int bufSize = 300;
            StringBuilder sb = new StringBuilder(bufSize);
            int rc = GetShortPathName(longPath, sb, bufSize);
            if (rc == 0) // check for errors
            {
                Console.WriteLine(Marshal.GetLastWin32Error().ToString());
                System.Environment.Exit(1);
            }
            string shortPath = sb.ToString();
            // Console.WriteLine($"path: {longPath} = {shortPath}");
            return shortPath;
        }
    }
}
