using System;

namespace CsCodeGenerator
{
    // This is reduced version of https://github.com/borisdj/CsCodeGenerator/blob/master/CsCodeGenerator/CsGenerator.cs
    // to make this code work in Roslyn context.
    public class CsGenerator
    {
        public static int DefaultTabSize = 4;
        public static string IndentSingle => new String(' ', CsGenerator.DefaultTabSize);
    }
}
