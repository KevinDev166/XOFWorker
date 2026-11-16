using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFHeader
    {
        public string Name { get; set; } = "";
        public string Value { get; set; } = "";

        public XOFHeader(string iname, string ivalue)
        {
            this.Name = iname.Trim();
            this.Value = ivalue.Trim();
        }

        public string GetHeaderLine()
        {
            return this.Name + ": " + this.Value;
        }

    }
}
