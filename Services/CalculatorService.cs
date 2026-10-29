using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using WinArma.Models;

namespace WinArma.Services
{
    public class CalculatorService
    {
        private static readonly DataTable _dataTable = new();

        public SearchResultItem? TryCalculate(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return null;

            var trimmed = query.Trim();

            // Check if string contains math characters
            if (!Regex.IsMatch(trimmed, @"^[\d\s\+\-\*\/\^\(\)\.,%]+$") &&
                !trimmed.StartsWith("sqrt(", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Must contain at least one operator or digit sequence
            if (!Regex.IsMatch(trimmed, @"[\+\-\*\/\^%]|\bsqrt\b"))
            {
                return null;
            }

            try
            {
                string expression = trimmed.Replace(',', '.');

                // Handle percentage (e.g. 50% -> (50/100))
                expression = Regex.Replace(expression, @"(\d+(\.\d+)?)%", "($1/100)");

                // Handle power (e.g. 2^3)
                if (expression.Contains('^'))
                {
                    var match = Regex.Match(expression, @"(\d+(\.\d+)?)\^(\d+(\.\d+)?)");
                    if (match.Success)
                    {
                        double b = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                        double p = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
                        double powResult = Math.Pow(b, p);
                        expression = expression.Replace(match.Value, powResult.ToString(CultureInfo.InvariantCulture));
                    }
                }

                // Handle sqrt
                if (expression.StartsWith("sqrt(", StringComparison.OrdinalIgnoreCase) && expression.EndsWith(")"))
                {
                    var inner = expression.Substring(5, expression.Length - 6);
                    if (double.TryParse(inner, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                    {
                        var sqrtResult = Math.Sqrt(val);
                        return CreateCalculatorResult(sqrtResult.ToString("G", CultureInfo.CurrentCulture), expression);
                    }
                }

                var result = _dataTable.Compute(expression, null);
                if (result != null && double.TryParse(result.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double numResult))
                {
                    if (double.IsInfinity(numResult) || double.IsNaN(numResult))
                    {
                        return null;
                    }

                    string formatted = numResult.ToString("N2", CultureInfo.CurrentCulture).TrimEnd('0').TrimEnd(',');
                    if (string.IsNullOrEmpty(formatted)) formatted = "0";

                    return CreateCalculatorResult(formatted, trimmed);
                }
            }
            catch
            {
                // Not a valid math expression, ignore
            }

            return null;
        }

        private SearchResultItem CreateCalculatorResult(string answer, string originalExpr)
        {
            return new SearchResultItem
            {
                Title = $"= {answer}",
                Subtitle = $"{originalExpr} (Sonucu panoya kopyalamak için Enter)",
                Category = SearchResultCategory.Calculator,
                IconSymbol = "🧮",
                ActionTarget = answer,
                Score = 1200,
                CustomAction = () =>
                {
                    try
                    {
                        System.Windows.Clipboard.SetText(answer);
                    }
                    catch { }
                }
            };
        }
    }
}
