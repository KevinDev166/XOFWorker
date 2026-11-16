using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XiangQiHelper
    {
        public static bool IsValidMoveText(string movetext, XOFPiecesKey key)
        {
            if (movetext.Length < 4) //xianqi move length
                return false;

            bool found = false;
            foreach (char pieceChar in key.GetKeyList())
            {
                if (movetext.Contains(pieceChar))
                    found = true;
            }

            return found;
        }
    }
}
