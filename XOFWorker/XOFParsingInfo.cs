using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFParsingInfo
    {
        /// <summary>
        /// File path of the file parsing
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// Reading line number
        /// </summary>
        public int ReadingLine { get; set; }

        /// <summary>
        /// The content of reading line
        /// </summary>
        public string ReadingLineContent { get; set; } = "";

        /// <summary>
        /// Count of game parsed success
        /// </summary>
        public int GameParsedSuccess { get; set; }

        /// <summary>
        /// Count of game parsed failed
        /// </summary>
        public int GameParsedFailed { get; set; }

        /// <summary>
        /// Error message or info message to caller
        /// </summary>
        public string Message { get; set; } = "";
    }
}
