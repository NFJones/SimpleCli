using System;
using System.Text.RegularExpressions;

namespace SimpleCli
{
    public class Validator
    {
        public static bool AcceptAll(string arg)
        {
            return true;
        }

        public static bool ValidateRange<T>(T arg, T min, T max)
        {
            dynamic darg = arg;
            dynamic dmin = min;
            dynamic dmax = max;
            return dmin <= darg && darg <= dmax;
        }

        public static bool ValidateRange<T>(string arg, Arg.ConverterDelegate<T> converter, T min, T max)
        {
            dynamic darg = converter(arg);
            dynamic dmin = min;
            dynamic dmax = max;
            return ValidateRange(darg, dmin, dmax);
        }

        public static Arg.ValidatorDelegate ValidateInt(int min, int max)
        {
            Arg.ValidatorDelegate ret = (string arg) => {
                return ValidateRange(arg, int.Parse, min, max);
            };
            return ret;
        }

        public static Arg.ValidatorDelegate ValidateDouble(double min, double max)
        {
            Arg.ValidatorDelegate ret = (string arg) => {
                return ValidateRange(arg, double.Parse, min, max);
            };
            return ret;
        }

        public static Arg.ValidatorDelegate ValidateString(int min, int max)
        {
            Arg.ValidatorDelegate ret = (string arg) => {
                return ValidateRange(arg.Length, min, max);
            };
            return ret;
        }

        public static Arg.ValidatorDelegate ValidateString(string regex)
        {
            Arg.ValidatorDelegate ret = (string arg) => {
                Regex pattern = new Regex(regex);
                return pattern.IsMatch(arg);
            };
            return ret;
        }
    }
}
