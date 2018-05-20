# SimpleCli
SimpleCli is a simple command line argument parser which supports:
 - Positional arguments
 - Optional arguments 
 - Post argument operands
 - Automatic help and usage text
 - Boolean, single, and multiple argument types
 - Default values 
 - Custom validation delegates
 - Custom type conversion delegates
 - Conflicting arguments
 - Overriding arguments 

## Example
```csharp
﻿using System;
using SimpleCli;

namespace Test
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var parser = new Parser(args, name: "mycli", version: "1.0.0");

                parser.Add("positional",
                           "This is a positional argument.");
                parser.Add("optional",
                           'o',
                           "This is an optional argument.",
                           "0",
                           Arg.Type.SINGLE);
                parser.Add("flag",
                           'f',
                           "This is an optional flag argument.",
                           "false",
                           Arg.Type.BOOLEAN);
                parser.Add("list",
                           'l',
                           "This is an optional list argument.",
                           "",
                           Arg.Type.MULTIPLE);
                parser.Add("conflicts",
                           'c',
                           "This is a conflicting argument.",
                           "42",
                           Arg.Type.SINGLE,
                           Validator.AcceptAll,
                           new string[]{"optional"});
                parser.Add("overrides",
                           'e',
                           "This is an overriding argument.",
                           "1234",
                           Arg.Type.SINGLE,
                           Validator.AcceptAll,
                           new string[]{},
                           new string[]{"optional"});
                parser.Add("rstring",
                           'r',
                           "This is a string validated by regex.",
                           "abc",
                           Arg.Type.SINGLE,
                           Validator.ValidateString("^[a-z]+$"));
                parser.Add("port",
                           'p',
                           "This is an integer validated by range.",
                           "80",
                           Arg.Type.SINGLE,
                           Validator.ValidateInt(0, 65535));

                parser.Parse();

                var positional = parser["positional"].GetString();
                var optional = parser["optional"].GetInt();
                var flag = parser["flag"].GetBool();
                var list = parser["list"].GetIntArray();
                var conflicts = parser["conflicts"].GetInt();
                var overrides = parser["overrides"].GetInt();
                var rstring = parser["rstring"].GetString();
                var port = parser["port"].GetInt();

                Console.WriteLine($"positional = {positional}");
                Console.WriteLine($"optional   = {optional}");
                Console.WriteLine($"flag       = {flag}");
                Console.WriteLine($"conflicts  = {conflicts}");
                Console.WriteLine($"overrides  = {overrides}");
                Console.WriteLine($"rstring    = {rstring}");
                Console.WriteLine($"port       = {port}");
                Console.WriteLine($"list       = {Util.ArrayToString<int>(list)}");
                Console.WriteLine($"operands   = {Util.ArrayToString<string>(parser.operands)}");

                return 0;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
```
### Usage help
```bash
> dotnet run
Usage: mycli [-c CONFLICTS|-o OPTIONAL] [-h] [-v] [-f] [-l 'ITEM, ...'] [-e OVERRIDES]
             [-r RSTRING] [-p PORT] POSITIONAL

```
### Full help
```bash
> dotnet run -- --help
Usage: mycli [-c CONFLICTS|-o OPTIONAL] [-h] [-v] [-f] [-l 'ITEM, ...'] [-e OVERRIDES]
             [-r RSTRING] [-p PORT] POSITIONAL
Positional arguments:
  POSITIONAL
        This is a positional argument.
Optional arguments:
  -h, --help
        Displays this help message and exits.
  -v, --version
        Displays the version.
  -o OPTIONAL, --optional OPTIONAL
        This is an optional argument.
        Default: 0
  -f, --flag
        This is an optional flag argument.
  -l 'ITEM, ...', --list 'ITEM, ...'
        This is an optional list argument.
  -c CONFLICTS, --conflicts CONFLICTS
        This is a conflicting argument.
        Conflicts: optional
        Default: 42
  -e OVERRIDES, --overrides OVERRIDES
        This is an overriding argument.
        Overrides: optional
        Default: 1234
  -r RSTRING, --rstring RSTRING
        This is a string validated by regex.
        Default: abc
  -p PORT, --port PORT
        This is an integer validated by range.
        Default: 80

```
### Correct parsing
```bash
> dotnet run -- positional -l 1,2,3,4 -o 4 -p80 -f
positional = positional
optional   = 4
flag       = True
conflicts  = 42
overrides  = 1234
rstring    = abc
port       = 80
list       = {1, 2, 3, 4}
operands   = {}
```
### Overriding parsing
```bash
> dotnet run -- positional -e 1234 -o 4
positional = positional
optional   = 1234
flag       = False
conflicts  = 42
overrides  = 1234
rstring    = abc
port       = 80
list       = {}
operands   = {}
```
### Post argument operands
```bash
> dotnet run -- positional -e 1234 -o 4 -p80 -- some operands
positional = positional
optional   = 1234
flag       = False
conflicts  = 42
overrides  = 1234
rstring    = abc
port       = 80
list       = {}
operands   = {some, operands}
```
### Conflicting parsing error
```bash
> dotnet run -- positional -c 5 -o 4
Argument 'conflicts' conflicts with argument 'optional'
```
### Validation error
```bash
> dotnet run -- positional -p1234567
Validation failed for 'port' = 1234567
```
### Type conversion error
```bash
> dotnet run -- positional -o "string"
Failed to convert 'optional' = Int32("string")
```