using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.Utils;
using PolicyTrackerWebService.Repository.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PolicyTrackerWebService.Repository
{
    public class StatementRepository : IStatementRepository
    {
        public Domain.StatementLineLister GetAll(Domain.StatementLineLister mLister)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = from s in context.StatementLines
                            join b in context.Brokers on s.BrokerId equals b.Id
                            join p in context.Policies on s.PolicyId equals (int?)p.Id into pj
                            from p in pj.DefaultIfEmpty()
                            select new { Line = s, Broker = b.Name, PosCode = b.PosCode, LinkedCustomer = p.CustomerName };

                var criteria = mLister.SearchCriteria;
                if (criteria != null)
                {
                    if (criteria.BrokerId > 0)
                        query = query.Where(x => x.Line.BrokerId == criteria.BrokerId);

                    if (criteria.StatementMonth > DateTime.MinValue)
                    {
                        var month = criteria.StatementMonth.FirstOfMonth();
                        query = query.Where(x => x.Line.StatementMonth == month);
                    }

                    if (criteria.MatchStatus.IsNotNullOrEmpty())
                        query = query.Where(x => x.Line.MatchStatus == criteria.MatchStatus);

                    if (criteria.SearchText.IsNotNullOrEmpty())
                    {
                        var text = criteria.SearchText.Trim();
                        query = query.Where(x => x.Line.CustomerName.Contains(text) || x.Line.PolicyNo.Contains(text) || x.Line.Product.Contains(text));
                    }
                }

                var rows = query.OrderByDescending(x => x.Line.StatementMonth).ThenBy(x => x.Line.StartDate).ToList();

                var list = rows.Select(x =>
                {
                    var m = x.Line.ToDomain();
                    m.Broker = x.Broker;
                    m.LinkedCustomer = x.LinkedCustomer;
                    m.IsOtherPosCode = x.PosCode.IsNotNullOrEmpty() && m.PosCode.IsNotNullOrEmpty() && m.PosCode.Trim() != x.PosCode.Trim();
                    return m;
                }).ToList();

                mLister.TotalValue = list.Sum(x => x.Value);
                mLister.Pagination = mLister.Pagination ?? new Domain.Pagination();
                mLister.List = PagingHelper.Apply(list, mLister.Pagination);
            }
            return mLister;
        }

        public Domain.StatementLine Get(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                return context.StatementLines.FirstOrDefault(x => x.Id == id).ToDomain();
            }
        }

        public List<Domain.StatementLine> GetByBroker(int? brokerId)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = from s in context.StatementLines
                            join b in context.Brokers on s.BrokerId equals b.Id
                            select new { Line = s, PosCode = b.PosCode };

                if (brokerId.HasValue && brokerId > 0)
                    query = query.Where(x => x.Line.BrokerId == brokerId);

                return query.ToList().Select(x =>
                {
                    var m = x.Line.ToDomain();
                    m.IsOtherPosCode = x.PosCode.IsNotNullOrEmpty() && m.PosCode.IsNotNullOrEmpty() && m.PosCode.Trim() != x.PosCode.Trim();
                    return m;
                }).ToList();
            }
        }

        public Domain.StatementLine Find(int brokerId, string policyNo, DateTime statementMonth, string customerName)
        {
            var month = statementMonth.FirstOfMonth();
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = context.StatementLines.Where(x => x.BrokerId == brokerId && x.StatementMonth == month);

                query = policyNo.IsNotNullOrEmpty()
                    ? query.Where(x => x.PolicyNo == policyNo)
                    : query.Where(x => x.CustomerName == customerName);

                return query.FirstOrDefault().ToDomain();
            }
        }

        public Domain.StatementLine Upsert(Domain.StatementLine mLine)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efLine = context.StatementLines.FirstOrDefault(x => x.Id == mLine.Id);
                if (efLine == null)
                {
                    efLine = new Persistence.StatementLine { CreatedBy = mLine.CreatedBy, CreatedDate = DateTime.Now };
                    context.StatementLines.Add(efLine);
                }
                else
                {
                    efLine.LastModifiedBy = mLine.LastModifiedBy;
                    efLine.LastModifiedDate = DateTime.Now;
                }

                mLine.ToEntity(efLine);
                context.SaveChanges();
                mLine.Id = efLine.Id;
            }
            return mLine;
        }

        public void Link(int lineId, int? policyId, string matchStatus, int modifiedBy)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efLine = context.StatementLines.FirstOrDefault(x => x.Id == lineId);
                if (efLine == null) return;

                efLine.PolicyId = policyId;
                efLine.MatchStatus = matchStatus;
                efLine.LastModifiedBy = modifiedBy;
                efLine.LastModifiedDate = DateTime.Now;
                context.SaveChanges();
            }
        }

        public void Delete(int id)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var efLine = context.StatementLines.FirstOrDefault(x => x.Id == id);
                if (efLine == null) return;

                context.StatementLines.Remove(efLine);
                context.SaveChanges();
            }
        }

        public int DeleteMonth(int brokerId, DateTime statementMonth)
        {
            var month = statementMonth.FirstOfMonth();
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var lines = context.StatementLines.Where(x => x.BrokerId == brokerId && x.StatementMonth == month).ToList();
                context.StatementLines.RemoveRange(lines);
                context.SaveChanges();
                return lines.Count;
            }
        }

        public List<DateTime> GetMonths(int? brokerId)
        {
            using (var context = new Persistence.PolicyTrackerDBEntities())
            {
                var query = context.StatementLines.AsQueryable();
                if (brokerId.HasValue && brokerId > 0)
                    query = query.Where(x => x.BrokerId == brokerId);

                return query.Select(x => x.StatementMonth).Distinct().OrderBy(x => x).ToList();
            }
        }
    }
}
