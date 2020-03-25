using System;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Calculator.Tests
{
    [TestClass]
    public class ProgramFacts
    {
        private readonly double floatingPointTollerance = 0.000000001;

        [TestMethod]
        [DataRow("(100+200)*3+99", 999, DisplayName = "Example input 1")]
        [DataRow("6^2+1", 37, DisplayName = "Example input 2")]
        [DataRow("1/3", 0.3333333333333333333333333, DisplayName = "Decimal result")]
        [DataRow("5-9", -4, DisplayName = "Negative result")]
        [DataRow("((100+1)^3)*((4^16)+255)", 4425109362762851, DisplayName = "Large result")]
        [DataRow("11^2/50", 2.42, DisplayName = "Exponent priority")]
        [DataRow("(2^(2^(1^(20/5))))^2", 16, DisplayName ="Exponentaion of parenthases")]
        public void FindResult_WhenExpressionValid_ReturnsCorrectResult(string expression, double correctResult)
        {
            // Arrange & Act
            var result = Program.FindResult(expression);

            // Assert
            Assert.IsTrue(Math.Abs(result - correctResult) < floatingPointTollerance);
        }

        [TestMethod]
        [DataRow("85m/2^2/500", DisplayName = "Illegal characters")]
        [DataRow("(2147483648*1)+500", DisplayName = "Non-32-bit integer")]
        [DataRow("-200^3", DisplayName = "Number prefixing front")]
        [DataRow("4^(3753+-9)", DisplayName = "Number prefixing inside")]
        public void FindResult_WhenExpressionIsInvalid_ThrowsArgumentException(string expression)
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentException>(() => Program.FindResult(expression));
        }

        [TestMethod]
        [DataRow("1+*8", DisplayName = "Too many operators")]
        [DataRow("(1+2*5", DisplayName = "Open parenthases")]
        public void FindResult_WhenExpressionContainsMalformedSyntax_ThrowsSyntaxErrorException(string expression)
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<SyntaxErrorException>(() => Program.FindResult(expression));
        }

        [TestMethod]
        [DataRow("455^455", DisplayName = "Exponentation too large")]
        [DataRow("2147483647+1", DisplayName = "Addition is too large")]
        public void FindResult_WhenResultTooLargeToCalculate_ThrowsOverflowException(string expression)
        {
            // Arrange, Act & Assert
            Assert.ThrowsException<OverflowException>(() => Program.FindResult(expression));
        }
    }
}
