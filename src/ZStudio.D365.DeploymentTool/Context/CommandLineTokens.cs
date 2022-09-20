using System;
using System.Collections.Generic;

namespace ZD365DT.DeploymentTool.Context
{
    public class CommandLineTokens : List<CommandLineArgument>
    {
        private List<CommandLineArgument> options;

        private CommandLineTokens()
        {
            options = new List<CommandLineArgument>();
        }

        public static CommandLineTokens FromArguments(params string[] args)
        {
            CommandLineTokens result = new CommandLineTokens();
            foreach (string arg in args)
            {
                CommandLineArgument a = CommandLineArgument.FromArgument(arg);
                if (a != null)
                    result.Add(a);
            }
            return result;
        }

        public static CommandLineTokens FromCommandLineArguments()
        {
            List<string> commandLineArgs = new List<string>();
            commandLineArgs.AddRange(Environment.GetCommandLineArgs());
            commandLineArgs.RemoveAt(0); // The first argument is the program name
            return FromArguments(commandLineArgs.ToArray());
        }
    }

    public class CommandLineArgument
    {
        private static readonly char[] TRIM_CHARS = new char[] { ':', ' ', '@', '"' };
        private string token = null;
        private string value = null;

        private CommandLineArgument(string token, string value)
        {
            if (String.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Blank token passed in on the command line argument");
            }
            else if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Blank value passed in on the command line argument");
            }
            this.value = value;
            this.token = token;
        }

        public string Token { get { return token; } }

        public string Value { get { return value; } }

        public static CommandLineArgument FromArgument(string arg)
        {
            //check if the argument is a command option
            if (!arg.StartsWith(@"/"))
            {
                if (arg.IndexOf(":") <= 0)
                {
                    throw new ArgumentException("Invalid Commandline Token", arg);
                }
                int splitIndex = arg.IndexOf(":");
                string tokenString = arg.Substring(0, splitIndex).Trim(TRIM_CHARS);
                string valueString = arg.Substring(splitIndex, arg.Length - splitIndex).Trim(TRIM_CHARS);
                return new CommandLineArgument(tokenString, valueString);
            }
            else
                return null;
        }
    }
}
