using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace RootsBuilder
{
    public class CommandExecuter
    {
        public static readonly string[] versionCommands = { "--version", "-version", "-v" };

        public static void CheckVersion(string command, string version)
        {
            CommandOutput commandOutput = GetVersion(command);

            string outputVersion = Regex.Replace(commandOutput.output.First(), "[^0-9.]", "");

            string[] explodedVersion = version.Split(".");
            string[] explodedOutputVersion = outputVersion.Split(".");

            for (int i = 0; i < explodedVersion.Length; i++)
                if (explodedOutputVersion[i] != explodedVersion[i])
                    throw new Exception($"{command} with a version of {outputVersion} and not {version}");
        }

        private static CommandOutput GetVersion(string command)
        {
            CommandOutput commandOutput = new();

            for (int i = 0; i < versionCommands.Length; i++)
            {
                string versionCommand = versionCommands[i];

                commandOutput = ExecuteCommand($"{command} {versionCommand}");

                if (commandOutput.exitCode.Contains("0"))
                    break;
            }

            if (!commandOutput.exitCode.Contains("0"))
                throw new Exception($"{command} is not a command with a version descriptor");

            return commandOutput;
        }

        public static CommandOutput ExecuteCommand(string command)
        {
            CommandOutput commandOutput = new(command);

            Console.WriteLine(commandOutput.command);

            Process process = Process.Start(CreateProcessInfo(command));

            LogStreamAsync(process.StandardOutput, commandOutput.AddOutput);

            LogStreamAsync(process.StandardError, commandOutput.AddError);

            process.WaitForExit();

            LogAndWriteToConsole(commandOutput.AddExitCode, process.ExitCode.ToString());

            process.Close();

            return commandOutput;
        }

        private static void LogStreamAsync(StreamReader stream, Func<string, string> LogMethod)
        {
            Thread thread = new(_ => LogStream(stream, LogMethod));
            thread.Start();
        }

        private static void LogStream(StreamReader stream, Func<string, string> LogMethod)
        {
            while (!stream.EndOfStream)
            {
                string output = stream.ReadLine();

                LogAndWriteToConsole(LogMethod, output);
            }
        }

        private static void LogAndWriteToConsole(Func<string, string> LogMethod, string output) =>
            Console.WriteLine(LogMethod.Invoke(output));

        private static ProcessStartInfo CreateProcessInfo(string command) =>
             new("cmd.exe", $"/c {command}")
             {
                 CreateNoWindow = false,
                 UseShellExecute = false,
                 RedirectStandardError = true,
                 RedirectStandardOutput = true
             };
    }
}
