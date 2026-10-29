using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Infrastructure.Utils
{
    public static class EnumHelpers
    {
        /// <summary>Quote_Sent -> "Quote Sent"</summary>
        public static string ToText(this Enum value)
        {
            return value == null ? string.Empty : Convert.ToString(value).Replace("_", " ");
        }

        public static List<string> Texts<TEnum>() where TEnum : struct
        {
            return Enum.GetValues(typeof(TEnum)).Cast<Enum>().Select(e => e.ToText()).ToList();
        }

        public static bool Is(this string text, Enum value)
        {
            return string.Equals((text ?? string.Empty).Trim(), value.ToText(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
