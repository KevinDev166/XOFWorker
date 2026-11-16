using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFPieceType
    {
        public static string King = "King";
        public static string Assistant = "Assistant";
        public static string Elephant = "Elephant";
        public static string Rook = "Rook";
        public static string Cannon = "Cannon";
        public static string Knight = "Knight";
        public static string Pawn = "Pawn";

        public static List<string> GetList()
        {
            List<string> all = new List<string>();

            all.Add(King);
            all.Add(Assistant);
            all.Add(Elephant);
            all.Add(Rook);
            all.Add(Cannon);
            all.Add(Knight);
            all.Add(Pawn);

            return all;
        }

        public static bool IsValid(string iname)
        {
            List<string> all = GetList();
            if (all.IndexOf(iname) >= 0)
                return true;
            else
                return false;
        }

        public static void Validate(string iname)
        {
            if (IsValid(iname) == false)
                throw new ArgumentException("Invalid piece type.");
        }
    }
}
