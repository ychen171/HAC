using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAC
{
    delegate void InstrumentUpdateEventHandler(Tick a);
    delegate void FillEventHandler(int a, string b, string c, string d);
    delegate void UpdateEventHandler(double a, double b, double c, double d, double e, double f);

    enum Value_State
    {
        ABOVE, BELOW
    };

    enum Position
    {
        FLAT, LONG, SHORT
    };

    class Delegates
    {
    }
}
