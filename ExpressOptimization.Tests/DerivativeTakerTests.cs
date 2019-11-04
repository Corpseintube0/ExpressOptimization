using ExpressOptimization.Library;
using NUnit.Framework;

namespace ExpressOptimization.Tests
{
    /// <summary>
    /// Tests for <see cref="DerivativeTaker"/>.
    /// </summary>
    [TestFixture]
    public class DerivativeTakerTests
    {
        /// <summary>Final form of the expression test.</summary>
        [Test]
        [TestCase("2*x^2", "4*x")]
        [TestCase("cos(-1*x)", "-1*sin(x)")]
        public void DerivativeFinalFormTest(string input, string expectedResult)
        {
            // arrange
            var dt = new DerivativeTaker();
            
            // act
            var result = dt.Derivation(input, "x");

            // assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}
