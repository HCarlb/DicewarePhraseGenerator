using DicewareGenerator.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicewareGenerator.Services
{
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

        internal static IEnumerable<DiceWordModel>? ParseWordlist(IEnumerable<string> rowdata, string separator = "\t")
        {
            ArgumentNullException.ThrowIfNull(rowdata);

            foreach (var row in rowdata)
            {
                var rowData = row.Trim().Split(separator);

                // Make sure we only get arrays with length of 2.
                if (rowData.Length != 2)
                    continue;

                yield return new DiceWordModel()
                {
                    DiceValues = long.Parse(rowData[0]),
                    Word = rowData[1].Trim(),
                };
            }
        }

        internal static int GetDiceCount(DiceWordModel diceWord)
        {
            return diceWord.DiceCount;
        }
    }
}