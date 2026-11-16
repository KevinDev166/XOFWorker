using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFReader
    {
        public string FilePath { get; set; } = "";
        public XOFParsingInfo ParsingInfo { get; set; } = new();
        public List<XOFGame> Games { get; set; } = new();

        /// <summary>
        /// This event will be fired after a game parsed success
        /// </summary>
        public event EventHandler<XOFGameEventArgs>? GameParsedSuccess;
        protected virtual void OnGameParsedSuccess(XOFGame igame, XOFParsingInfo iInfo)
        {
            GameParsedSuccess?.Invoke(this, new XOFGameEventArgs(igame, iInfo));
        }

        /// <summary>
        /// This event will be fired after completed parsing
        /// </summary>
        public event EventHandler<XOFGameEventArgs>? CompletedParsing;
        protected virtual void OnCompletedParsing(XOFParsingInfo iInfo)
        {
            CompletedParsing?.Invoke(this, new XOFGameEventArgs(null, iInfo));
        }

        /// <summary>
        /// The latest exception during writing games
        /// </summary>
        public Exception? LatestException { get; set; }

        /// <summary>
        /// This event will be fired on error during writing
        /// </summary>
        public event EventHandler<XOFGameEventArgs>? ErrorParsing;
        protected virtual void OnErrorParsing(XOFParsingInfo iInfo)
        {
            ErrorParsing?.Invoke(this, new XOFGameEventArgs(null, iInfo));
        }

        public XOFReader()
        {

        }

        public XOFReader(string ifilePath)
        {
            if (File.Exists(ifilePath) == false)
                throw new ArgumentException("XOF file does not exist");

            this.FilePath = ifilePath;
            this.Games = new List<XOFGame>();

            //this.ParseFile();
        }

        public void ParseFile()
        {
            ParsingInfo = new XOFParsingInfo();
            ParsingInfo.FilePath = this.FilePath;
            ParsingInfo.GameParsedSuccess = 0;
            ParsingInfo.GameParsedFailed = 0;
            ParsingInfo.Message = "";
            ParsingInfo.ReadingLine = 0;

            try
            {
                string ccontent = "";

                // Processes ONE game whose lines have all been collected (separator line reached, or end of file).
                void FlushGame()
                {
                    try
                    {
                        XOFGame cgame = this.ParseGame(ccontent);

                        if (cgame != null) //parsed success
                        {
                            this.Games.Add(cgame);
                            ParsingInfo.GameParsedSuccess++;
                            this.OnGameParsedSuccess(cgame, ParsingInfo); //fire event
                        }
                    }
                    catch (Exception ex) //parsed failed
                    {
                        ParsingInfo.GameParsedFailed++;
                        ParsingInfo.Message = "Parsed failed: " + ex.Message;
                        this.OnErrorParsing(ParsingInfo); //fire event
                    }

                    ccontent = ""; //reset it
                }

                // using: the file is closed even if an error occurs midway (previously sr.Close() was skipped on an exception).
                using (StreamReader sr = new StreamReader(this.FilePath, XOFGame.UTF8_NoBOM))
                {
                    while (sr.EndOfStream == false)
                    {
                        string? cline = sr.ReadLine();
                        ParsingInfo.ReadingLineContent = cline ?? "";

                        if (string.IsNullOrEmpty(cline)) //skip empty lines
                            continue;

                        if (cline == XOFKeywords.GameSeparator)
                            FlushGame();
                        else
                            ccontent += Environment.NewLine + cline.Trim();

                        ParsingInfo.ReadingLine++;
                    }
                }

                // The LAST game has no separator line after it (file written by another tool / edited by hand) — before 2026-10-20
                // this game was SILENTLY DROPPED. Now it is still read; broken content is counted in GameParsedFailed like any other bad game.
                if (ccontent.Trim().Length > 0)
                    FlushGame();

                this.OnCompletedParsing(ParsingInfo); //fire event
            }
            catch (Exception ex)
            {
                this.LatestException = ex;
                ParsingInfo.Message = "Got exception: " + ex.Message;
                this.OnErrorParsing(ParsingInfo);
            }
            
        }

        public XOFGame ParseGame(string icontent)
        {
            icontent = icontent.Trim();
            
            string[] parts = icontent.Split(new string[] { XOFKeywords.SectionSeparator }, StringSplitOptions.None);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid XOF Game string.");

            string headerContent = parts[0];
            string moveContent = parts[1];

            XOFGame xgame = new XOFGame();
            
            //PARSE THE HEADER
            string[] hlines = StringWorker.SplitLinesByEOL(headerContent);

            for (int x = 0; x < hlines.Length; x++)
            {
                string cline = hlines[x];

                if (string.IsNullOrEmpty(cline)) //skip empty lines
                    continue;

                XOFHeader header = ParseHeader(cline);
                xgame.Headers.Add(header);

                if (header.Name == XOFKeywords.PiecesKey) //parse pieces key
                {
                    XOFPiecesKey key = ParsePiecesKey(header.Value);
                    xgame.Keys = key;
                }
            }

            //check if missing required header
            foreach (string hname in XOFKeywords.GetRequiredHeaderNames())
            {
                bool found = false;
                foreach (XOFHeader header in xgame.Headers)
                {
                    if (header.Name == hname)
                    {
                        found = true;
                    } 
                }

                if (found == false)
                    throw new ApplicationException("Missing required header: " + hname);
            }

            //PARSE THE MOVE TEXT
            string[] mtlines = StringWorker.SplitLinesByEOL(moveContent);

            for (int x = 0; x < mtlines.Length; x++)
            {
                string cline = mtlines[x];

                if (string.IsNullOrEmpty(cline)) //skip empty lines
                    continue;

                /*
                 * A line can be a move line, i.e. ML or VA
                 * But a line can also be [VARIANT or VARIANT]
                 */

                XOFMove move = ParseMoveText(cline, xgame.Keys);

                //recursive function: parses the variants of the move above, depth first
                int nextindex = x + 1;

                //must use while because several variants can follow one another
                while ( nextindex < mtlines.Length && mtlines[nextindex].Trim() == XOFKeywords.OpenVariant)
                {
                    int completedIndex = 0;

                    //this is the move above; the variant will be added to it
                    move = ParseVariant(move, xgame.Keys, mtlines, nextindex, out completedIndex);

                    x = completedIndex; //index of the VARIANT] line
                    nextindex = x + 1;
                }
                
                xgame.Moves.Add(move);
            }

            return xgame;
        }

        public XOFMove ParseVariant(XOFMove rootMove, XOFPiecesKey key, string[] mtLines, int startingIndex, out int completedIndex)
        {
            if (startingIndex > mtLines.Length)
                throw new ArgumentException("Invalid starting index for variant parsing.");
            
            int x = startingIndex;

            //the first line is [VARIANT, so skip it and use the next index
            if (mtLines[x].Trim() == "[VARIANT")
                x++;

            //the line at the next index must be a VA line
            if (mtLines[x].StartsWith(XOFKeywords.VA) == false)
                throw new ArgumentException("Invalid starting string for variant line.");

            List<XOFMove> movesList = new List<XOFMove>();

            bool foundCloseTag = false;
            while (foundCloseTag == false && x < mtLines.Length)
            {
                if (mtLines[x].Trim() == XOFKeywords.CloseVariant)
                {
                    foundCloseTag = true;
                    break;
                }

                if (mtLines[x].Trim() == XOFKeywords.OpenVariant)
                {
                    int finishedIndex = 0;

                    XOFMove childMove = ParseVariant(movesList[movesList.Count - 1], key, mtLines, x, out finishedIndex);

                    //the move is already in the list, so do not add it again
                    //movesList.Add(childMove); //this move contains the variant

                    x = finishedIndex;
                }

                else if (mtLines[x].Trim().StartsWith(XOFKeywords.ML) || mtLines[x].Trim().StartsWith(XOFKeywords.VA))
                {
                    //not a close-variant or open-variant line, so it is a move text line
                    XOFMove m = ParseMoveText(mtLines[x], key);
                    movesList.Add(m);
                }

                x++; //for next loop
            }


            completedIndex = x; //index of the VARIANT] line

            rootMove.Variants.Add(movesList);

            return rootMove;
        }

        public XOFHeader ParseHeader(string headerLine)
        {
            headerLine = headerLine.Trim();

            string[] parts = headerLine.Split(XOFKeywords.HeaderSeparator);
            if (parts.Length != 2)
                throw new ArgumentException("Invalid header line input string.");

            XOFHeader header = new XOFHeader(parts[0], parts[1]);
            return header;
        }

        public XOFMove ParseMoveText(string moveTextLine, XOFPiecesKey key)
        {
            //[ML/VA] [Number] [Move text] [Side] [Date time] [Comment]

            moveTextLine = moveTextLine.Trim();

            string[] parts = moveTextLine.Split(new string[] { XOFKeywords.ColumnSeparator}, StringSplitOptions.None);
            if (parts.Length < 4)
                throw new ArgumentException("Invalid move text line input string.");

            XOFMove move = new XOFMove();

            if (parts[0].StartsWith(XOFKeywords.ML) == false && 
                parts[0].StartsWith(XOFKeywords.VA) == false)
                throw new ArgumentException("Invalid starting string for move text line.");
            move.IsMainLine = parts[0] == XOFKeywords.ML;

            int moveNumber = 0;
            if (int.TryParse(parts[1], out moveNumber) == false)
                throw new ArgumentException("Invalid move number in move text line.");
            move.Number = moveNumber;

            if (XiangQiHelper.IsValidMoveText(parts[2], key) == false)
                throw new ArgumentException("Invalid move text in move text line.");
            move.MoveText = parts[2];

            if (parts[3] != "R" && parts[3] != "B")
                throw new ArgumentException("Invalid move side in move text line.");
            move.PlayerColor = parts[3];

            if (parts.Length >= 5)
                move.Date = parts[4]; //nullable

            if (parts.Length >= 6)
                move.Comment = parts[5]; //nullable

            return move;
        }

        public XOFPiecesKey ParsePiecesKey(string icontent)
        {
            //Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P

            icontent = icontent.Trim();

            string[] parts = icontent.Split(XOFKeywords.PieceKeyColSeparator);

            if (parts.Length != 7)
                throw new ArgumentException("Invalid pieces key input string.");

            //INDEX = 0 => KING KEY
            string[] keys = parts[0].Split(XOFKeywords.PieceKeyPairSeparator);

            XOFPiecesKey key = new XOFPiecesKey();

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.King || keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for King.");
            key.King = keys[1].Trim()[0];

            //INDEX = 1 => ASSISTANT KEY
            keys = parts[1].Split(XOFKeywords.PieceKeyPairSeparator);

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.Assistant|| keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for Assistant.");
            key.Assistant = keys[1].Trim()[0];

            //INDEX = 2 => ELEPHANT KEY
            keys = parts[2].Split(XOFKeywords.PieceKeyPairSeparator);

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.Elephant || keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for Elephant.");
            key.Elephant = keys[1].Trim()[0];

            //INDEX = 3 => ROOK KEY
            keys = parts[3].Split(XOFKeywords.PieceKeyPairSeparator);

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.Rook || keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for Rook.");
            key.Rook = keys[1].Trim()[0];

            //INDEX = 4 => CANNON KEY
            keys = parts[4].Split(XOFKeywords.PieceKeyPairSeparator);

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.Cannon || keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for Cannon.");
            key.Cannon = keys[1].Trim()[0];

            //INDEX = 5 => KNIGHT KEY
            keys = parts[5].Split(XOFKeywords.PieceKeyPairSeparator);

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.Knight || keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for Knight.");
            key.Knight = keys[1].Trim()[0];

            //INDEX = 6 => PAWN KEY
            keys = parts[6].Split(XOFKeywords.PieceKeyPairSeparator);

            if (keys.Length != 2 || keys[0].Trim() != XOFKeywords.Pawn || keys[1].Trim().Length != 1)
                throw new ArgumentException("Invalid pieces key for Pawn.");
            key.Pawn = keys[1].Trim()[0];

            return key;
        }

    }
}
