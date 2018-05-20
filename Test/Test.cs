using System;
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
