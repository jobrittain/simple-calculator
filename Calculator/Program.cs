using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calculator
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var expression = Console.ReadLine();

                if (expression != string.Empty)
                {
                    try
                    {
                        var result = FindResult(expression);
                        Console.Write(result);
                    }
                    catch (ArgumentException e)
                    {
                        Console.Write($"The input is invalid. {e.Message}");
                    }
                    catch (SyntaxErrorException e)
                    {
                        Console.Write($"The expression syntax is malformed. {e.Message}");
                    }
                    catch (OverflowException)
                    {
                        Console.Write("The result is too large to calculate.");
                    }
                    catch (Exception)
                    {
                        Console.Write($"An unexpected error occured.");
                    }
                }
                else
                {
                    Console.Write("Please enter a mathematical expression.");
                }

                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                }
                Console.WriteLine();
            }
        }

        internal static double FindResult(string expression)
        {
            if (new Regex(@"[^+\-*/^()\d]").IsMatch(expression))
            {
                throw new ArgumentException("The expression contains illegal characters.");
            }

            if (new Regex(@"^[+-][\d()]|[+\-*/^][+-][\d()]").IsMatch(expression))
            {
                throw new ArgumentException(
                    "The expression contains number prefixes which are unsupported.");
            }

            if (ContainsNon32BitIntegers(expression))
            {
                throw new ArgumentException(
                    "The expression contains values that cannot be interpreted as positive 32 bit integers.");
            }

            return Calculate(expression);
        }

        private static bool ContainsNon32BitIntegers(string expression)
        {
            var values = expression.Split('+', '-', '*', '/', '^', '(', ')').Where(x => x != string.Empty);

            try
            {
                foreach (var value in values)
                {
                    int.Parse(value);
                }
            }
            catch
            {
                return true;
            }

            return false;
        }

        private static double Calculate(string expression)
        {
            // DataColumn expressions supports + - * / operators but not ^
            // so the exponent calculations must be performed seperately.
            // The order of operations in maths appears to be a convenient truth.
            expression = CalculateExponents(expression);
            
            var result = new DataTable().Compute(expression, string.Empty);
            return double.Parse(result.ToString());
        }

        private static string CalculateExponents(string expression)
        {
            if (!expression.Contains('^'))
            {
                return expression;
            }

            do
            {
                var squareIndex = expression.IndexOf('^');

                var baseValue = GetBase(expression, squareIndex, out int firstBaseCharIndex);
                var powerValue = GetPower(expression, squareIndex, out int lastPowerCharIndex);

                var result = Math.Pow(baseValue, powerValue).ToString();
                if (result.Contains("∞"))
                {
                    throw new OverflowException();
                }

                var preExponent = expression.Substring(0, firstBaseCharIndex);
                var postExponent = expression.Substring(lastPowerCharIndex + 1);

                expression = preExponent + result + postExponent;

            } while (expression.Contains('^'));

            return expression;
        }

        private static double GetBase(string parentExpression, int exponentOperatorIndex, out int firstBaseCharIndex)  
        {
            var baseExpression = string.Empty;
            var bracketLevel = 0;

            firstBaseCharIndex = 0;

            for (int i = exponentOperatorIndex - 1; i >= 0; i--)
            {
                if (parentExpression[i] == ')')
                {
                    bracketLevel++;
                }
                else if (bracketLevel == 0 && !char.IsDigit(parentExpression[i]))
                {
                    firstBaseCharIndex = i + 1;
                    break;
                }
                else if (parentExpression[i] == '(')
                {
                    bracketLevel--;
                }

                baseExpression = parentExpression[i] + baseExpression;
            }

            // Recurse to eliminate potential child exponents
            return Calculate(baseExpression);
        }

        private static double GetPower(string parentExpression, int exponentOperatorIndex, out int lastPowerCharIndex)
        {
            var powerExpression = string.Empty;
            var bracketLevel = 0;

            lastPowerCharIndex = parentExpression.Length - 1;

            for (int i = exponentOperatorIndex + 1; i < parentExpression.Length; i++)
            {
                if (parentExpression[i] == '(')
                {
                    bracketLevel++;
                }
                else if (bracketLevel == 0 && !char.IsDigit(parentExpression[i]))
                {
                    lastPowerCharIndex = i - 1;
                    break;
                }
                else if (parentExpression[i] == ')')
                {
                    bracketLevel--;
                }

                powerExpression = powerExpression + parentExpression[i];
            }

            // Recurse to eliminate potential child exponents
            return Calculate(powerExpression);
        }
    }
}
