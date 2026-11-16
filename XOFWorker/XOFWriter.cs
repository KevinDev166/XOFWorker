using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XOFWorker
{
    public class XOFWriter
    {
        public static void WriteGame(XOFGame game, string filePath)
        {
            // The library shows no UI. Errors are thrown for the caller to handle
            // (the 2026 version called MessageBox — removed so the library is cross-platform/public).
            string content = game.ExportEntireGameToString();

            StreamWriter sw = new StreamWriter(filePath, false, XOFGame.UTF8_NoBOM);
            sw.Write(content);
            sw.Close();
        }

        /// <summary>
        /// This event will be fired after a game wrote success
        /// </summary>
        public event EventHandler? GameWroteSuccess;
        protected virtual void OnGameWroteSuccess()
        {
            GameWroteSuccess?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// This event will be fired after completed writing
        /// </summary>
        public event EventHandler? CompletedWriting;
        protected virtual void OnCompletedWriting()
        {
            CompletedWriting?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// The latest exception during writing games
        /// </summary>
        public Exception? LatestException { get; set; }

        /// <summary>
        /// This event will be fired on error during writing
        /// </summary>
        public event EventHandler? ErrorWriting;
        protected virtual void OnErrorWriting()
        {
            ErrorWriting?.Invoke(this, EventArgs.Empty);
        }

        public void WriteGames(List<XOFGame> glist, string filePath)
        {
            try
            {
                StreamWriter sw = new StreamWriter(filePath, false, XOFGame.UTF8_NoBOM);

                for (int x = 0; x < glist.Count; x++)
                {
                    XOFGame game = glist[x];

                    string content = game.ExportEntireGameToString();
                    sw.Write(content);

                    this.OnGameWroteSuccess(); //fire event
                }
                
                sw.Close();

                this.OnCompletedWriting(); //fire event
            }
            catch (Exception ex)
            {
                this.LatestException = ex;
                this.OnErrorWriting();
            }
        }

    }
}
