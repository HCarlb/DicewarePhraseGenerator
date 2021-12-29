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

        internal static List<string>? LoadWordlist(string file)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Cannot find file {file}");

            var rows = new List<string>();
            try
            {
                using var sr = new StreamReader(file);
                while (sr.Peek() >= 0)
                {
                    var x = sr.ReadLine();
                    if (x != null && !CheckIfLineIsIgnorable(x))
                        rows.Add(x.Trim());
                }
                return rows;
            }
            catch (Exception e)
            {
                throw new FileLoadException($"Failed to read file {file}.{Environment.NewLine}{e.Message}");
            }
        }

        internal static List<DiceWordModel>? ParseWordlist(List<string>? rowdata, string separator = "\t")
        {
            if (rowdata == null || rowdata.Count == 0)
                throw new ArgumentNullException();

            var words = new List<DiceWordModel>();
            foreach (var row in rowdata)
            {
                var rowData = row.Trim().Split(separator);

                // Make sure we only get arrays withg length of 2.
                if (rowData.Length != 2)
                    continue;

                words.Add(new DiceWordModel()
                {
                    DiceValues = long.Parse(rowData[0]),
                    Word = rowData[1].Trim(),
                });
            }

            if (words.Count == 0)
                throw new FileFormatException();

            return words;
        }

        internal int GetDiceCount(DiceWordModel diceWord)
        {
            return diceWord.DiceCount;
        }
    }
}