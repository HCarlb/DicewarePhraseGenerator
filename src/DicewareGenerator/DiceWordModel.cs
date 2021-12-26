using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicewareGenerator
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