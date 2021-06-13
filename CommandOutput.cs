using System;
using System.Collections.Generic;
using System.Linq;

namespace RootsBuilder
{
    public class CommandOutput
    {
        public string command = "";
        public List<string> output = new();
        public List<string> errors = new();
        public string exitCode = "";

        public CommandOutput(string command = "") =>
            this.command = $"Command>> {command}";

        public string AddOutput(string output)
        {
            string outputMessage = $"Output>> {output}";
            this.output.Add(outputMessage);
            return outputMessage;
        }

        public string AddError(string error)
        {
            string errorMessage = $"Error>> {error}";
            this.errors.Add(errorMessage);
            return errorMessage;
        }

        public string AddExitCode(string exitCode)
        {
            string exitCodeMessage = $"ExitCode>> {exitCode}";
            this.exitCode = exitCodeMessage;
            return exitCodeMessage;
        }

        public override int GetHashCode() =>
            HashCode.Combine(command, output, errors, exitCode);

        public override string ToString()
        {
            List<string> values = new();
            values.Add(command);
            values.AddRange(output.ToArray());
            values.AddRange(errors.ToArray());
            values.Add(exitCode);
            return string.Join("\n", values);
        }

        public override bool Equals(object obj) =>
            obj is CommandOutput output &&
            command == output.command &&
            this.output.SequenceEqual(output.output) &&
            errors.SequenceEqual(output.errors) &&
            exitCode == output.exitCode;
    }
}
