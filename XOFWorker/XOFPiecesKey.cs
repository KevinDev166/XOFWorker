using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFPiecesKey
    {
        public XOFPiecesKey()
        { 
            
        }

        public XOFPiecesKey(string langCode)
        {
            if (langCode.ToLower() == "en")
            {
                this.King = 'K';
                this.Assistant = 'A';
                this.Elephant = 'E';
                this.Rook = 'R';
                this.Cannon = 'C';
                this.Knight = 'N';
                this.Pawn = 'P';
            }
            else if (langCode.ToLower() == "vi")
            {
                this.King = 'T';
                this.Assistant = 'S';
                this.Elephant = 'V';
                this.Rook = 'X';
                this.Cannon = 'P';
                this.Knight = 'M';
                this.Pawn = 'C';
            }
            else if (langCode.ToLower() == "zh")
            // 2026-11-06 (decided): ONE character set for BOTH sides = Red's SIMPLIFIED characters (Simplified is the most widely
            // used; China's two different Red/Black sets are inconvenient for people who do not know Chinese characters well).
            // Previously 帥仕相俥炮碼卒 (old Traditional, mixed with Black's Pawn 卒, and 碼 = "code number" is the WRONG character
            // for the Knight). Anyone who prefers another set can fill in their own — XOF still decodes it correctly.
            {
                this.King = '帅';
                this.Assistant = '仕';
                this.Elephant = '相';
                this.Rook = '车';
                this.Cannon = '炮';
                this.Knight = '马';
                this.Pawn = '兵';
            }
        }

        public char King { get; set; }
        public char Assistant { get; set; }
        public char Elephant { get; set; }
        public char Rook { get; set; }
        public char Cannon { get; set; }
        public char Knight { get; set; }
        public char Pawn { get; set; }

        public List<char> GetKeyList()
        {
            List<char> list = new List<char>();
            list.Add(this.King);
            list.Add(this.Assistant);
            list.Add(this.Elephant);
            list.Add(this.Rook);
            list.Add(this.Cannon);
            list.Add(this.Knight);
            list.Add(this.Pawn);

            return list;
        }

        public char GetPieceCharByPieceType(string pieceType)
        {
            if (pieceType == XOFPieceType.King) return this.King;

            else if (pieceType == XOFPieceType.Assistant) return this.Assistant;

            else if (pieceType == XOFPieceType.Elephant) return this.Elephant;

            else if (pieceType == XOFPieceType.Rook) return this.Rook;

            else if (pieceType == XOFPieceType.Cannon) return this.Cannon;

            else if (pieceType == XOFPieceType.Knight) return this.Knight;

            else if (pieceType == XOFPieceType.Pawn) return this.Pawn;

            throw new ArgumentException("Invalid piece type.");
        }

        public string GetPieceTypeByPieceChar(char pieceChar)
        {
            if (pieceChar == this.King) return XOFPieceType.King;
            else if (pieceChar == this.Assistant) return XOFPieceType.Assistant;
            else if (pieceChar == this.Elephant) return XOFPieceType.Elephant;
            else if (pieceChar == this.Rook) return XOFPieceType.Rook;
            else if (pieceChar == this.Cannon) return XOFPieceType.Cannon;
            else if (pieceChar == this.Knight) return XOFPieceType.Knight;
            else if (pieceChar == this.Pawn) return XOFPieceType.Pawn;

            throw new ArgumentException("Unknown piece char.");
        }

        public string ExportToString()
        {
            //Header:
            //Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P

            string template = "King = [K], Assistant = [A], Elephant = [E], Rook = [R], Cannon = [C], Knight = [N], Pawn = [P]";
            string keyValue = template;

            keyValue = keyValue.Replace("[K]", this.King.ToString());
            keyValue = keyValue.Replace("[A]", this.Assistant.ToString());
            keyValue = keyValue.Replace("[E]", this.Elephant.ToString());
            keyValue = keyValue.Replace("[R]", this.Rook.ToString());
            keyValue = keyValue.Replace("[C]", this.Cannon.ToString());
            keyValue = keyValue.Replace("[N]", this.Knight.ToString());
            keyValue = keyValue.Replace("[P]", this.Pawn.ToString());

            return keyValue;
        }

        public string ExportToShortString()
        {
            //Header:
            //Pieces Key: King = K, Assistant = A, Elephant = E, Rook = R, Cannon = C, Knight = N, Pawn = P

            string template = "[K][A][E][R][C][N][P]";
            string keyValue = template;

            keyValue = keyValue.Replace("[K]", this.King.ToString());
            keyValue = keyValue.Replace("[A]", this.Assistant.ToString());
            keyValue = keyValue.Replace("[E]", this.Elephant.ToString());
            keyValue = keyValue.Replace("[R]", this.Rook.ToString());
            keyValue = keyValue.Replace("[C]", this.Cannon.ToString());
            keyValue = keyValue.Replace("[N]", this.Knight.ToString());
            keyValue = keyValue.Replace("[P]", this.Pawn.ToString());

            return keyValue;
        }

        public void ParseFromShortString(string shortKey)
        {
            if (string.IsNullOrEmpty(shortKey) || shortKey.Length != 7)
                throw new ArgumentException("Invalid short piece key.");

            this.King = shortKey[0];
            this.Assistant = shortKey[1];
            this.Elephant = shortKey[2];
            this.Rook = shortKey[3];
            this.Cannon = shortKey[4];
            this.Knight = shortKey[5];
            this.Pawn = shortKey[6];
        }

        public bool Validate()
        {
            HashSet<char> seen = new HashSet<char>();
            foreach (char c in this.GetKeyList())
            {
                if (seen.Contains(c))
                {
                    return false; // duplicate found => error
                }
                seen.Add(c);
            }

            return true; // no error
        }

        public bool IsTheSameAs(XOFPiecesKey secondKey)
        {
            if (this.King == secondKey.King &&
                this.Assistant == secondKey.Assistant &&
                this.Elephant == secondKey.Elephant &&
                this.Rook == secondKey.Rook &&
                this.Cannon == secondKey.Cannon &&
                this.Knight == secondKey.Knight &&
                this.Pawn == secondKey.Pawn)
            {
                return true;
            }
            else
                return false;
        }

    }
}
