using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TJADotNet.Format
{
    public class Measure
    {
        public Measure(double part, double beat)
        {
            Part = part;
            Beat = beat;
        }
        public double Part { get; set; }
        public double Beat { get; set; }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Part, Beat);
        }

        public double GetRate()
        {
            return 240 * (Part / Beat);
        }
    }
}
