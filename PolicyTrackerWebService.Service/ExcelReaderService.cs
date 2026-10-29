using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace PolicyTrackerWebService.Service
{
    /// <summary>
    /// Minimal .xlsx reader (Open XML) – no third-party package needed.
    /// Returns cell values as string, double or DateTime.
    /// </summary>
    public class ExcelReaderService : IExcelReaderService
    {
        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace PkgRel = "http://schemas.openxmlformats.org/package/2006/relationships";

        public List<object[]> ReadFirstSheet(byte[] fileBytes)
        {
            if (fileBytes == null || fileBytes.Length == 0)
                throw new APIRequestFailedException("The file is empty!");

            try
            {
                using (var stream = new MemoryStream(fileBytes))
                using (var zip = new ZipArchive(stream, ZipArchiveMode.Read))
                {
                    var sharedStrings = ReadSharedStrings(zip);
                    var dateStyles = ReadDateStyles(zip);
                    var sheetPath = FindFirstSheetPath(zip);

                    var sheetEntry = zip.GetEntry(sheetPath);
                    if (sheetEntry == null)
                        throw new APIRequestFailedException("No worksheet found in the Excel file!");

                    return ReadSheet(sheetEntry, sharedStrings, dateStyles);
                }
            }
            catch (InvalidDataException)
            {
                throw new APIRequestFailedException("Please upload an Excel .xlsx file (old .xls files are not supported – open it in Excel and 'Save As' .xlsx).");
            }
        }

        private static XDocument Load(ZipArchive zip, string path)
        {
            var entry = zip.GetEntry(path);
            if (entry == null) return null;
            using (var s = entry.Open())
            {
                return XDocument.Load(s);
            }
        }

        private static List<string> ReadSharedStrings(ZipArchive zip)
        {
            var list = new List<string>();
            var doc = Load(zip, "xl/sharedStrings.xml");
            if (doc == null) return list;

            foreach (var si in doc.Root.Elements(Main + "si"))
            {
                var sb = new StringBuilder();
                foreach (var t in si.Descendants(Main + "t"))
                {
                    // skip phonetic runs
                    if (t.Ancestors(Main + "rPh").Any()) continue;
                    sb.Append(t.Value);
                }
                list.Add(sb.ToString());
            }
            return list;
        }

        private static HashSet<int> ReadDateStyles(ZipArchive zip)
        {
            var result = new HashSet<int>();
            var doc = Load(zip, "xl/styles.xml");
            if (doc == null) return result;

            var builtInDateFormats = new HashSet<int> { 14, 15, 16, 17, 18, 19, 20, 21, 22, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 45, 46, 47, 50, 51, 52, 53, 54, 55, 56, 57, 58 };

            var customDateFormats = new HashSet<int>();
            var numFmts = doc.Root.Element(Main + "numFmts");
            if (numFmts != null)
            {
                foreach (var nf in numFmts.Elements(Main + "numFmt"))
                {
                    int id;
                    if (!int.TryParse((string)nf.Attribute("numFmtId"), out id)) continue;
                    if (IsDateFormat((string)nf.Attribute("formatCode"))) customDateFormats.Add(id);
                }
            }

            var cellXfs = doc.Root.Element(Main + "cellXfs");
            if (cellXfs == null) return result;

            var index = 0;
            foreach (var xf in cellXfs.Elements(Main + "xf"))
            {
                int fmtId;
                if (int.TryParse((string)xf.Attribute("numFmtId"), out fmtId) &&
                    (builtInDateFormats.Contains(fmtId) || customDateFormats.Contains(fmtId)))
                {
                    result.Add(index);
                }
                index++;
            }
            return result;
        }

        private static bool IsDateFormat(string formatCode)
        {
            if (string.IsNullOrEmpty(formatCode)) return false;
            // remove quoted text, escaped chars and [colour]/[locale] sections
            var cleaned = Regex.Replace(formatCode, "\"[^\"]*\"|\\\\.|\\[[^\\]]*\\]", string.Empty);
            if (cleaned.IndexOf("General", StringComparison.OrdinalIgnoreCase) >= 0) return false;
            return Regex.IsMatch(cleaned, "[dmyhs]", RegexOptions.IgnoreCase);
        }

        private static string FindFirstSheetPath(ZipArchive zip)
        {
            var workbook = Load(zip, "xl/workbook.xml");
            var rels = Load(zip, "xl/_rels/workbook.xml.rels");

            if (workbook != null && rels != null)
            {
                var firstSheet = workbook.Root.Element(Main + "sheets")?.Elements(Main + "sheet").FirstOrDefault();
                var relId = firstSheet == null ? null : (string)firstSheet.Attribute(Rel + "id");
                var rel = rels.Root.Elements(PkgRel + "Relationship").FirstOrDefault(r => (string)r.Attribute("Id") == relId);
                if (rel != null)
                {
                    var target = ((string)rel.Attribute("Target")).Replace('\\', '/');
                    return target.StartsWith("/") ? target.TrimStart('/') : "xl/" + target;
                }
            }
            return "xl/worksheets/sheet1.xml";
        }

        private static List<object[]> ReadSheet(ZipArchiveEntry entry, List<string> sharedStrings, HashSet<int> dateStyles)
        {
            XDocument doc;
            using (var s = entry.Open())
            {
                doc = XDocument.Load(s);
            }

            var rows = new List<object[]>();
            var sheetData = doc.Root.Element(Main + "sheetData");
            if (sheetData == null) return rows;

            foreach (var row in sheetData.Elements(Main + "row"))
            {
                var cells = new Dictionary<int, object>();
                var nextIndex = 0;

                foreach (var c in row.Elements(Main + "c"))
                {
                    var reference = (string)c.Attribute("r");
                    var col = reference != null ? ColumnIndex(reference) : nextIndex;
                    nextIndex = col + 1;

                    var value = ReadCell(c, sharedStrings, dateStyles);
                    if (value != null) cells[col] = value;
                }

                var width = cells.Count == 0 ? 0 : cells.Keys.Max() + 1;
                var arr = new object[width];
                foreach (var kv in cells) arr[kv.Key] = kv.Value;
                rows.Add(arr);
            }
            return rows;
        }

        private static object ReadCell(XElement c, List<string> sharedStrings, HashSet<int> dateStyles)
        {
            var type = (string)c.Attribute("t");
            var v = c.Element(Main + "v");

            if (type == "inlineStr")
            {
                var isElement = c.Element(Main + "is");
                return isElement == null ? null : string.Concat(isElement.Descendants(Main + "t").Select(t => t.Value));
            }

            if (v == null) return null;
            var raw = v.Value;

            switch (type)
            {
                case "s":
                    int idx;
                    return int.TryParse(raw, out idx) && idx >= 0 && idx < sharedStrings.Count ? sharedStrings[idx] : null;
                case "str":
                case "e":
                    return raw;
                case "b":
                    return raw == "1" ? "TRUE" : "FALSE";
            }

            double number;
            if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                return raw;

            int style;
            if (int.TryParse((string)c.Attribute("s"), out style) && dateStyles.Contains(style) && number > 0 && number < 2958466)
                return DateTime.FromOADate(number);

            return number;
        }

        private static int ColumnIndex(string reference)
        {
            var index = 0;
            foreach (var ch in reference)
            {
                if (!char.IsLetter(ch)) break;
                index = index * 26 + (char.ToUpperInvariant(ch) - 'A' + 1);
            }
            return index - 1;
        }
    }
}
