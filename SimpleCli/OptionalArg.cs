using System;

namespace SimpleCli
{
    public class OptionalArg : Arg
    {
        public OptionalArg(string name,
                           char flag,
                           string helpText,
                           string defaultValue,
                           Type type,
                           ValidatorDelegate validator,
                           string[] conflicts,
                           string[] overrides)
        : base(name, helpText, validator)
        {
            this.flag = flag;
            this.defaultValue = defaultValue;
            this.type = type;
            this.conflicts = conflicts;
            this.overrides = overrides;
            wasParsed = false;
        }

        public override T Get<T>(ConverterDelegate<T> converter)
        {
            try
            {
                return base.Get<T>(converter);
            }
            catch
            {
                if(wasParsed)
                    throw new ParseException($"Failed to convert '{name}' = {typeof(T).Name}({value})");
                else
                {
                    value = defaultValue;
                    return base.Get<T>(converter);
                }
            }
        }

        public override T[] GetArray<T>(ConverterDelegate<T> converter)
        {
            try
            {
                return base.GetArray<T>(converter);
            }
            catch
            {
                if(wasParsed)
                    throw new ParseException($"Failed to convert '{name}' = {typeof(T).Name}[]({value})");
                else
                {
                    value = defaultValue;
                    return base.GetArray<T>(converter);
                }
            }
        }

        public string defaultValue { get; set; }
        public string[] conflicts { get; set; }
        public string[] overrides { get; set; }
    }
}
