using DicewareGenerator.Models;
using DicewareGenerator.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicewareGeneratorTest
{
    [TestClass]
    public class UnitTestWordlistReader
    {
        [TestMethod]
        public void WordlistReader_VerifyDiceCount()
        {
            long expected = 6;

            var input = new DiceWordModel
            {
                DiceValues = 123456,
                Word = "testword",
            };

            var result = WordlistReader.GetDiceCount(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void WordlistReader_DetectIgnorableLines()
        {
            string[] lines = new string[]
            {
                "#123    word1",        // Lines starting with #
                "  #123    word1",      // Lines starting with #
                "             ",        // Empty lines
                "123 abc",              // lines not tab delimited
                "123    abc fed",       // lines with more than 2 tab delimited fields
            };

            foreach (string line in lines)
            {
                var result = WordlistReader.CheckIfLineIsIgnorable(line);
                Assert.IsTrue(result);
            }
        }
    }
}