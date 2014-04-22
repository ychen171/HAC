using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAC
{
    delegate void OnInstrumentUpdateEventHandler(Tick a);
    delegate void OnInstrumentFillEventHandler(int a, string b, string c, string d);
    delegate void OnSystemUpdateEventHandler(double a, double b, double c, double d, double e, double f);

    enum Cross_State
    {
        // ABOVE means that the short indicator is above the long indicator
        // BELOW means that the short indicator is below the long indicator
        ABOVE, BELOW
    };

    enum Value_State
    {
        // LOW means oversold, undervalued.
        // HIGH means overbought, overvalued.
        LOW, MID, HIGH
    };

    enum Position
    {
        FLAT, LONG, SHORT
    };

    class Delegates
    {
    }
}
