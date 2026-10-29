using PolicyTrackerWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PolicyTrackerWebService.Service.Helpers
{
    /// <summary>
    /// Converts the broker's commission statement (rows from Excel) into statement lines.
    /// Handles the problems seen in real broker files:
    ///  - two-row headers ("OD" / "Premium"),
    ///  - rows with or without GST / Gross columns in the same sheet,
    ///  - health points typed in the "Points on OD" column,
    ///  - dates read by Excel as month-first (03-07-26 shown as 7-Mar),
    ///  - policy numbers turned into 2.30E+19 or "0".
    /// </summary>
    public static class StatementParser
    {
        private enum Col { Start, Expiry, SalesManager, PosName, PosCode, Customer, Company, Product, PolicyNo, Od, Tp, Net, Gst, Gross, PointsOd, PointsTp, PointsNet, Value, Remarks }

        private static readonly string[] DateFormats =
        {
            "dd-MM-yyyy", "d-M-yyyy", "dd-MM-yy", "d-M-yy",
            "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "d/M/yy",
            "dd-MMM-yy", "d-MMM-yy", "dd-MMM-yyyy", "d-MMM-yyyy",
            "dd MMM yyyy", "d MMM yyyy", "yyyy-MM-dd", "dd.MM.yyyy"
        };

        public static List<Domain.StatementLine> Parse(List<object[]> rows, DateTime? statementMonth, List<string> warnings)
        {
            var result = new List<Domain.StatementLine>();
            if (rows == null || rows.Count == 0) return result;

            int headerRow;
            var map = FindHeader(rows, out headerRow);
            if (map == null)
            {
                warnings.Add("Could not find the header row (expected columns such as 'Customer Name' and 'Policy Number').");
                return result;
            }

            int policyCol = map[Col.PolicyNo];
            int customerCol = map[Col.Customer];

            for (var r = headerRow + 1; r < rows.Count; r++)
            {
                var row = rows[r];
                var rowNo = r + 1;

                var customer = Text(Cell(row, customerCol));
                var rawPolicy = Cell(row, policyCol);
                if (customer.IsNullOrEmpty() && Text(rawPolicy).IsNullOrEmpty()) continue;
                if (IsHeaderContinuation(row)) continue;

                var line = new Domain.StatementLine
                {
                    SalesManager = Text(Cell(row, Get(map, Col.SalesManager))),
                    PosName = Text(Cell(row, Get(map, Col.PosName))),
                    PosCode = Text(Cell(row, Get(map, Col.PosCode))),
                    CustomerName = CleanName(customer),
                    InsuranceCompany = CleanName(Text(Cell(row, Get(map, Col.Company)))),
                    Product = CleanName(Text(Cell(row, Get(map, Col.Product)))),
                };

                // ---- policy number ----
                string policyWarning;
                line.PolicyNo = ReadPolicyNo(rawPolicy, out policyWarning);
                if (policyWarning != null)
                {
                    line.Remarks = policyWarning;
                    warnings.Add(string.Format("Row {0} ({1}): {2}", rowNo, line.CustomerName, policyWarning));
                }

                // ---- numbers (layout detected per row) ----
                if (!ReadAmounts(row, map, line))
                {
                    warnings.Add(string.Format("Row {0} ({1}): no commission value found – row skipped.", rowNo, line.CustomerName));
                    continue;
                }

                // ---- dates ----
                var start = ReadDateCandidates(Cell(row, Get(map, Col.Start)));
                var expiry = ReadDateCandidates(Cell(row, Get(map, Col.Expiry)));
                bool swapped;
                ResolveDates(start, expiry, out var startDate, out var expiryDate, out swapped);
                line.StartDate = startDate;
                line.ExpiryDate = expiryDate;
                if (swapped)
                    warnings.Add(string.Format("Row {0} ({1}): dates were read month-first in the broker file – corrected to {2:dd-MMM-yyyy} → {3:dd-MMM-yyyy}.", rowNo, line.CustomerName, startDate, expiryDate));

                // ---- statement month ----
                if (statementMonth.HasValue)
                    line.StatementMonth = statementMonth.Value.FirstOfMonth();
                else if (line.StartDate.HasValue)
                    line.StatementMonth = line.StartDate.Value.FirstOfMonth();
                else
                {
                    warnings.Add(string.Format("Row {0} ({1}): no start date and no statement month selected – row skipped.", rowNo, line.CustomerName));
                    continue;
                }

                result.Add(line);
            }

            return result;
        }

        // ------------------------------------------------------------------ header

        private static Dictionary<Col, int> FindHeader(List<object[]> rows, out int headerRow)
        {
            headerRow = -1;
            var limit = Math.Min(rows.Count, 30);

            for (var r = 0; r < limit; r++)
            {
                var texts = rows[r].Select(c => Normalize(Text(c))).ToArray();
                var hasPolicy = texts.Any(t => t.Contains("policy") && (t.Contains("number") || t.Contains("no")));
                var hasCustomer = texts.Any(t => t.Contains("customer") || t.Contains("insured name"));
                if (!hasPolicy || !hasCustomer) continue;

                // merge a continuation row such as "Premium" under "OD"
                var labels = texts.ToArray();
                var last = r;
                if (r + 1 < rows.Count && IsHeaderContinuation(rows[r + 1]))
                {
                    var next = rows[r + 1];
                    labels = new string[Math.Max(labels.Length, next.Length)];
                    for (var i = 0; i < labels.Length; i++)
                    {
                        var a = i < texts.Length ? texts[i] : string.Empty;
                        var b = Normalize(Text(Cell(next, i)));
                        labels[i] = (a + " " + b).Trim();
                    }
                    last = r + 1;
                }

                var map = MapColumns(labels);
                if (map.ContainsKey(Col.PolicyNo) && map.ContainsKey(Col.Customer))
                {
                    headerRow = last;
                    return map;
                }
            }
            return null;
        }

        private static bool IsHeaderContinuation(object[] row)
        {
            var values = row.Where(c => c != null && Text(c).IsNotNullOrEmpty()).ToList();
            if (values.Count == 0 || values.Count > 6) return false;
            return values.All(c => c is string && !IsNumberText((string)c) && Normalize((string)c).Length <= 15);
        }

        private static Dictionary<Col, int> MapColumns(string[] labels)
        {
            var map = new Dictionary<Col, int>();
            for (var i = 0; i < labels.Length; i++)
            {
                var t = labels[i];
                if (t.IsNullOrEmpty()) continue;

                Col? col = null;
                if (t.Contains("points") || t.Contains("rate") || t.Contains("%"))
                {
                    if (t.Contains("net")) col = Col.PointsNet;
                    else if (t.Contains("tp") || t.Contains("terror")) col = Col.PointsTp;
                    else if (t.Contains("od")) col = Col.PointsOd;
                    else col = Col.PointsNet;
                }
                else if (t.Contains("start") || t.Contains("risk inception") || t == "from") col = Col.Start;
                else if (t.Contains("expiry") || t.Contains("end date") || t == "to") col = Col.Expiry;
                else if (t.Contains("sales manager") || t.Contains("rm name")) col = Col.SalesManager;
                else if (t.Contains("pos name")) col = Col.PosName;
                else if (t.Contains("pos code")) col = Col.PosCode;
                else if (t.Contains("customer") || t.Contains("insured name")) col = Col.Customer;
                else if (t.Contains("insurance company") || t.Contains("insurer")) col = Col.Company;
                else if (t.Contains("product") || t.Contains("plan")) col = Col.Product;
                else if (t.Contains("policy") && (t.Contains("number") || t.Contains("no"))) col = Col.PolicyNo;
                else if (t.StartsWith("od")) col = Col.Od;
                else if (t.StartsWith("tp") || t.Contains("terrorism")) col = Col.Tp;
                else if (t.StartsWith("net")) col = Col.Net;
                else if (t.StartsWith("gst") || t.Contains("tax")) col = Col.Gst;
                else if (t.StartsWith("gross") || t.Contains("total premium")) col = Col.Gross;
                else if (t.StartsWith("value") || t.Contains("commission") || t.Contains("payout") || t.Contains("brokerage")) col = Col.Value;
                else if (t.StartsWith("remark")) col = Col.Remarks;

                if (col.HasValue && !map.ContainsKey(col.Value)) map[col.Value] = i;
            }
            return map;
        }

        // ------------------------------------------------------------------ amounts

        /// <summary>
        /// The amount block after the policy number is either
        ///   OD, TP, Net, PtsOD, PtsTP, PtsNet, Value            (compact)
        ///   OD, TP, Net, GST, Gross, PtsOD, PtsTP, PtsNet, Value (with tax)
        /// and both layouts can appear in the same sheet, so it is detected per row
        /// from the position of the last number (the commission value).
        /// </summary>
        private static bool ReadAmounts(object[] row, Dictionary<Col, int> map, Domain.StatementLine line)
        {
            var first = map.ContainsKey(Col.Od) ? map[Col.Od] : map[Col.PolicyNo] + 1;

            var lastNumeric = -1;
            for (var i = row.Length - 1; i >= first; i--)
            {
                if (Number(Cell(row, i)).HasValue) { lastNumeric = i; break; }
            }
            if (lastNumeric < 0) return false;

            var offset = lastNumeric - first;
            decimal? od = Number(Cell(row, first)), tp = Number(Cell(row, first + 1)), net = Number(Cell(row, first + 2));
            decimal? gst = null, pOd = null, pTp = null, pNet = null;

            if (map.ContainsKey(Col.Value) && map[Col.Value] == lastNumeric)
            {
                // header layout matches the row
                od = Number(Cell(row, Get(map, Col.Od)));
                tp = Number(Cell(row, Get(map, Col.Tp)));
                net = Number(Cell(row, Get(map, Col.Net)));
                gst = Number(Cell(row, Get(map, Col.Gst)));
                pOd = Number(Cell(row, Get(map, Col.PointsOd)));
                pTp = Number(Cell(row, Get(map, Col.PointsTp)));
                pNet = Number(Cell(row, Get(map, Col.PointsNet)));
            }
            else if (offset == 8)
            {
                gst = Number(Cell(row, first + 3));
                pOd = Number(Cell(row, first + 5));
                pTp = Number(Cell(row, first + 6));
                pNet = Number(Cell(row, first + 7));
            }
            else if (offset == 6)
            {
                pOd = Number(Cell(row, first + 3));
                pTp = Number(Cell(row, first + 4));
                pNet = Number(Cell(row, first + 5));
            }
            else if (offset == 7)
            {
                gst = Number(Cell(row, first + 3));
                pOd = Number(Cell(row, first + 4));
                pTp = Number(Cell(row, first + 5));
                pNet = Number(Cell(row, first + 6));
            }

            line.Value = Number(Cell(row, lastNumeric)) ?? 0m;
            line.OdPremium = od;
            line.TpPremium = tp;
            line.NetPremium = net ?? ((od ?? 0) + (tp ?? 0));
            line.GstAmount = gst;

            // Health / non-motor rows often carry their points in the "Points on OD" column.
            if (!pNet.HasValue && pOd.HasValue && (tp ?? 0m) == 0m && !pTp.HasValue)
            {
                pNet = pOd;
                pOd = null;
            }

            line.PointsOd = pOd;
            line.PointsTp = pTp;
            line.PointsNet = pNet;
            return true;
        }

        // ------------------------------------------------------------------ dates

        private class DateCandidates
        {
            public DateTime? Value;
            public DateTime? Swapped;
        }

        private static DateCandidates ReadDateCandidates(object cell)
        {
            var c = new DateCandidates();
            if (cell == null) return c;

            if (cell is DateTime)
            {
                c.Value = ((DateTime)cell).Date;
            }
            else if (cell is double)
            {
                var d = (double)cell;
                if (d > 20000 && d < 80000) c.Value = DateTime.FromOADate(d).Date;
            }
            else
            {
                var s = Text(cell).Replace(' ', ' ').Trim();
                DateTime parsed;
                if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
                    c.Value = parsed.Date;
                else if (DateTime.TryParse(s, CultureInfo.GetCultureInfo("en-IN"), DateTimeStyles.AllowWhiteSpaces, out parsed))
                    c.Value = parsed.Date;
            }

            if (c.Value.HasValue && c.Value.Value.Day <= 12 && c.Value.Value.Day != c.Value.Value.Month)
            {
                var v = c.Value.Value;
                c.Swapped = new DateTime(v.Year, v.Day, v.Month);
            }
            return c;
        }

        /// <summary>
        /// A policy normally ends the day before its start date N years later. When the dates as read
        /// don't fit that rule but the day/month-swapped dates do, the file was read month-first.
        /// </summary>
        private static void ResolveDates(DateCandidates start, DateCandidates expiry, out DateTime? startDate, out DateTime? expiryDate, out bool swapped)
        {
            startDate = start.Value;
            expiryDate = expiry.Value;
            swapped = false;

            if (!start.Value.HasValue || !expiry.Value.HasValue) return;
            if (Consistent(start.Value.Value, expiry.Value.Value)) return;

            var starts = new[] { start.Value, start.Swapped };
            var expiries = new[] { expiry.Value, expiry.Swapped };

            foreach (var s in starts)
            {
                foreach (var e in expiries)
                {
                    if (!s.HasValue || !e.HasValue) continue;
                    if (Consistent(s.Value, e.Value))
                    {
                        startDate = s;
                        expiryDate = e;
                        swapped = true;
                        return;
                    }
                }
            }
        }

        private static bool Consistent(DateTime start, DateTime expiry)
        {
            if (expiry <= start) return false;
            var next = expiry.AddDays(1);
            return next.Day == start.Day && next.Month == start.Month && (next.Year - start.Year) >= 1 && (next.Year - start.Year) <= 10;
        }

        // ------------------------------------------------------------------ policy number

        private static string ReadPolicyNo(object cell, out string warning)
        {
            warning = null;
            if (cell == null)
            {
                warning = "Policy number is blank in the broker file.";
                return string.Empty;
            }

            if (cell is double)
            {
                var d = (double)cell;
                if (Math.Abs(d) < 0.5)
                {
                    warning = "Policy number is blank ('0') in the broker file.";
                    return string.Empty;
                }
                if (Math.Abs(d) >= 1e15)
                {
                    warning = "Policy number is corrupted in the broker file (Excel turned it into " + d.ToString("0.###E+0", CultureInfo.InvariantCulture) + ") – ask for the full number.";
                    return d.ToString("0.#############E+0", CultureInfo.InvariantCulture);
                }
                return d.ToString("0", CultureInfo.InvariantCulture);
            }

            var s = Regex.Replace(Text(cell).Replace(' ', ' '), @"\s{2,}", " ").Trim();
            if (s == "0" || s == "-" || s.Length == 0)
            {
                warning = "Policy number is blank ('" + s + "') in the broker file.";
                return string.Empty;
            }
            if (Regex.IsMatch(s, @"^\d+(\.\d+)?[eE]\+\d+$"))
                warning = "Policy number is corrupted in the broker file (" + s + ") – ask for the full number.";

            return s;
        }

        // ------------------------------------------------------------------ cell helpers

        private static int Get(Dictionary<Col, int> map, Col col)
        {
            return map.ContainsKey(col) ? map[col] : -1;
        }

        private static object Cell(object[] row, int index)
        {
            return index >= 0 && row != null && index < row.Length ? row[index] : null;
        }

        private static string Text(object cell)
        {
            if (cell == null) return string.Empty;
            if (cell is DateTime) return ((DateTime)cell).ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            if (cell is double) return ((double)cell).ToString(CultureInfo.InvariantCulture);
            return Convert.ToString(cell, CultureInfo.InvariantCulture).Replace(' ', ' ').Trim();
        }

        private static string Normalize(string text)
        {
            return Regex.Replace((text ?? string.Empty).ToLowerInvariant(), @"\s+", " ").Trim();
        }

        private static string CleanName(string text)
        {
            return Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
        }

        private static bool IsNumberText(string s)
        {
            decimal d;
            return decimal.TryParse(s.Replace(",", string.Empty).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out d);
        }

        public static decimal? Number(object cell)
        {
            if (cell == null) return null;
            if (cell is double) return Convert.ToDecimal((double)cell);
            if (cell is DateTime) return null;

            var s = Text(cell).Replace(",", string.Empty).Replace("₹", string.Empty).Trim();
            if (s.Length == 0 || s == "-") return null;

            decimal d;
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out d) ? d : (decimal?)null;
        }
    }
}
