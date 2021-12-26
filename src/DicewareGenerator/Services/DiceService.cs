using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DicewareGeneratorTest")]

namespace DicewareGenerator.Services
{
    internal class DiceService
    {
        private readonly Random _rnd;

        public DiceService()
        {
            _rnd = new Random();
        }

        internal static long DiceArrayToInt(int[]? numbers)
        {
            if (numbers == null)
                return 0;

            long result = 0;
            for (int i = 0; i < numbers.Length; i++)
            {
                long multiplier = (long)Math.Pow(10, numbers.Length - 1 - i);
                result += numbers[i] * multiplier;
            }

            return result;
        }

        internal int[]? RollDice(int dices)
        {
            List<int>? results = null;
            for (int i = 0; i < dices; i++)
            {
                if (results == null)
                {
                    results = new List<int>();
                }

                results.Add(RollDie());
            }

            if (results == null)
                return null;

            return results.ToArray();
        }

        internal int RollDie()
        {
            return _rnd.Next(1, 7); // generates values from 1 to 6.
        }

        internal long GetDiceResult(int numDices)
        {
            if (numDices <= 0)
                return 0;

            var arr = RollDice(numDices);
            var result = DiceArrayToInt(arr);

            return result;
        }
    }
}