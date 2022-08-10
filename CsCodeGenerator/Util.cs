using CsCodeGenerator.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CsCodeGenerator
{
    internal static class Util
    {
        public static string Using = "using";
        public static string Namespace = "namespace";
        public static string Class = "class";
        public static string Struct = "struct";
        public static string Enum = "enum";
        public static string Interface = "interface";
        public static string Base = "base";
        public static string CsExtension = "cs";
        public static string TxtExtension = "txt";
        public static string NewLine = System.Environment.NewLine;
        public static string NewLineDouble = NewLine + NewLine;

        public static string LowerFirst(this string s)
        {
            if (s != string.Empty && char.IsUpper(s[0]))
            {
                s =  char.ToLower(s[0]) + s.Substring(1);
            }
            return s;
        }
    }
}
