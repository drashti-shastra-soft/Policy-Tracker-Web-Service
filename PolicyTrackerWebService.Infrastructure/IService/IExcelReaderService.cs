using System.Collections.Generic;

namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IExcelReaderService
    {
        /// <summary>
        /// Reads the first worksheet of an .xlsx file. Each cell is a string, double or DateTime (null when empty).
        /// </summary>
        List<object[]> ReadFirstSheet(byte[] fileBytes);
    }
}
