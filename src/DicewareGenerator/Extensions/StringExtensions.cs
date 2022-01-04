namespace DicewareGenerator.Extensions;

internal static class StringExtensions
{
    public static int WordCount(this string text, string? separator)
    {
        return text.Split(separator).Length;
    }
}