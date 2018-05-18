using System;
using System.Text;
using System.Collections;

namespace SimpleCli
{
    public class Util
    {
        public static T[] Slice<T>(T[] l, int from, int to)
        {
            if (to < from)
                throw new ParseException($"Invalid slice: {from} > {to}");

            T[] ret = new T[to - from];
            for (int i = from; i < to; i++)
            {
                ret[i - from] = l[i];
            }
            return ret;
        }

        public static string MultiplyString(string str, int count)
        {
            StringBuilder buf = new StringBuilder();
            for (var i = 0; i < count; i++)
                buf.Append(str);

            return buf.ToString();
        }

        public static string[] SplitArray(string str)
        {
            var ret = new ArrayList();
            var buffer = new StringBuilder();

            for(var i = 0; i < str.Length; i++)
            {
                if(str[i] == ',')
                {
                    if(i > 0 && str[i - 1] == '\\')
                    {
                        buffer.Length--;
                        buffer.Append(',');
                    }
                    else
                    {
                        ret.Add(buffer.ToString());
                        buffer.Clear();
                        i++;
                    }
                }
                else
                {
                    buffer.Append(str[i]);
                }
            }
            ret.Add(buffer.ToString());

            return (string[])ret.ToArray(typeof(string));
        }
    }
}