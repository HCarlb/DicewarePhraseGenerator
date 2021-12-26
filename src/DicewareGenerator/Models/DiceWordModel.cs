using System;

namespace DicewareGenerator.Models
{
    public class DiceWordModel
    {
        public Int64 DiceValues { get; set; }
        public string? Word { get; set; }

        public int DiceCount
        {
            get
            {
                if (DiceValues == 0) return 0;
                return DiceValues.ToString().Length;
            }
        }
    }
}