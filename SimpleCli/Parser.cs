using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SimpleCli
{
    public class Parser
    {
        public Parser(string name,
                      string[] args,
                      int maxWidth = 80,
                      string indent = "        ")
        {
            this.name = name;
            this.args = args;
            this.maxWidth = maxWidth;
            this.indent = indent;
            positionalArgs = new List<PositionalArg>();
            optionalArgs = new List<OptionalArg>();
            operands = new ArrayList();

            Add("help",
                'h',
                "Displays this help message and exits.",
                "",
                Arg.Type.BOOLEAN,
                Validator.AcceptAll,
                new string[] { },
                new string[] { });
        }

        public string Pop(string name = "", bool mustExist = true)
        {
            if (Count() == 0)
            {
                if (mustExist)
                    throw new ParseException($"Argument: \"{name}\" must be supplied.");
                else
                    return "";
            }

            var ret = args[0];
            if (Count() > 1)
                args = Util.Slice(args, 1, args.Length);
            else
                args = new string[0];
            return ret;
        }

        public string Peek(string name = "", bool mustExist = true)
        {
            if (Count() == 0)
            {
                if (mustExist)
                    throw new ParseException($"Argument: \"{name}\" must be supplied.");
                else
                    return "";
            }

            return args[0];
        }

        public int Count()
        {
            return args.Length;
        }

        public void Add(string name,
                        string helpText,
                        PositionalArg.ValidatorDelegate validator)
        {
            positionalArgs.Add(new PositionalArg(name, helpText, validator));
        }

        public void Add(string name,
                        string helpText)
        {
            positionalArgs.Add(new PositionalArg(name, helpText, Validator.AcceptAll));
        }

        public void Add(string name,
                        char flag,
                        string helpText,
                        string defaultValue,
                        Arg.Type type,
                        Arg.ValidatorDelegate validator,
                        string[] conflicts,
                        string[] overrides)
        {
            optionalArgs.Add(new OptionalArg(name,
                                             flag,
                                             helpText,
                                             defaultValue,
                                             type,
                                             validator,
                                             conflicts,
                                             overrides));
        }

        public void Add(string name,
                        char flag,
                        string helpText,
                        string defaultValue,
                        Arg.Type type,
                        Arg.ValidatorDelegate validator,
                        string[] conflicts)
        {
            optionalArgs.Add(new OptionalArg(name,
                                             flag,
                                             helpText,
                                             defaultValue,
                                             type,
                                             validator,
                                             conflicts,
                                             new string[] { }));
        }

        public void Add(string name,
                        char flag,
                        string helpText,
                        string defaultValue,
                        Arg.Type type,
                        Arg.ValidatorDelegate validator)
        {
            optionalArgs.Add(new OptionalArg(name,
                                             flag,
                                             helpText,
                                             defaultValue,
                                             type,
                                             validator,
                                             new string[] { },
                                             new string[] { }));
        }

        public void Add(string name,
                        char flag,
                        string helpText,
                        string defaultValue,
                        Arg.Type type)
        {
            optionalArgs.Add(new OptionalArg(name,
                                             flag,
                                             helpText,
                                             defaultValue,
                                             type,
                                             Validator.AcceptAll,
                                             new string[] { },
                                             new string[] { }));
        }

        private int IndexOfPrefix(ArrayList argList, string prefix)
        {
            for (var i = 0; i < argList.Count; i++)
                if (((string)argList[i]).StartsWith(prefix))
                    return i;
            return -1;
        }

        private void ParseOperands(ArrayList argList)
        {
            int index = argList.IndexOf("--");
            if (index != -1)
            {
                for (var i = index + 1; i < argList.Count; i++)
                    operands.Add(argList[i]);
                argList.RemoveRange(index, argList.Count - index);
            }
        }

        private void ParseHelp(ArrayList argList)
        {
            int index = argList.IndexOf("-h");
            if (index == -1)
                index = argList.IndexOf("--help");
            if (index != -1)
                throw new ParseException(GetHelp());
        }

        private void ParseOptionalArg(ArrayList argList, OptionalArg arg)
        {
            string flag = $"-{arg.flag}";
            string longFlag = $"--{arg.name}";

            int index = IndexOfPrefix(argList, flag);
            if (index == -1)
                index = IndexOfPrefix(argList, longFlag);

            if (index == -1)
            {
                arg.value = arg.defaultValue;
                arg.Validate();
                return;
            }

            string argFlag = (string)argList[index];
            if (arg.type == Arg.Type.SINGLE || arg.type == Arg.Type.MULTIPLE)
            {
                if (argFlag.StartsWith(flag) && argFlag.Length > flag.Length)
                {
                    arg.value = argFlag.Substring(flag.Length);
                    argList.RemoveAt(index);
                }
                else if (argFlag.StartsWith(longFlag) && argFlag.Length > longFlag.Length)
                    throw new ParseException(GetUsageHelp());
                else
                {
                    if (index == argList.Count - 1)
                        throw new ParseException(GetUsageHelp());

                    arg.value = (string)argList[index + 1];
                    argList.RemoveRange(index, 2);
                }

                arg.Validate();
            }
            else
            {
                if (argFlag != flag && argFlag != longFlag)
                    throw new ParseException(GetUsageHelp());

                arg.value = "true";
                argList.RemoveAt(index);
            }

            arg.wasParsed = true;
        }

        private void ParsePositionalArgs(ArrayList argList)
        {
            if (argList.Count != positionalArgs.Count)
                throw new ParseException(GetUsageHelp());

            for (var i = 0; i < positionalArgs.Count; i++)
            {
                positionalArgs[i].value = (string)argList[i];
                positionalArgs[i].Validate();
            }
        }

        private void ParseConflictsAndOverrides(ArrayList argList)
        {
            foreach (var arg in optionalArgs)
            {
                if (arg.wasParsed)
                {
                    if (arg.conflicts.Length > 0)
                        foreach (var conflict in arg.conflicts)
                            if (this[conflict].wasParsed)
                                throw new ParseException($"Argument \"{arg.name}\" conflicts with argument \"{conflict}\"");

                    if (arg.overrides.Length > 0)
                        foreach (var over in arg.overrides)
                            this[over].value = arg.value;
                }
            }
        }

        public void Parse()
        {
            var argList = new ArrayList();

            while (Count() > 0)
                argList.Add(Pop());

            ParseOperands(argList);
            ParseHelp(argList);

            foreach (var arg in optionalArgs)
                ParseOptionalArg(argList, arg);

            ParsePositionalArgs(argList);
            ParseConflictsAndOverrides(argList);
        }

        private string TerminalFormat(ArrayList words, string prepend)
        {
            StringBuilder buffer = new StringBuilder();

            int lineLength = 0;

            foreach (string word in words)
            {
                if (lineLength + word.Length > maxWidth)
                {
                    buffer.Append('\n');
                    buffer.Append(prepend);
                    lineLength = prepend.Length;
                }

                lineLength += word.Length;
                buffer.Append(word);
                buffer.Append(' ');
            }

            buffer.Append('\n');

            return buffer.ToString();
        }

        public string GetHelp()
        {
            StringBuilder buffer = new StringBuilder();

            buffer.Append(GetUsageHelp());

            if (positionalArgs.Count > 0)
                buffer.Append(GetPositionalHelp());

            if (optionalArgs.Count > 0)
                buffer.Append(GetOptionalHelp());

            return buffer.ToString();
        }

        public Arg this[string name]
        {
            get
            {
                foreach (var arg in positionalArgs)
                    if (arg.name == name)
                        return arg;

                foreach (var arg in optionalArgs)
                    if (arg.name == name)
                        return arg;

                throw new ParseException($"Invalid arg: \"{name}\"");
            }
            set
            {
                throw new ParseException("Args cannot be mutated.");
            }
        }

        public bool WasParsed(string name)
        {

            foreach (var arg in optionalArgs)
                if (arg.name == name)
                    return arg.wasParsed;

            throw new ParseException($"Invalid arg: \"{name}\"");
        }

        private string GetUsageHelp()
        {
            StringBuilder buffer = new StringBuilder();
            ArrayList lineParts = new ArrayList();
            ArrayList conflictsHandled = new ArrayList();

            lineParts.Add($"Usage: {name}");

            foreach (var arg in optionalArgs)
            {
                StringBuilder word = new StringBuilder();

                if (conflictsHandled.IndexOf(arg.name) != -1)
                    continue;

                word.Append("[");
                if (arg.type == Arg.Type.BOOLEAN)
                    word.Append($"-{arg.flag}");
                else if (arg.type == Arg.Type.SINGLE)
                    word.Append($"-{arg.flag} {arg.name.ToUpper()}");
                else
                    word.Append($"-{arg.flag} 'ITEM, ...'");

                if (arg.conflicts.Length > 0)
                    foreach (var conflict in arg.conflicts)
                    {
                        conflictsHandled.Add(conflict);
                        if (this[conflict].type == Arg.Type.BOOLEAN)
                            word.Append($"|-{this[conflict].flag}");
                        else if (this[conflict].type == Arg.Type.SINGLE)
                            word.Append($"|-{this[conflict].flag} {this[conflict].name.ToUpper()}");
                        else
                            word.Append($"|-{this[conflict].flag} 'ITEM, ...'");
                    }
                word.Append("]");

                lineParts.Add(word.ToString());
            }

            foreach (var arg in positionalArgs)
                lineParts.Add($"{arg.name.ToUpper()}");

            buffer.Append(TerminalFormat(lineParts, Util.MultiplyString(indent, 2)));
            lineParts.Clear();

            return buffer.ToString();
        }

        private string GetPositionalHelp()
        {
            StringBuilder buffer = new StringBuilder();
            ArrayList lineParts = new ArrayList();

            buffer.Append("Positional arguments:");
            buffer.Append("\n");

            foreach (var arg in positionalArgs)
            {
                if (arg.helpText == "")
                    lineParts.AddRange($"  {arg.name.ToUpper()}".Split(' '));
                else
                    lineParts.AddRange($"  {arg.name.ToUpper()}\n{indent}{arg.helpText}".Split(' '));

                buffer.Append(TerminalFormat(lineParts, indent));
                lineParts.Clear();
            }

            return buffer.ToString();
        }

        private string GetOptionalHelp()
        {
            StringBuilder buffer = new StringBuilder();
            ArrayList lineParts = new ArrayList();

            buffer.Append("Optional arguments:");
            buffer.Append("\n");

            foreach (var arg in optionalArgs)
            {
                if (arg.type == Arg.Type.BOOLEAN)
                    buffer.Append($"  -{arg.flag}, --{arg.name}");
                else if (arg.type == Arg.Type.SINGLE)
                    buffer.Append($"  -{arg.flag} {arg.name.ToUpper()}, --{arg.name} {arg.name.ToUpper()}");
                else
                    buffer.Append($"  -{arg.flag} 'ITEM, ...', --{arg.name} 'ITEM, ...'");
                buffer.Append("\n");

                if (arg.helpText != "")
                {
                    lineParts.AddRange($"{indent}{arg.helpText}".Split(' '));
                    buffer.Append(TerminalFormat(lineParts, indent + "  "));
                    lineParts.Clear();
                }

                if (arg.conflicts.Length > 0)
                {
                    StringBuilder conflictBuffer = new StringBuilder();
                    conflictBuffer.Append($"{indent}Conflicts: ");
                    for (var i = 0; i < arg.conflicts.Length - 1; i++)
                        conflictBuffer.Append($"{arg.conflicts[i]}, ");
                    conflictBuffer.Append($"{arg.conflicts[arg.conflicts.Length - 1]}");
                    lineParts.AddRange(conflictBuffer.ToString().Split(' '));
                    buffer.Append(TerminalFormat(lineParts, indent + "  "));
                    lineParts.Clear();
                }

                if (arg.overrides.Length > 0)
                {
                    StringBuilder overrideBuffer = new StringBuilder();
                    overrideBuffer.Append($"{indent}Overrides: ");
                    for (var i = 0; i < arg.overrides.Length - 1; i++)
                        overrideBuffer.Append($"{arg.overrides[i]}, ");
                    overrideBuffer.Append($"{arg.overrides[arg.overrides.Length - 1]}");
                    lineParts.AddRange(overrideBuffer.ToString().Split(' '));
                    buffer.Append(TerminalFormat(lineParts, indent + "  "));
                    lineParts.Clear();
                }

                if (!(arg.type == Arg.Type.BOOLEAN) && arg.defaultValue != "")
                {
                    lineParts.AddRange($"{indent}Default: {arg.defaultValue}".Split(' '));
                    buffer.Append(TerminalFormat(lineParts, indent + "  "));
                    lineParts.Clear();
                }

                lineParts.Clear();
            }

            return buffer.ToString();
        }

        public string[] args { get; set; }
        public string name { get; set; }
        public int maxWidth { get; set; }
        public string indent { get; set; }
        private List<PositionalArg> positionalArgs;
        private List<OptionalArg> optionalArgs;

        public ArrayList operands { get; set; }
    }
}
