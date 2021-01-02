using System;
using Xunit;

namespace Book.Chapter3.Listing1
{
    public class CalculatorTests
    {
        [Fact]
        public void Sum_of_two_numbers()
        {
            // Arrange
            double first = 10;
            double second = 20;
            var calculator = new Calculator();

            // Act
            double result = calculator.Sum(first, second);

            // Assert
            Assert.Equal(30, result);
        }
    }

    public class Calculator
    {
        public double Sum(double first, double second)
        {
            return first + second;
        }

        public void CleanUp()
        {
        }
    }

    public class CalculatorTests2 : IDisposable
    {
        private readonly Calculator _calculator;

        public CalculatorTests2()
        {
            _calculator = new Calculator();
        }

        [Fact]
        public void Sum_of_two_numbers()
        {
            // Arrange
            double first = 10;
            double second = 20;

            // Act
            double result = _calculator.Sum(first, second);

            // Assert
            Assert.Equal(30, result);
        }

        public void Dispose()
        {
            _calculator.CleanUp();
        }
    }
}
