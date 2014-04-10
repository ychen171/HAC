using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAC
{
    class Tick
    {
        public double Price;
        public DateTime Time;
        public double Qty;

        public Tick(DateTime T, double P, double Q)
        {
            Time = T;
            Price = P;
            Qty = Q;
        }
    }
}
