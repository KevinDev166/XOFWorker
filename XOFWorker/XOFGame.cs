using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFGame
    {

        public XOFGame()
        {
            this.Headers = new List<XOFHeader>();
            this.Keys = new XOFPiecesKey();
            this.Moves = new List<XOFMove>();
        }

        public static Encoding UTF8_NoBOM
        {
            get
            {
                /*
                 * XOFGame uses exactly one encoding:
                 * UTF-8 without BOM
                 * Easy to use, widely compatible and safe for SQL Server
                 */

                return new UTF8Encoding(false);
            }
        }

        public List<XOFHeader> Headers { get; set; }
        public XOFPiecesKey Keys { get; set; }
        public List<XOFMove> Moves { get; set; }

        public string ExportAllHeadersToString()
        {
            StringBuilder sb = new StringBuilder();

            //header section
            sb.AppendLine(this.RequireHeader(XOFKeywords.Event).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.Address).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.Date).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.Round).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.GameNumber).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.RedPlayer).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.BluePlayer).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.RedTeam).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.BlueTeam).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.StartingFEN).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.Result).GetHeaderLine());

            sb.AppendLine(this.RequireHeader(XOFKeywords.PiecesKey).GetHeaderLine());

            foreach (XOFHeader h in this.GetCustomHeaders())
            {
                sb.AppendLine(h.GetHeaderLine());
            }

            return sb.ToString();
        }
        public string ExportCustomHeadersToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (XOFHeader h in this.GetCustomHeaders())
            {
                sb.AppendLine(h.GetHeaderLine());
            }

            return sb.ToString();
        }
        public string ExportMovesToString()
        {
            StringBuilder sb = new StringBuilder();

            //move text section
            foreach (XOFMove m in this.Moves)
            {
                sb.AppendLine(m.ExportToTextLine());
            }

            return sb.ToString();
        }
        public string ExportEntireGameToString()
        {
            StringBuilder sb = new StringBuilder();

            string header = this.ExportAllHeadersToString();
            sb.AppendLine(header.Trim()); //must trim to remove the last empty line

            //separator
            sb.AppendLine(XOFKeywords.SectionSeparator);

            //move text section
            foreach (XOFMove m in this.Moves)
            {
                sb.AppendLine(m.ExportToTextLine());
            }

            //separator
            sb.AppendLine(XOFKeywords.GameSeparator); //important

            return sb.ToString();
        }


        /// <summary>
        /// Gets a REQUIRED header. If it is missing, throws an error with a CLEAR message instead of
        /// NullReferenceException "Object reference not set to an instance of an object"
        /// — previously a missing header threw exactly that confusing error, without saying which header was missing.
        /// </summary>
        private XOFHeader RequireHeader(string headerName)
            => this.GetHeader(headerName)
               ?? throw new InvalidOperationException(
                      "The game is missing the required header \"" + headerName + "\" so it cannot be written to an XOF file.");

        public XOFHeader? GetHeader(string headerName)
        {
            foreach (XOFHeader h in this.Headers)
            {
                if (h.Name == headerName)
                    return h;
            }
            return null;
        }
        public List<XOFHeader> GetCustomHeaders()
        {
            List<XOFHeader> list = new List<XOFHeader>();

            List<string> reqHeaders = XOFKeywords.GetRequiredHeaderNames();

            foreach (XOFHeader h in this.Headers)
            {
                if (reqHeaders.Contains(h.Name) == false)
                {
                    list.Add(h);
                }
            }

            return list;
        }

        public static XOFMoveSigns GetMoveSign()
        {
            XOFMoveSigns ms = new XOFMoveSigns
                (
                XOFKeywords.MovePlus[0], 
                XOFKeywords.MoveMinus[0], 
                XOFKeywords.MoveEqual[0]
                );

            return ms;
        }

    }
}
