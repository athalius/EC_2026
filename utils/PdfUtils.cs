using System;
using System.Text;
using UglyToad.PdfPig;

namespace Hackathon.Utils
{
    public static class PdfUtils
    {
        // Extract plain text from a PDF file
        public static string ExtractPDFText(string filePath)
        {
            using (var document = PdfDocument.Open(filePath))
            {
                var sb = new StringBuilder();
                foreach (var page in document.GetPages())
                {
                    sb.AppendLine(page.Text);
                }
                return sb.ToString();
            }
        }
    }
}
