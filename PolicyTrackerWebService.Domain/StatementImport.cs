using System;
using System.Collections.Generic;

namespace PolicyTrackerWebService.Domain
{
    public class StatementImport
    {
        public int BrokerId { get; set; }
        /// <summary>Leave empty to take each row's month from its policy start date.</summary>
        public DateTime? StatementMonth { get; set; }
        public string FileName { get; set; }
        public string FileBase64 { get; set; }
        /// <summary>Replace rows already imported for the same broker + policy no + month.</summary>
        public bool ReplaceExisting { get; set; }
        public int CreatedBy { get; set; }
    }

    public class StatementImportResult
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Skipped { get; set; }
        public int MatchedByPolicyNo { get; set; }
        public int MatchedByName { get; set; }
        public int Unmatched { get; set; }
        public decimal TotalValue { get; set; }
        public List<string> Months { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class StatementLink
    {
        public int StatementLineId { get; set; }
        public int? PolicyId { get; set; }
        public string PolicyNo { get; set; }
        public int LastModifiedBy { get; set; }
    }
}
