# SimpleCli
SimpleCli is a simple command line argument parser which supports:
 - positional and optional arguments 
 - custom validation 
 - default values 
 - boolean, single, and multiple types
 - automatic help text
 - conflicting and overriding arguments 
 - post argument operands

## Example
```csharp
using System;
using SimpleCli;

namespace Test
{
    class Program
    {
        public static bool ValidatePort(string arg)
        {
            try
            {
                var i = int.Parse(arg);
                if(i < 0 || i > 65535)
                    return false;
                else
                    return true;
            }
            catch
            {
                return false;
            }
        } 

        static int Main(string[] args)
        {
            try
            {
                var parser = new Parser("mycli", args);

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
                parser.Add("string-list", 
                           'a', 
                           "This is another optional list argument.", 
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
                           'v', 
                           "This is an overriding argument.", 
                           "1234", 
                           Arg.Type.SINGLE,
                           Validator.AcceptAll,
                           new string[]{},
                           new string[]{"optional"});
                parser.Add("port", 
                           'p', 
                           "This is a custom argument.", 
                           "80", 
                           Arg.Type.SINGLE,
                           ValidatePort);

                parser.Parse();

                var positional = parser["positional"].GetString();
                var optional = parser["optional"].GetInt();
                var flag = parser["flag"].GetBool();
                var list = parser["list"].GetIntArray();
                var stringList = parser["string-list"].GetStringArray();
                var conflicts = parser["conflicts"].GetInt();
                var overrides = parser["overrides"].GetInt();
                var port = parser["port"].GetInt();

                Console.WriteLine($"positional = {positional}");
                Console.WriteLine($"optional = {optional}");
                Console.WriteLine($"flag = {flag}");

                Console.WriteLine("list:");
                foreach(var item in list)
                    Console.WriteLine($"    {item}");
                Console.WriteLine();

                Console.WriteLine("string-list:");
                foreach(var item in stringList)
                    Console.WriteLine($"    {item}");
                Console.WriteLine();

                Console.WriteLine($"conflicts = {conflicts}");
                Console.WriteLine($"overrides = {overrides}");
                Console.WriteLine($"port = {port}");

                Console.WriteLine("Operands:");
                foreach(var item in parser.operands)
                    Console.WriteLine($"    {item}");
                Console.WriteLine();

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
Usage: mycli [-h] [-c CONFLICTS|-o OPTIONAL] [-f] [-l 'ITEM, ...'] [-a 'ITEM, ...']
             [-v OVERRIDES] [-p PORT] POSITIONAL
```
### Full help
```bash
> dotnet run -- --help
Usage: mycli [-h] [-c CONFLICTS|-o OPTIONAL] [-f] [-l 'ITEM, ...'] [-a 'ITEM, ...']
             [-v OVERRIDES] [-p PORT] POSITIONAL
Positional arguments:
  POSITIONAL
        This is a positional argument.
Optional arguments:
  -h, --help
        Displays this help message and exits.
  -o OPTIONAL, --optional OPTIONAL
        This is an optional argument.
        Default: 0
  -f, --flag
        This is an optional flag argument.
  -l 'ITEM, ...', --list 'ITEM, ...'
        This is an optional list argument.
  -a 'ITEM, ...', --string-list 'ITEM, ...'
        This is another optional list argument.
  -c CONFLICTS, --conflicts CONFLICTS
        This is a conflicting argument.
        Conflicts: optional
        Default: 42
  -v OVERRIDES, --overrides OVERRIDES
        This is an overriding argument.
        Overrides: optional
        Default: 1234
  -p PORT, --port PORT
        This is a custom argument.
        Default: 80
```
### Correct parsing
```bash
> dotnet run -- positional -l '1, 2, 3, 4' -a 'This, is, a, string\, list' -o 4 -p80 -f
positional = positional
optional = 4
flag = True
list:
    1
    2
    3
    4

string-list:
    This
    is
    a
    string, list

conflicts = 42
overrides = 1234
port = 80
Operands:
```
### Conflicting parsing error
```bash
> dotnet run -- positional -c 5 -o 4
Argument "conflicts" conflicts with argument "optional"
```
### Overriding parsing
```bash
> dotnet run -- positional -v 1234 -o 4
positional = positional
optional = 1234
flag = False
list:

string-list:

conflicts = 42
overrides = 1234
port = 80
Operands:
```
### Validation error
```bash
> dotnet run -- positional -p1234567
Validation failed for "port" = 1234567
```
### Type conversion error
```bash
> dotnet run -- positional -o 'A string'
Failed to convert "optional" = Int32(A string)
```
### Post argument operands
```bash
> dotnet run -- positional -v 1234 -o 4 -p80 -- some operands
positional = positional
optional = 1234
flag = False
list:

string-list:

conflicts = 42
overrides = 1234
port = 80
Operands:
    some
    operands
```