using DicewareGenerator;
using DicewareGenerator.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DicewareGeneratorTest
{
    [TestClass]
    public class UnitTest1
    {
        private DiceService? _diceService = null;

        [TestMethod]
        public void DiceService_VerifyArrayToInt()
        {
            var expected = 123456;
            var result = DiceService.DiceArrayToInt(new int[] { 1, 2, 3, 4, 5, 6 });

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DiceService_VerifyValuesAreWithinMinAndMax()
        {
            var expectedMin = 1;
            var expectedMax = 6;

            // Roll a whole lot of dice and expec att least one of them to be min and one max.

            var ds = GetDiceService();
            if (ds == null)
            {
                Assert.Fail();
                return;
            }

            var results = ds.RollDice(100);
            if (results == null)
            {
                Assert.Fail();
                return;
            }

            Assert.AreEqual(expectedMin, results.Min());
            Assert.AreEqual(expectedMax, results.Max());
        }

        internal DiceService? GetDiceService()
        {
            if (_diceService == null)
            {
                _diceService = new DiceService();
            }
            return _diceService;
        }

        internal void SetDiceService(DiceService? value)
        {
            _diceService = value;
        }
    }
}