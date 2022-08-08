namespace LittleToySourceGenerator;

using System;

internal static class StringExtension
{
    public static string ToCamel(this string source)
    {
        return Char.ToLowerInvariant(source[0]) + source.Substring(1);
    }
}