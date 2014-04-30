using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeMatching;
using System.Diagnostics;

namespace HAC
{
    // Money Flow Index Strategy
    class SystemManager04
    {
        private Instrument m_Instrument;
        private List<Tick> m_TickList;

        private bool m_Go;
        private bool m_Start;

        private int m_Ticks;

        private double m_WeightedDiff;
        private double m_UpTotal;
        private double m_DownTotal;
        private double m_MF;
        private double m_MFI;

        private double m_Position;
        private double m_NetPos;

        private bool m_Bool;
        private Value_State m_State;

        private double m_Qty;

        private double m_Target;
        private double m_Stop;
        private int m_TargetTicks;
        private int m_StopTicks;

        private TradeMatcher m_Matcher;

        public event OnSystemUpdateEventHandler OnSystemUpdate;
        // public event FillEventHandler OnFill;

        public SystemManager04()
        {
            m_Matcher = new TradeMatcher(RoundTurnMethod.FIFO);

            // Create a new Instrument object.
            m_Instrument = new Instrument();
            m_Instrument.OnInstrumentUpdate += new OnInstrumentUpdateEventHandler(OnInstrumentUpdate);
            m_Instrument.OnInstrumentFill += new OnInstrumentFillEventHandler(OnInstrumentFill);

            // Create a new SortedList to hold the Tick objects.
            m_TickList = new List<Tick>();

            m_Position = 0;
            m_Go = false;
            m_Qty = 0;
        }

        ~SystemManager04()
        {
            //Debug::WriteLine( "SystemManager dying." );
        }

        private void OnInstrumentUpdate(Tick m_Tick)
        {
            m_TickList.Add(m_Tick);

            m_MFI = 50;
            m_MF = 0;

            // Begin calculation
            if (m_Ticks > 0 && m_TickList.Count > m_Ticks)
            {
                // Calculate the RS and RSI.
                m_UpTotal = 0;
                m_DownTotal = 0;
                for (int i = m_TickList.Count - m_Ticks; i < m_TickList.Count - 1; i++)
                {
                    m_WeightedDiff = (m_TickList[i].Price - m_TickList[i - 1].Price) * m_TickList[i].Qty;
                    if (m_WeightedDiff > 0)
                        m_UpTotal += m_WeightedDiff;
                    else if (m_WeightedDiff < 0)
                        m_DownTotal -= m_WeightedDiff;
                }
                m_MF = m_UpTotal / m_DownTotal;
                m_MFI = 100 - 100 / (1 + m_MF);
                Debug.WriteLine(m_MFI);

                //// Set the Value State.
                //if (m_MFI < 40)
                //    m_State = Value_State.LOW;
                //else if (m_MFI < 60)
                //    m_State = Value_State.MID;
                //else
                //    m_State = Value_State.HIGH;
            }

            // START/STOP Switch
            if (m_Go)
            {
                // If we already have a position on, and have either met out target or stop price, get out.
                if (m_Position > 0 && (m_Tick.Price > m_Target || m_Tick.Price < m_Stop))
                {
                    m_Bool = m_Instrument.EnterOrder("S", m_Qty, "TARGET/STOP OUT");
                }
                if (m_Position < 0 && (m_Tick.Price < m_Target || m_Tick.Price > m_Stop))
                {
                    m_Bool = m_Instrument.EnterOrder("B", m_Qty, "TARGET/STOP OUT");
                }

                // First time only and on reset, set initial state.
                if (m_Start)
                {
                    if (m_MFI >= 60)
                        m_State = Value_State.HIGH;
                    else if (m_MFI >= 40)
                        m_State = Value_State.MID;
                    else
                        m_State = Value_State.LOW;
                    m_Start = false;
                }

                // Has there been oversold?
                if (m_MFI < 40 && m_State != Value_State.LOW)
                {
                    // Change state.
                    m_State = Value_State.LOW;

                    // If we are already short, first get flat.
                    if (m_Position < 0)
                    {
                        m_Bool = m_Instrument.EnterOrder("B", m_Qty, "GET OUT");
                    }
                    // Go long.
                    m_Bool = m_Instrument.EnterOrder("B", m_Qty, "OPEN");

                    // Set target price and stop loss price.
                    m_Target = m_Tick.Price + m_TargetTicks * m_Instrument.TickSize();
                    m_Stop = m_Tick.Price - m_StopTicks * m_Instrument.TickSize();
                }

                // Has there been overbought?
                if (m_MFI >= 60 && m_State != Value_State.HIGH)
                {
                    // Change state.
                    m_State = Value_State.HIGH;

                    // If we are already long, first get flat.
                    if (m_Position > 0)
                    {
                        m_Bool = m_Instrument.EnterOrder("S", m_Qty, "GET OUT");
                    }
                    // Go short.
                    m_Bool = m_Instrument.EnterOrder("S", m_Qty, "OPEN");

                    // Set target price and stop loss price.
                    m_Target = m_Tick.Price - m_TargetTicks * m_Instrument.TickSize();
                    m_Stop = m_Tick.Price + m_StopTicks * m_Instrument.TickSize();
                }
            }
            // Send the data to the GUI.
            OnSystemUpdate(m_Tick.Price, m_Tick.Qty, m_MFI, m_MF, m_Target, m_Stop);
        }

        private void OnInstrumentFill(int qty, string BS, string px, string key)
        {
            // Update position.
            if (BS == "B")
            {
                m_Position += qty;
            }
            else
            {
                m_Position -= qty;
            }

            // Send the data to the TradeMacher.
            TradeMatching.Fill m_Fill = new TradeMatching.Fill();
            if (BS == "B")
                m_Fill.BS = TradeMatching.TradeType.BUY;
            else
                m_Fill.BS = TradeMatching.TradeType.SELL;

            m_Fill.Price = Convert.ToDouble(px);
            m_Fill.TradeID = key;
            m_Fill.Qty = qty;
            m_Matcher.Fill_Received(m_Fill);

            m_NetPos = m_Matcher.NetPos;
        }

        public void StartStop()
        {
            if (m_Go == false)
            {
                m_Go = true;
                m_Start = true;
            }
            else
            {
                m_Go = false;
            }
        }

        public void ShutDown()
        {
            m_Go = false;
            m_Instrument.ShutDown();
            m_Instrument.OnInstrumentUpdate -= new OnInstrumentUpdateEventHandler(OnInstrumentUpdate);
            m_Instrument.OnInstrumentFill -= new OnInstrumentFillEventHandler(OnInstrumentFill);
            m_Instrument = null;
        }

        public double Qty
        {
            get { return m_Qty; }
            set { m_Qty = value; }
        }

        public double Bid
        {
            get { return m_Instrument.Bid; }
        }

        public double Ask
        {
            get { return m_Instrument.Ask; }
        }

        public double Position
        {
            get { return m_Position; }
        }

        public double NetPos
        {
            get { return m_NetPos; }
        }

        public int StopTicks
        {
            get { return m_StopTicks; }
            set { m_StopTicks = value; }
        }

        public int TargetTicks
        {
            get { return m_TargetTicks; }
            set { m_TargetTicks = value; }
        }

        public int Ticks
        {
            get { return m_Ticks; }
            set { m_Ticks = value; }
        }

        public TradeMatcher Matcher
        {
            get { return m_Matcher; }
        }
    }
}
