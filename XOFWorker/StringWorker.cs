using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class StringWorker
    {
        public static string[] SplitLinesByEOL(string icontent)
        {
            string[] hlines = icontent.Split(new string[] { XOFKeywords.WinEOL }, StringSplitOptions.None);
            if (hlines.Length > 1)
                return hlines;

            hlines = icontent.Split(new string[] { XOFKeywords.LinuxEOL }, StringSplitOptions.None);
            if (hlines.Length > 1)
                return hlines;

            hlines = icontent.Split(new string[] { XOFKeywords.MacEOL }, StringSplitOptions.None);
            if (hlines.Length > 1)
                return hlines;

            else throw new ArgumentException("Input is not a multi-lines string");
        }

    }

}
