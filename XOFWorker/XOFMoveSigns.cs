using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOFWorker
{
    public class XOFMoveSigns
    {

        public char Advancing { get; set; }
        public char Retreating { get; set; }
        public char Traversing { get; set; }

        public XOFMoveSigns(char advancing, char retreating, char traversing)
        {
            this.Advancing = advancing;
            this.Retreating = retreating;
            this.Traversing = traversing;
        }

        public List<char> GetList()
        {
            List<char> list = new List<char>();
            list.Add(this.Advancing);
            list.Add(this.Retreating);
            list.Add(this.Traversing);

            return list;
        }

        public void Validate(char iSign)
        {
            if (this.GetList().IndexOf(iSign) < 0)
                throw new ArgumentException("Invalid input move sign");
        }

    }
}
