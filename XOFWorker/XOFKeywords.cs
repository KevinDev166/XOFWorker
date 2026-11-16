using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFKeywords
    {
        
        //SEPARATORS
        public static string GameSeparator = "------------------";
        public static string SectionSeparator = "---------";

        public static char HeaderSeparator = ':';
        public static char PieceKeyColSeparator = ',';
        public static char PieceKeyPairSeparator = '=';
        public static string ColumnSeparator = "\t";

        public static string OpenVariant = "[VARIANT";
        public static string CloseVariant = "VARIANT]";

        public static string ML = "ML";
        public static string VA = "VA";

        public static string R = "R";
        public static string B = "B";

        //HEADER NAMES
        public static string Event = "Event";
        public static string Round = "Round";
        public static string GameNumber = "Game Number";
        public static string Date = "Date";
        public static string Address = "Address";
        public static string RedPlayer = "Red Player";
        public static string BluePlayer = "Blue Player";
        public static string RedTeam = "Red Team";
        public static string BlueTeam = "Blue Team";
        public static string StartingFEN = "Starting FEN";
        public static string Result = "Result";
        public static string PiecesKey = "Pieces Key";

        //PIECE KEYS
        public static string King = "King";
        public static string Assistant = "Assistant";
        public static string Elephant = "Elephant";
        public static string Rook = "Rook";
        public static string Cannon = "Cannon";
        public static string Knight = "Knight";
        public static string Pawn = "Pawn";

        //END OF LINES
        public static string WinEOL = "\r\n";
        public static string LinuxEOL = "\n";
        public static string MacEOL = "\r";

        //MOVE SIGNS
        public static string MovePlus = "+"; //important, don't change
        public static string MoveMinus = "-"; //important, don't change
        public static string MoveEqual = "="; //important, don't change

        //FEN
        public static string DefaultFEN = "rnbakabnr/9/1c5c1/p1p1p1p1p/9/9/P1P1P1P1P/1C5C1/9/RNBAKABNR w - - 0 1";

        public static List<string> GetRequiredHeaderNames()
        {
            List<string> list = new List<string>();

            list.Add(Event);
            list.Add(Round);
            list.Add(GameNumber);
            list.Add(Date);
            list.Add(Address);
            list.Add(RedPlayer);
            list.Add(BluePlayer);
            list.Add(RedTeam);
            list.Add(BlueTeam);
            list.Add(StartingFEN);
            list.Add(Result);
            list.Add(PiecesKey);

            return list;
        }

    }
}
