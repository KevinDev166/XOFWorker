using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFGameEventArgs : EventArgs
    {
        /// <summary>
        /// Parsed success game, only available on success parsing event
        /// </summary>
        public XOFGame? Game { get; }

        /// <summary>
        /// Parsing info container
        /// </summary>
        public XOFParsingInfo Info { get; }

        public XOFGameEventArgs(XOFGame? igame, XOFParsingInfo iInfo)
        {
            this.Game = igame;
            this.Info = iInfo;
        }
        
    }
}
