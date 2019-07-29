using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviaGetPhoneSize
{
    class Utility
    {
        public static string StringArrayConcat(String[] f_StringArray)
        {
            string t_OutputString = string.Empty;
            foreach(string t_String in f_StringArray)
            {
                t_OutputString += $"{t_String} ";
            }
            return t_OutputString;
        }
        public static string DictionaryToStringConcat(System.Collections.Specialized.StringDictionary f_StringDictionary)
        {
            string t_OutputString = string.Empty;
            foreach (System.Collections.DictionaryEntry t_DictionaryEntry in f_StringDictionary)
            {
                t_OutputString += $"{t_DictionaryEntry.Key} = {t_DictionaryEntry.Value}";
            }
            return t_OutputString;
        }

    }
}
