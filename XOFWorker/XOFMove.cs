using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFMove
    {
        public XOFMove()
        {
            this.Variants = new List<List<XOFMove>>();
        }

        public bool IsMainLine { get; set; }
        public int Number { get; set; }
        public string MoveText { get; set; } = "";
        public string PlayerColor { get; set; } = "";
        public string Date { get; set; } = "";
        public string? Comment { get; set; }
        public List<List<XOFMove>> Variants { get; set; }

        public string ExportToTextLine()
        {
            string mline = "";

            //[ML/VA] [Number] [Move text] [Side] [Date time] [Comment]

            mline += (this.IsMainLine ? XOFKeywords.ML : XOFKeywords.VA);
            mline += XOFKeywords.ColumnSeparator;

            mline += this.Number.ToString();
            mline += XOFKeywords.ColumnSeparator;

            mline += this.MoveText;
            mline += XOFKeywords.ColumnSeparator;

            mline += this.PlayerColor;
            mline += XOFKeywords.ColumnSeparator;

            mline += this.Date;
            mline += XOFKeywords.ColumnSeparator;

            string comment = this.Comment == null ? "" : this.Comment;
            comment = comment.Replace("\t", " "); //a comment cannot contain a tab
            comment = comment.Replace(XOFKeywords.WinEOL, " "); //a comment cannot contain a newline
            comment = comment.Replace(XOFKeywords.LinuxEOL, " "); //a comment cannot contain a newline
            comment = comment.Replace(XOFKeywords.MacEOL, " "); //a comment cannot contain a newline

            mline += comment;
            mline += XOFKeywords.ColumnSeparator;

            if (this.Variants.Count > 0)
            {
                //variants without any move must be removed
                for (int x = this.Variants.Count - 1; x >= 0; x--)
                {
                    List<XOFMove> list = this.Variants[x];
                    if (list.Count == 0)
                        this.Variants.RemoveAt(x);
                }

                //process the variants that remain
                if (this.Variants.Count > 0)
                {
                    mline += Environment.NewLine;

                    for (int x = 0; x < this.Variants.Count; x++)
                    {
                        List<XOFMove> list = this.Variants[x];

                        //only take lists that have moves, to avoid an empty variant
                        if (list.Count > 0)
                        {
                            mline += XOFKeywords.OpenVariant + Environment.NewLine;

                            foreach (XOFMove m in list)
                            {
                                string cline = m.ExportToTextLine();
                                mline += cline + Environment.NewLine;
                            }

                            if (x == this.Variants.Count - 1) //last variant => no trailing newline
                                mline += XOFKeywords.CloseVariant;
                            else //middle variant => add a newline to avoid "VARIANT][VARIANT"
                                mline += XOFKeywords.CloseVariant + Environment.NewLine;
                        }
                    }
                }
            }

            return mline;
        }
    }
}
