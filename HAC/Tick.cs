using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAC
{
    class Tick
    {
        private double m_Price;
        public DateTime m_Time;
        public double m_Qty;

        public Tick(DateTime T, double P, double Q)
        {
            m_Time = T;
            m_Price = P;
            m_Qty = Q;
        }

        public DateTime Time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        public double Price
        {
            get { return m_Price; }
            set { m_Price = value; }
        }

        public double Qty
        {
            get { return m_Qty; }
            set { m_Qty = value; }
        }
    }
}
