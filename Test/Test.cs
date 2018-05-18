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
