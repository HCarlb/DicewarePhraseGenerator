using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicewareGenerator.Extensions
{
    internal static class StringExtensions
    {
        public static int WordCount(this string text, string? separator)
        {
            return text.Split(separator).Length;
        }
    }
}