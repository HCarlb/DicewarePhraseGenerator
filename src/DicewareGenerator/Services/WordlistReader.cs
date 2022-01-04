using System.IO;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DicewareGeneratorTest")]

namespace DicewareGenerator.Services;

internal class WordlistReader
{
    internal static bool CheckIfLineIsIgnorable(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return true;
        if (string.IsNullOrEmpty(text)) return true;
        if (text.Trim().Length == 0) return true;
        if (text.Trim()[0..1] == "#") return true;
        if (text.Split("\t").Length != 2) return true;
        return false;
    }

    internal static IEnumerable<string>? LoadWordlist(string file)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException($"Cannot find file {file}");

        using var sr = new StreamReader(file);
        while (sr.Peek() >= 0)
        {
            var line = sr.ReadLine();
            ArgumentNullException.ThrowIfNull(line);

            if (!CheckIfLineIsIgnorable(line))
                yield return line.Trim();
        }
    }

    internal static Dictionary<long, string> ParseWordlist(IEnumerable<string> rowdata, string separator = "\t")
    {
        ArgumentNullException.ThrowIfNull(rowdata);

        var result = new Dictionary<long, string>();

        foreach (var row in rowdata)
        {
            var rowData = row.Trim().Split(separator);

            // Make sure we only get arrays with length of 2.
            if (rowData.Length != 2)
                continue;

            result.Add(long.Parse(rowData[0]), rowData[1].Trim());
        }

        return result;
    }
}