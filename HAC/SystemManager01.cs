using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeMatching;
using System.Diagnostics;

namespace HAC
{
    // Stochastic Oscillator Strategy
    class SystemManager01
    {
        private Instrument m_Instrument;
        private List<Tick> m_TickList;

        private bool m_Go;
        private bool m_Start;

        private int m_Ticks;

        private double m_Max;
        private double m_Min;
        private List<double> m_RSV;
        private double m_K;
        private double m_D;

        private double m_Position;
        private double m_NetPos;

        private bool m_Bool;
        private Cross_State m_State;

        private double m_Qty;

        private double m_Target;
        private double m_Stop;
        private int m_TargetTicks;
        private int m_StopTicks;

        private TradeMatcher m_Matcher;

        public event OnSystemUpdateEventHandler OnSystemUpdate;
        // public event FillEventHandler OnFill;

        public SystemManager01()
        {
            m_Matcher = new TradeMatcher(RoundTurnMethod.FIFO);

            // Create a new Instrument object.
            m_Instrument = new Instrument("ES", "201406", Instrument.InstrumentType.FUTURE);
            m_Instrument.BidAskUpdate += new DataUpdateEventHandler(OnInstrumentUpdate);
            m_Instrument.FillUpdate += new FillEventHandler(OnInstrumentFill);

            // Create a new SortedList to hold the Tick objects.
            m_TickList = new List<Tick>();
            m_RSV = new List<double>();

            m_Position = 0;
            m_Go = false;
            m_Qty = 0;
        }

        ~SystemManager01()
        {
            //Debug::WriteLine( "SystemManager dying." );
        }

        private void OnInstrumentUpdate(Tick m_Tick)
        {
            m_TickList.Add(m_Tick);

            m_K = 0;
            m_D = 0;

            // Begin calculating
            if (m_Ticks > 0 && m_TickList.Count > m_Ticks)
            {
                // Calculate the K and D values.
                m_Max = 0;
                m_Min = 1000000000;
                for (int i = m_TickList.Count - m_Ticks; i < m_TickList.Count - 1; i++)
                {
                    m_Max = Math.Max(m_Max, m_TickList[i].Price);
                    m_Min = Math.Min(m_Min, m_TickList[i].Price);
                }
                m_RSV.Add((m_TickList.Last().Price - m_Min) / (m_Max - m_Min) * 100);
                //Debug.WriteLine(m_RSV.Last());
                if (m_RSV.Count >= 3)
                    m_D = (m_RSV[m_RSV.Count - 1] + m_RSV[m_RSV.Count - 2] + m_RSV[m_RSV.Count - 3]) / 3;
                m_K = m_RSV.Last();
                //Debug.WriteLine(m_K);
                //Debug.WriteLine(m_D);

                //// Set the Cross State.
                //if (m_K > m_D)
                //    m_State = Cross_State.ABOVE;
                //else
                //    m_State = Cross_State.BELOW;

            }

            // START/STOP Switch
            if (m_Go)
            {
                // If we already have a position on, and have either met out target or stop price, get out.
                if (m_Position > 0 && (m_Tick.Price > m_Target || m_Tick.Price < m_Stop))
                {
                    m_Instrument.EnterMarketOrder("S", m_Qty.ToString());
                }
                if (m_Position < 0 && (m_Tick.Price < m_Target || m_Tick.Price > m_Stop))
                {
                    m_Instrument.EnterMarketOrder("B", m_Qty.ToString());
                }

                // First time only and on reset, set initial state.
                if (m_Start)
                {
                    if (m_K > m_D)
                        m_State = Cross_State.ABOVE;
                    else
                        m_State = Cross_State.BELOW;
                    m_Start = false;
                }

                // Has there been a crossover up?
                if (m_K > m_D && m_State == Cross_State.BELOW)
                {
                    // Change state.
                    m_State = Cross_State.ABOVE;

                    // If we are already short, first get flat.
                    if (m_Position < 0)
                    {
                        m_Instrument.EnterMarketOrder("B", m_Qty.ToString());
                    }
                    // Go long.
                    m_Instrument.EnterMarketOrder("B", m_Qty.ToString());

                    // Set target price and stop loss price.
                    m_Target = m_Tick.Price + m_TargetTicks * m_Instrument.get_TickSize();
                    m_Stop = m_Tick.Price - m_StopTicks * m_Instrument.get_TickSize();
                }

                // Has there been a crossover down?
                if (m_K < m_D && m_State == Cross_State.ABOVE)
                {
                    // Change state.
                    m_State = Cross_State.BELOW;

                    // If we are already long, first get flat.
                    if (m_Position > 0)
                    {
                        m_Instrument.EnterMarketOrder("S", m_Qty.ToString());
                    }
                    // Go short.
                    m_Instrument.EnterMarketOrder("S", m_Qty.ToString());

                    // Set target price and stop loss price.
                    m_Target = m_Tick.Price - m_TargetTicks * m_Instrument.get_TickSize();
                    m_Stop = m_Tick.Price + m_StopTicks * m_Instrument.get_TickSize();
                }
            }
            // Send the data to the GUI.
            OnSystemUpdate(m_Tick.Price, m_Tick.Qty, m_D, m_K, m_Target, m_Stop);
        }

        private void OnInstrumentFill(Fill x)
        {
            // Update position.
            if (x.BuySell == "B")
            {
                m_Position += x.Qty;
            }
            else
            {
                m_Position -= x.Qty;
            }

            //// Send the data to the TradeMacher.
            //Fill m_Fill = new Fill();
            //if (BS == "B")
            //    m_Fill.BS = TradeType.BUY;
            //else
            //    m_Fill.BS = TradeType.SELL;

            //m_Fill.Price = Convert.ToDouble(price);
            //m_Fill.TradeID = key;
            //m_Fill.Qty = qty;
            //m_Matcher.Fill_Received(m_Fill);

            m_NetPos = m_Position;
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
            //m_Instrument.ShutDown();
            //m_Instrument.OnInstrumentUpdate -= new InstrumentUpdateEventHandler(OnInstrumentUpdate);
            //m_Instrument.OnFill -= new FillEventHandler(OnInstrumentFill);
            m_Instrument = null;
        }

        public double Qty
        {
            get { return m_Qty; }
            set { m_Qty = value; }
        }

        public double Bid
        {
            get { return m_Instrument.get_Bid(); }
        }

        public double Ask
        {
            get { return m_Instrument.get_Ask(); }
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
