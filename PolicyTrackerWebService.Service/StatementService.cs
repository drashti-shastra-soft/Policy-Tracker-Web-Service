using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PolicyTrackerWebService.Service
{
    public class StatementService : IStatementService
    {
        #region Declaration
        private readonly IStatementRepository _repo;
        private readonly IPolicyRepository _policyRepo;
        private readonly IBrokerRepository _brokerRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly IInsuranceCompanyRepository _companyRepo;
        private readonly IExcelReaderService _excelReader;

        public StatementService(IStatementRepository repo, IPolicyRepository policyRepo, IBrokerRepository brokerRepo,
                                ICategoryRepository categoryRepo, IInsuranceCompanyRepository companyRepo, IExcelReaderService excelReader)
        {
            _repo = repo;
            _policyRepo = policyRepo;
            _brokerRepo = brokerRepo;
            _categoryRepo = categoryRepo;
            _companyRepo = companyRepo;
            _excelReader = excelReader;
        }
        #endregion

        #region Public Methods
        public Domain.StatementImportResult Import(Domain.StatementImport mImport)
        {
            if (mImport == null) throw new APIRequestFailedException("Object must have a value!");
            if (mImport.BrokerId <= 0) throw new APIRequestFailedException("Please select the broker!");
            if (mImport.FileBase64.IsNullOrEmpty()) throw new APIRequestFailedException("Please select the broker's Excel file!");

            var broker = _brokerRepo.Get(mImport.BrokerId);
            if (broker == null) throw new APIRequestFailedException("Broker not found!");

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(mImport.FileBase64);
            }
            catch (FormatException)
            {
                throw new APIRequestFailedException("The uploaded file could not be read!");
            }

            var result = new Domain.StatementImportResult();
            var parsed = StatementParser.Parse(_excelReader.ReadFirstSheet(bytes), mImport.StatementMonth, result.Warnings);
            result.TotalRows = parsed.Count;
            if (parsed.Count == 0)
                throw new APIRequestFailedException("No statement rows were found in the file. " + string.Join(" ", result.Warnings));

            var batch = string.Format("{0} ({1:dd-MMM-yyyy HH:mm})", mImport.FileName ?? "upload", DateTime.Now);
            var toSave = new List<Domain.StatementLine>();

            foreach (var line in parsed)
            {
                line.BrokerId = mImport.BrokerId;
                line.CreatedBy = mImport.CreatedBy;
                line.LastModifiedBy = mImport.CreatedBy;
                line.ImportBatch = batch;
                line.MatchStatus = Enums.MatchStatus.Unmatched.ToText();

                var existing = _repo.Find(line.BrokerId, line.PolicyNo, line.StatementMonth, line.CustomerName);
                if (existing != null)
                {
                    if (!mImport.ReplaceExisting)
                    {
                        result.Skipped++;
                        continue;
                    }
                    line.Id = existing.Id;
                    line.PolicyId = existing.PolicyId;
                    line.MatchStatus = existing.MatchStatus;
                }

                if (broker.PosCode.IsNotNullOrEmpty() && line.PosCode.IsNotNullOrEmpty() && line.PosCode.Trim() != broker.PosCode.Trim())
                    result.Warnings.Add(string.Format("{0} ({1}) is under POS code {2}, not your code {3}.", line.CustomerName, line.PolicyNo, line.PosCode, broker.PosCode));

                var calc = CommissionCalculator.LineValue(line);
                if (calc.HasValue && Math.Abs(calc.Value - line.Value) > 1)
                    result.Warnings.Add(string.Format("{0} ({1}): broker's value ₹{2:N0} ≠ premium × points ₹{3:N0}.", line.CustomerName, line.PolicyNo, line.Value, calc.Value));

                toSave.Add(line);
            }

            // ---- auto match ----
            var allLines = _repo.GetByBroker(mImport.BrokerId);
            var taken = new HashSet<int>(allLines.Where(l => l.PolicyId.HasValue && !toSave.Any(s => s.Id == l.Id)).Select(l => l.PolicyId.Value));
            var policies = _policyRepo.GetForMatching(mImport.BrokerId);

            foreach (var m in PolicyMatcher.Match(toSave, policies, taken))
            {
                toSave[m.LineIndex].PolicyId = m.PolicyId;
                toSave[m.LineIndex].MatchStatus = (m.ByPolicyNo ? Enums.MatchStatus.Matched_Policy_No : Enums.MatchStatus.Matched_Name_Date).ToText();
            }

            foreach (var line in toSave)
            {
                _repo.Upsert(line);
                result.Imported++;
                result.TotalValue += line.Value;

                if (line.MatchStatus.Is(Enums.MatchStatus.Matched_Policy_No)) result.MatchedByPolicyNo++;
                else if (line.MatchStatus.Is(Enums.MatchStatus.Matched_Name_Date)) result.MatchedByName++;
                else if (line.MatchStatus.Is(Enums.MatchStatus.Unmatched)) result.Unmatched++;
            }

            result.Months = toSave.Select(x => x.StatementMonth).Distinct().OrderBy(x => x)
                                  .Select(x => x.ToString("MMM-yyyy", CultureInfo.InvariantCulture)).ToList();
            return result;
        }

        public Domain.StatementLineLister GetAll(Domain.StatementLineLister mLister)
        {
            var result = _repo.GetAll(mLister);
            foreach (var line in result.List)
            {
                line.CalculatedValue = CommissionCalculator.LineValue(line);
                line.ValueDifference = line.CalculatedValue.HasValue ? line.Value - line.CalculatedValue.Value : (decimal?)null;
            }
            return result;
        }

        public Domain.StatementLine Get(int id)
        {
            return _repo.Get(id);
        }

        public Domain.StatementLine Link(Domain.StatementLink mLink)
        {
            if (mLink == null) throw new APIRequestFailedException("Object must have a value!");

            var line = _repo.Get(mLink.StatementLineId);
            if (line == null) throw new APIRequestFailedException("Statement line not found!");

            int? policyId = mLink.PolicyId;
            if (!policyId.HasValue && mLink.PolicyNo.IsNotNullOrEmpty())
            {
                var key = mLink.PolicyNo.NormalizePolicyNo();
                var policy = _policyRepo.GetForMatching(line.BrokerId).FirstOrDefault(p => p.PolicyNo.NormalizePolicyNo() == key);
                if (policy == null) throw new APIRequestFailedException("No policy found with policy number " + mLink.PolicyNo + "!");
                policyId = policy.Id;
            }

            if (policyId.HasValue)
            {
                var other = _repo.GetByBroker(line.BrokerId).FirstOrDefault(l => l.PolicyId == policyId && l.Id != line.Id);
                if (other != null)
                    throw new APIRequestFailedException(string.Format("That policy is already linked to {0} ({1:MMM-yyyy}). Unlink it first.", other.CustomerName, other.StatementMonth));
            }

            var status = policyId.HasValue ? Enums.MatchStatus.Manual.ToText() : Enums.MatchStatus.Unmatched.ToText();
            _repo.Link(line.Id, policyId, status, mLink.LastModifiedBy);

            line.PolicyId = policyId;
            line.MatchStatus = status;
            return line;
        }

        public Domain.Policy CreatePolicyFromLine(int lineId, int createdBy)
        {
            var line = _repo.Get(lineId);
            if (line == null) throw new APIRequestFailedException("Statement line not found!");
            if (line.PolicyId.HasValue) throw new APIRequestFailedException("This line is already linked to a policy!");

            var policy = new Domain.Policy
            {
                BrokerId = line.BrokerId,
                CategoryId = GuessCategory(line.Product),
                InsuranceCompanyId = GuessCompany(line.InsuranceCompany),
                ProductPlan = line.Product,
                CustomerName = line.CustomerName,
                PolicyNo = line.PolicyNo.IsNotNullOrEmpty() && !line.PolicyNo.Contains("E+") ? line.PolicyNo : null,
                StartDate = line.StartDate,
                ExpiryDate = line.ExpiryDate,
                OdPremium = line.OdPremium,
                TpPremium = line.TpPremium,
                NetPremium = line.NetPremium,
                GstAmount = line.GstAmount,
                RenewalStatus = Enums.RenewalStatus.Due.ToText(),
                Remarks = "Created from broker statement " + line.StatementMonth.ToString("MMM-yyyy", CultureInfo.InvariantCulture) + " – fill business type, payment and proof.",
                IsActive = true,
                CreatedBy = createdBy,
                LastModifiedBy = createdBy,
            };

            policy = _policyRepo.Upsert(policy);
            _repo.Link(line.Id, policy.Id, Enums.MatchStatus.Manual.ToText(), createdBy);
            return policy;
        }

        public int Rematch(int brokerId, int modifiedBy)
        {
            var lines = _repo.GetByBroker(brokerId);
            var open = lines.Where(l => !l.PolicyId.HasValue).ToList();
            if (open.Count == 0) return 0;

            var taken = new HashSet<int>(lines.Where(l => l.PolicyId.HasValue).Select(l => l.PolicyId.Value));
            var matches = PolicyMatcher.Match(open, _policyRepo.GetForMatching(brokerId), taken);

            foreach (var m in matches)
            {
                var status = m.ByPolicyNo ? Enums.MatchStatus.Matched_Policy_No : Enums.MatchStatus.Matched_Name_Date;
                _repo.Link(open[m.LineIndex].Id, m.PolicyId, status.ToText(), modifiedBy);
            }
            return matches.Count;
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public int DeleteMonth(int brokerId, DateTime statementMonth)
        {
            return _repo.DeleteMonth(brokerId, statementMonth);
        }

        public List<DateTime> GetMonths(int? brokerId)
        {
            return _repo.GetMonths(brokerId);
        }
        #endregion

        #region Private Methods
        private int? GuessCategory(string product)
        {
            var p = (product ?? string.Empty).ToUpperInvariant();
            var categories = _categoryRepo.GetActive();

            Func<string, int?> find = word =>
            {
                var c = categories.FirstOrDefault(x => x.Name.ToUpperInvariant().Contains(word));
                return c == null ? (int?)null : c.Id;
            };

            if (p.Contains("TOP UP") || p.Contains("TOP-UP")) return find("TOP") ?? find("HEALTH");
            if (p.Contains("FLOATER")) return find("FLOATER") ?? find("HEALTH");
            if (p.Contains("MEDICLAIM") || p.Contains("HEALTH")) return find("INDIVIDUAL") ?? find("HEALTH");
            if (p.Contains("PERSONAL") || p.StartsWith("PA ") || p.Contains("ACCIDENT")) return find("ACCIDENT");
            if (p.Contains("OVERSEAS") || p.Contains("TRAVEL")) return find("TRAVEL");
            if (p.Contains("GRIHA") || p.Contains("HOME")) return find("HOME");
            if (p.Contains("2W") || p.Contains("TWO WHEELER")) return find("TWO WHEELER");
            if (p.Contains("GCV") || p.Contains("PCV") || p.Contains("COMMERCIAL")) return find("COMMERCIAL");
            if (p.Contains("CAR")) return find("PRIVATE CAR");
            if (p.Contains("LIABILITY")) return find("LIABILITY");
            if (p.Contains("SHOP") || p.Contains("SOOKSHMA") || p.Contains("SME")) return find("SHOP");
            return find("OTHER");
        }

        private int? GuessCompany(string name)
        {
            if (name.IsNullOrEmpty()) return null;
            var key = Simplify(name);
            var company = _companyRepo.GetActive().FirstOrDefault(c => Simplify(c.Name) == key)
                       ?? _companyRepo.GetActive().FirstOrDefault(c => Simplify(c.Name).StartsWith(key) || key.StartsWith(Simplify(c.Name)));
            return company == null ? (int?)null : company.Id;
        }

        private static string Simplify(string name)
        {
            var s = (name ?? string.Empty).ToUpperInvariant();
            foreach (var w in new[] { "THE ", " CO.", " CO ", " COMPANY", " LTD", " LIMITED", ".", "INSURANCE", "GENERAL", "ASSURANCE" })
                s = s.Replace(w, " ");
            return string.Join(" ", s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }
        #endregion
    }
}
