using System;
using System.Linq;
using System.Collections.Generic;

namespace SimpleCli
{
    public class Arg
    {
        public enum Type { BOOLEAN, SINGLE, MULTIPLE };
        public delegate bool ValidatorDelegate(string arg);
        public delegate T ConverterDelegate<T>(string value);

        public static T ReturnValue<T>(T value)
        {
            return value;
        }

        public Arg(string name, string helpText, ValidatorDelegate validator)
        {
            this.name = name;
            this.flag = ' ';
            this.type = Type.SINGLE;
            this.helpText = helpText;
            this.validator = validator;
        }

        public virtual T Get<T>(ConverterDelegate<T> converter)
        {
            try
            {
                return converter(value);
            }
            catch
            {
                throw new ParseException($"Failed to convert \"{name}\" = {typeof(T).Name}({value})");
            }
        }

        public virtual string GetString()
        {
            return Get<string>(ReturnValue<string>);
        }

        public virtual int GetInt()
        {
            return Get<int>(int.Parse);
        }

        public virtual bool GetBool()
        {
            return Get<bool>(bool.Parse);
        }

        public virtual double GetDouble()
        {
            return Get<double>(double.Parse);
        }

        public virtual T[] GetArray<T>(ConverterDelegate<T> converter)
        {
            try
            {
                if(value.Length > 0)
                {
                    var arr = Util.SplitArray(value);
                    var ret = from s in arr select converter(s.Trim());
                    return ret.ToArray();
                }
                else
                    return new T[]{};
            }
            catch
            {
                throw new ParseException($"Failed to convert \"{name}\" = {typeof(T).Name}[]({value})");
            }
        }

        public virtual string[] GetStringArray()
        {
            return GetArray<string>(ReturnValue<string>);
        }

        public virtual int[] GetIntArray()
        {
            return GetArray<int>(int.Parse);
        }

        public virtual bool[] GetBoolArray()
        {
            return GetArray<bool>(bool.Parse);
        }

        public virtual double[] GetDoubleArray()
        {
            return GetArray<double>(double.Parse);
        }

        public void Validate()
        {
            if (!validator(value))
                throw new ParseException($"Validation failed for \"{name}\" = {value}");
        }

        public string name { get; set; }
        public char flag { get; set; }
        public string helpText { get; set; }
        public string value { get; set; }
        public Type type { get; set; }
        public ValidatorDelegate validator { get; set; }
        public bool wasParsed { get; set; }
    }
}
