using System;

namespace PolicyTrackerWebService.Repository.Mappers
{
    /// <summary>ToDomain / ToEntity conversions between EF entities and domain models.</summary>
    public static class EntityMapper
    {
        // ---------------- User ----------------
        public static Domain.User ToDomain(this Persistence.User e, string roleName = null)
        {
            if (e == null) return null;
            return new Domain.User
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                EmailAddress = e.EmailAddress,
                MobileNo = e.MobileNo,
                RoleId = e.RoleId,
                Role = roleName,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        // ---------------- Broker ----------------
        public static Domain.Broker ToDomain(this Persistence.Broker e)
        {
            if (e == null) return null;
            return new Domain.Broker
            {
                Id = e.Id,
                Name = e.Name,
                PosCode = e.PosCode,
                PosName = e.PosName,
                SalesManager = e.SalesManager,
                ContactNo = e.ContactNo,
                EmailAddress = e.EmailAddress,
                TdsRate = e.TdsRate,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.Broker m, Persistence.Broker e)
        {
            e.Name = m.Name.Trim();
            e.PosCode = m.PosCode;
            e.PosName = m.PosName;
            e.SalesManager = m.SalesManager;
            e.ContactNo = m.ContactNo;
            e.EmailAddress = m.EmailAddress;
            e.TdsRate = m.TdsRate;
            e.IsActive = m.IsActive;
        }

        // ---------------- InsuranceCompany ----------------
        public static Domain.InsuranceCompany ToDomain(this Persistence.InsuranceCompany e)
        {
            if (e == null) return null;
            return new Domain.InsuranceCompany
            {
                Id = e.Id,
                Name = e.Name,
                ShortName = e.ShortName,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.InsuranceCompany m, Persistence.InsuranceCompany e)
        {
            e.Name = m.Name.Trim();
            e.ShortName = m.ShortName;
            e.IsActive = m.IsActive;
        }

        // ---------------- Category ----------------
        public static Domain.Category ToDomain(this Persistence.Category e)
        {
            if (e == null) return null;
            return new Domain.Category
            {
                Id = e.Id,
                Name = e.Name,
                DefaultPoints = e.DefaultPoints,
                IsMotor = e.IsMotor,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.Category m, Persistence.Category e)
        {
            e.Name = m.Name.Trim();
            e.DefaultPoints = m.DefaultPoints;
            e.IsMotor = m.IsMotor;
            e.IsActive = m.IsActive;
        }

        // ---------------- PointsGrid ----------------
        public static Domain.PointsGrid ToDomain(this Persistence.PointsGrid e, string broker = null, string category = null)
        {
            if (e == null) return null;
            return new Domain.PointsGrid
            {
                Id = e.Id,
                BrokerId = e.BrokerId,
                Broker = broker,
                CategoryId = e.CategoryId,
                Category = category,
                AgeFrom = e.AgeFrom,
                AgeTo = e.AgeTo,
                Points = e.Points,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.PointsGrid m, Persistence.PointsGrid e)
        {
            e.BrokerId = m.BrokerId;
            e.CategoryId = m.CategoryId;
            e.AgeFrom = m.AgeFrom;
            e.AgeTo = m.AgeTo;
            e.Points = m.Points;
            e.IsActive = m.IsActive;
        }

        // ---------------- Policy ----------------
        public static Domain.Policy ToDomain(this Persistence.Policy e)
        {
            if (e == null) return null;
            return new Domain.Policy
            {
                Id = e.Id,
                BrokerId = e.BrokerId,
                BusinessType = e.BusinessType,
                CategoryId = e.CategoryId,
                InsuranceCompanyId = e.InsuranceCompanyId,
                ProductPlan = e.ProductPlan,
                CustomerName = e.CustomerName,
                MobileNo = e.MobileNo,
                EmailAddress = e.EmailAddress,
                EldestAge = e.EldestAge,
                SumInsured = e.SumInsured,
                PolicyNo = e.PolicyNo,
                PreviousPolicyNo = e.PreviousPolicyNo,
                RenewalRefNo = e.RenewalRefNo,
                VehicleNo = e.VehicleNo,
                StartDate = e.StartDate,
                ExpiryDate = e.ExpiryDate,
                OdPremium = e.OdPremium,
                TpPremium = e.TpPremium,
                NetPremium = e.NetPremium,
                GstAmount = e.GstAmount,
                IndicativePremium = e.IndicativePremium,
                PointsOd = e.PointsOd,
                PointsTp = e.PointsTp,
                PointsNet = e.PointsNet,
                RenewalStatus = e.RenewalStatus,
                LostReason = e.LostReason,
                QuoteSentOn = e.QuoteSentOn,
                QuoteDetails = e.QuoteDetails,
                RenewedOn = e.RenewedOn,
                RenewedPolicyId = e.RenewedPolicyId,
                RenewedFromPolicyId = e.RenewedFromPolicyId,
                PaymentMode = e.PaymentMode,
                ChequeNo = e.ChequeNo,
                BankName = e.BankName,
                UtrNo = e.UtrNo,
                AmountPaid = e.AmountPaid,
                PaymentDate = e.PaymentDate,
                SubmittedToBrokerOn = e.SubmittedToBrokerOn,
                BrokerStaff = e.BrokerStaff,
                ProofRef = e.ProofRef,
                PolicyCopyReceived = e.PolicyCopyReceived,
                Remarks = e.Remarks,
                IsActive = e.IsActive,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.Policy m, Persistence.Policy e)
        {
            e.BrokerId = m.BrokerId;
            e.BusinessType = m.BusinessType;
            e.CategoryId = m.CategoryId;
            e.InsuranceCompanyId = m.InsuranceCompanyId;
            e.ProductPlan = m.ProductPlan;
            e.CustomerName = (m.CustomerName ?? string.Empty).Trim();
            e.MobileNo = m.MobileNo;
            e.EmailAddress = m.EmailAddress;
            e.EldestAge = m.EldestAge;
            e.SumInsured = m.SumInsured;
            e.PolicyNo = m.PolicyNo == null ? null : m.PolicyNo.Trim();
            e.PreviousPolicyNo = m.PreviousPolicyNo;
            e.RenewalRefNo = m.RenewalRefNo;
            e.VehicleNo = m.VehicleNo;
            e.StartDate = m.StartDate;
            e.ExpiryDate = m.ExpiryDate;
            e.OdPremium = m.OdPremium;
            e.TpPremium = m.TpPremium;
            e.NetPremium = m.NetPremium;
            e.GstAmount = m.GstAmount;
            e.IndicativePremium = m.IndicativePremium;
            e.PointsOd = m.PointsOd;
            e.PointsTp = m.PointsTp;
            e.PointsNet = m.PointsNet;
            e.RenewalStatus = m.RenewalStatus;
            e.LostReason = m.LostReason;
            e.QuoteSentOn = m.QuoteSentOn;
            e.QuoteDetails = m.QuoteDetails;
            e.RenewedOn = m.RenewedOn;
            e.RenewedPolicyId = m.RenewedPolicyId;
            e.RenewedFromPolicyId = m.RenewedFromPolicyId;
            e.PaymentMode = m.PaymentMode;
            e.ChequeNo = m.ChequeNo;
            e.BankName = m.BankName;
            e.UtrNo = m.UtrNo;
            e.AmountPaid = m.AmountPaid;
            e.PaymentDate = m.PaymentDate;
            e.SubmittedToBrokerOn = m.SubmittedToBrokerOn;
            e.BrokerStaff = m.BrokerStaff;
            e.ProofRef = m.ProofRef;
            e.PolicyCopyReceived = m.PolicyCopyReceived;
            e.Remarks = m.Remarks;
            e.IsActive = m.IsActive;
        }

        // ---------------- StatementLine ----------------
        public static Domain.StatementLine ToDomain(this Persistence.StatementLine e)
        {
            if (e == null) return null;
            return new Domain.StatementLine
            {
                Id = e.Id,
                BrokerId = e.BrokerId,
                StatementMonth = e.StatementMonth,
                StartDate = e.StartDate,
                ExpiryDate = e.ExpiryDate,
                SalesManager = e.SalesManager,
                PosName = e.PosName,
                PosCode = e.PosCode,
                CustomerName = e.CustomerName,
                InsuranceCompany = e.InsuranceCompany,
                Product = e.Product,
                PolicyNo = e.PolicyNo,
                OdPremium = e.OdPremium,
                TpPremium = e.TpPremium,
                NetPremium = e.NetPremium,
                GstAmount = e.GstAmount,
                PointsOd = e.PointsOd,
                PointsTp = e.PointsTp,
                PointsNet = e.PointsNet,
                Value = e.Value,
                Remarks = e.Remarks,
                PolicyId = e.PolicyId,
                MatchStatus = e.MatchStatus,
                ImportBatch = e.ImportBatch,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.StatementLine m, Persistence.StatementLine e)
        {
            e.BrokerId = m.BrokerId;
            e.StatementMonth = new DateTime(m.StatementMonth.Year, m.StatementMonth.Month, 1);
            e.StartDate = m.StartDate;
            e.ExpiryDate = m.ExpiryDate;
            e.SalesManager = m.SalesManager;
            e.PosName = m.PosName;
            e.PosCode = m.PosCode;
            e.CustomerName = m.CustomerName;
            e.InsuranceCompany = m.InsuranceCompany;
            e.Product = m.Product;
            e.PolicyNo = m.PolicyNo;
            e.OdPremium = m.OdPremium;
            e.TpPremium = m.TpPremium;
            e.NetPremium = m.NetPremium;
            e.GstAmount = m.GstAmount;
            e.PointsOd = m.PointsOd;
            e.PointsTp = m.PointsTp;
            e.PointsNet = m.PointsNet;
            e.Value = m.Value;
            e.Remarks = m.Remarks;
            e.PolicyId = m.PolicyId;
            e.MatchStatus = m.MatchStatus;
            e.ImportBatch = m.ImportBatch;
        }

        // ---------------- Payout ----------------
        public static Domain.Payout ToDomain(this Persistence.Payout e)
        {
            if (e == null) return null;
            return new Domain.Payout
            {
                Id = e.Id,
                BrokerId = e.BrokerId,
                StatementMonth = e.StatementMonth,
                ReceivedOn = e.ReceivedOn,
                AmountReceived = e.AmountReceived,
                PaidByFirm = e.PaidByFirm,
                TdsRate = e.TdsRate,
                Remarks = e.Remarks,
                CreatedBy = e.CreatedBy,
                LastModifiedBy = e.LastModifiedBy,
            };
        }

        public static void ToEntity(this Domain.Payout m, Persistence.Payout e)
        {
            e.BrokerId = m.BrokerId;
            e.StatementMonth = m.StatementMonth.HasValue ? new DateTime(m.StatementMonth.Value.Year, m.StatementMonth.Value.Month, 1) : (DateTime?)null;
            e.ReceivedOn = m.ReceivedOn;
            e.AmountReceived = m.AmountReceived;
            e.PaidByFirm = m.PaidByFirm;
            e.TdsRate = m.TdsRate;
            e.Remarks = m.Remarks;
        }
    }
}
