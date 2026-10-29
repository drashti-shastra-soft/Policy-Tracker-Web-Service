using PolicyTrackerWebService.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PolicyTrackerWebService.Service.Helpers
{
    /// <summary>
    /// Links broker statement lines to our policies:
    ///  1. exact policy number (ignoring spaces / case),
    ///  2. customer name similarity + start date within 45 days (+ closest premium).
    /// </summary>
    public static class PolicyMatcher
    {
        private const int MaxDaysApart = 45;
        private const double MinNameScore = 1.5;

        private static readonly HashSet<string> CommonWords = new HashSet<string> { "PATEL", "BHAI", "KUMAR", "KUMARI", "BEN", "MRS", "SHRI" };

        public class MatchResult
        {
            public int LineIndex { get; set; }
            public int PolicyId { get; set; }
            public bool ByPolicyNo { get; set; }
        }

        /// <param name="lines">Lines to match (only those without PolicyId are considered).</param>
        /// <param name="policies">Candidate policies of the same broker.</param>
        /// <param name="takenPolicyIds">Policies already linked to other statement lines.</param>
        public static List<MatchResult> Match(IList<Domain.StatementLine> lines, IList<Domain.Policy> policies, ISet<int> takenPolicyIds)
        {
            var results = new List<MatchResult>();
            var taken = new HashSet<int>(takenPolicyIds ?? new HashSet<int>());
            var matchedLines = new HashSet<int>();

            // ---- pass 1: policy number ----
            var byNo = policies.Where(p => p.PolicyNo.IsNotNullOrEmpty())
                               .GroupBy(p => p.PolicyNo.NormalizePolicyNo())
                               .ToDictionary(g => g.Key, g => g.ToList());

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.PolicyId.HasValue) continue;

                var key = line.PolicyNo.NormalizePolicyNo();
                if (key.Length < 4) continue;

                List<Domain.Policy> found;
                if (!byNo.TryGetValue(key, out found)) continue;

                var policy = found.FirstOrDefault(p => !taken.Contains(p.Id));
                if (policy == null) continue;

                taken.Add(policy.Id);
                matchedLines.Add(i);
                results.Add(new MatchResult { LineIndex = i, PolicyId = policy.Id, ByPolicyNo = true });
            }

            // ---- pass 2: name + date ----
            var candidates = new List<Tuple<double, bool, double, int, int, int>>();
            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                if (line.PolicyId.HasValue || matchedLines.Contains(i) || !line.StartDate.HasValue) continue;

                foreach (var p in policies)
                {
                    if (taken.Contains(p.Id)) continue;

                    var policyStart = p.StartDate ?? (p.ExpiryDate.HasValue ? p.ExpiryDate.Value.AddDays(1) : (DateTime?)null);
                    if (!policyStart.HasValue) continue;

                    var days = Math.Abs((line.StartDate.Value - policyStart.Value).TotalDays);
                    if (days > MaxDaysApart) continue;

                    var score = NameScore(p.CustomerName, line.CustomerName);
                    if (score < MinNameScore) continue;

                    var premium = p.NetPremium ?? p.IndicativePremium ?? 0m;
                    var premiumDiff = premium > 0 && line.NetPremium.HasValue
                        ? (double)(Math.Abs(line.NetPremium.Value - premium) / premium)
                        : 1d;

                    candidates.Add(Tuple.Create(-score, days > 5, premiumDiff, (int)days, i, p.Id));
                }
            }

            foreach (var c in candidates.OrderBy(c => c.Item1).ThenBy(c => c.Item2).ThenBy(c => c.Item3).ThenBy(c => c.Item4))
            {
                if (matchedLines.Contains(c.Item5) || taken.Contains(c.Item6)) continue;

                matchedLines.Add(c.Item5);
                taken.Add(c.Item6);
                results.Add(new MatchResult { LineIndex = c.Item5, PolicyId = c.Item6, ByPolicyNo = false });
            }

            return results;
        }

        public static double NameScore(string ourName, string brokerName)
        {
            var a = Tokens(ourName);
            var b = Tokens(brokerName);
            double score = 0;

            foreach (var token in a)
            {
                if (b.Any(x => Close(token, x)))
                    score += CommonWords.Contains(token) ? 0.5 : 1;
            }
            return score;
        }

        private static HashSet<string> Tokens(string name)
        {
            var cleaned = Regex.Replace((name ?? string.Empty).ToUpperInvariant(), "[^A-Z ]", " ");
            return new HashSet<string>(cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Where(t => t.Length >= 3));
        }

        private static bool Close(string a, string b)
        {
            a = Strip(a);
            b = Strip(b);
            if (a == b) return a.Length > 0;
            if (a.Length < 4 || b.Length < 4) return false;
            return a.StartsWith(b) || b.StartsWith(a) || a.Substring(0, Math.Min(5, a.Length)) == b.Substring(0, Math.Min(5, b.Length));
        }

        private static string Strip(string token)
        {
            return token.Replace("KUMAR", string.Empty).Replace("BHAI", string.Empty).Replace("BEN", string.Empty);
        }
    }
}
