using System;
using System.Text.RegularExpressions;

namespace PolicyTrackerWebService.Infrastructure.Utils
{
    public static class ExtensionMethods
    {
        public static bool IsNotNullOrEmpty(this string inputtedString)
        {
            return !string.IsNullOrWhiteSpace(inputtedString);
        }

        public static bool IsNullOrEmpty(this string inputtedString)
        {
            return string.IsNullOrWhiteSpace(inputtedString);
        }

        public static string StrToUpper(this string inputtedString)
        {
            return inputtedString.IsNotNullOrEmpty() ? inputtedString.Trim().ToUpperInvariant() : inputtedString;
        }

        public static string TrimOrNull(this string inputtedString)
        {
            return inputtedString.IsNotNullOrEmpty() ? inputtedString.Trim() : null;
        }

        /// <summary>Policy numbers are compared without spaces / case: "2302 2086 0585" == "230220860585".</summary>
        public static string NormalizePolicyNo(this string policyNo)
        {
            if (policyNo.IsNullOrEmpty()) return string.Empty;
            return Regex.Replace(policyNo, @"\s+", string.Empty).ToUpperInvariant();
        }

        public static DateTime FirstOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime? FirstOfMonth(this DateTime? date)
        {
            return date.HasValue ? date.Value.FirstOfMonth() : (DateTime?)null;
        }

        public static decimal RoundMoney(this decimal value)
        {
            return Math.Round(value, 0, MidpointRounding.AwayFromZero);
        }
    }
}
