using System;

namespace SimpleCli
{
    public class Validator
    {
        public static bool AcceptAll(string arg)
        {
            return true;
        }

        public static bool ValidateRange<T>(string arg, Arg.ConverterDelegate<T> converter, T min, T max)
        {
            dynamic check = converter(arg);
            dynamic dmin = min;
            dynamic dmax = max;
            return dmin <= check && check <= dmax;
        }

        public static bool ValidateInt(string arg, int min, int max)
        {
            return ValidateRange<int>(arg, int.Parse, min, max);
        }

        public static bool ValidateDouble(string arg, double min, double max)
        {
            return ValidateRange<double>(arg, double.Parse, min, max);
        }

        public static bool ValidateStringLength(string arg, int minLength, int maxLength)
        {
            return ValidateInt(arg.Length.ToString(), minLength, maxLength);
        }
    }
}
