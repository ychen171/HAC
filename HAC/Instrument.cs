using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Krs.Ats.IBNet;
using Krs.Ats.IBNet.Contracts;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;


namespace HAC
{
    delegate void UpdateEventHandler( double x);
    delegate void FillEventHandler( Fill x );
    delegate void DataUpdateEventHandler(Tick x);
    delegate void FillUpdateEventHandler( Object sender, ExecDetailsEventArgs e );
    //delegate void LimitOrderEventHandler();
    delegate void PriceUpdateEventHandler(Object sender, TickPriceEventArgs e);
    delegate void SizeUpdateEventHandler(Object sender, TickSizeEventArgs e);

    class Instrument : Form
    {
    	private Contract m_Contract;
    
    	private static Dictionary < int, Instrument  > contracts;
    	private static IBClient m_Client;
    	private static int id;
        //private static PriceUpdateEventHandler S_OnPriceUpdateDelegate;
        //private static SizeUpdateEventHandler S_OnSizeUpdateDelegate;
        //private static FillUpdateEventHandler S_OnFillUpdateDelegate;
    	private PriceUpdateEventHandler OnPriceUpdateDelegate;
    	private SizeUpdateEventHandler OnSizeUpdateDelegate;
    	private FillUpdateEventHandler OnFillUpdateDelegate;
	
        //private void I_OnPriceDataUpdate( Object o, TickPriceEventArgs  );
        //private void I_OnSizeDataUpdate( Object o, TickSizeEventArgs  );
        //private void I_OnFill( Object o, ExecDetailsEventArgs  );

        //private static void OnHistoricalDataUpdate( Object , HistoricalDataEventArgs  );
	
    	private static int NextOrderId;
    	private int m_ID;

        //// these are KRS API Event Handlers that run on KRS threads
        //private static void S_OnSizeDataUpdate(Object sender, TickSizeEventArgs e);
        //private static void S_OnPriceDataUpdate(Object sender, TickPriceEventArgs e);
        //private static void S_OnFill( Object sender, ExecDetailsEventArgs e );
        //private static void S_OnError( Object o, Krs.Ats.IBNet.ErrorEventArgs  );
        //private static void S_OnNextValidId( Object o, NextValidIdEventArgs  );

        //// these are event handlers that run on my thread.
        //private void Client_TickPrice(Object sender, TickPriceEventArgs e);
        //private void Client_TickSize(Object sender, TickSizeEventArgs e);
        //private void Client_Fill( Object sender, ExecDetailsEventArgs e);

    	private int m_TickSize;
    	private double m_Bid;
    	private double m_Ask;
    	private double m_BidQty;
    	private double m_AskQty;
    	private double m_Last;
    	private double m_LastQty;
	
    	public String Symbol;

    	public enum InstrumentType
    	{
    		EQUITY,
    		FOREX,
    		FUTURE,
	    	INDEX,
	    	OPTION
	    };


        public event FillEventHandler FillUpdate;
	    //public event LimitOrderEventHandler LimitOrderUpdate;
	    public event DataUpdateEventHandler BidAskUpdate;

        public Instrument( String m_Symbol, String m_Expiry, InstrumentType m_Type )
        {
    	    this.CreateHandle();
        	this.Visible = false;
    	
        	if ( m_Client == null )
        	{
        		m_Client = new IBClient();
        		m_Client.ThrowExceptions = true;
        		m_Client.TickPrice += new EventHandler< TickPriceEventArgs >( S_OnPriceDataUpdate );
        		m_Client.TickSize += new EventHandler< TickSizeEventArgs >( S_OnSizeDataUpdate);
        		m_Client.Error += new EventHandler< ErrorEventArgs >( S_OnError);
          		m_Client.NextValidId += new EventHandler< NextValidIdEventArgs >( S_OnNextValidId);
            	//m_Client.OrderStatus += new EventHandler< OrderStatusEventArgs >( OnOrderStatus);
        		m_Client.ExecDetails += new EventHandler< ExecDetailsEventArgs >( S_OnFill);

            	m_Client.HistoricalData += new EventHandler< HistoricalDataEventArgs >( OnHistoricalDataUpdate );

	        	m_Client.Connect("127.0.0.1", 7496, 0);

	        	contracts = new Dictionary< int, Instrument  >();
        	}

        	m_ID = id;
        	id++;
	
        	Symbol = m_Symbol;

        	switch( m_Type )
        	{
        		case InstrumentType.EQUITY:
        			m_Contract = new Equity( m_Symbol );
		        	break;
	        	case InstrumentType.FOREX:
	        		//m_Contract = new Forex();
	        		break;
	        	case InstrumentType.FUTURE:
	        		m_Contract = new Future( m_Symbol, "GLOBEX", m_Expiry );
	        		break;
		
                    //case InstrumentType.FUTURE:
                    //    m_Contract = new Future( "CCK2", "NYBOT", "JUN12" );
                    //    break;

	        	case InstrumentType.INDEX:
        			m_Contract = new Index( m_Symbol, "CBOE" );
        			break;
        		case InstrumentType.OPTION:
        			m_Contract = new Option( "IBM", "IBM   120518C00170000", 2012, 5, Krs.Ats.IBNet.RightType.Call, 170 );
        			break;
        	};

            m_Client.RequestMarketData( m_ID, m_Contract, null, false, false );
        	contracts.Add( m_ID, this );

        	//client.RequestExecutions(34, new ExecutionFilter());
	
            ///////////////////////////////////////////////////////////////////
        	////////////  These delegates perform cross-thread operation ///////
        	////////////////////////////////////////////////////////////////////
        	OnPriceUpdateDelegate = new PriceUpdateEventHandler( Client_TickPrice );
        	OnSizeUpdateDelegate = new SizeUpdateEventHandler( Client_TickSize );
        	OnFillUpdateDelegate = new FillUpdateEventHandler( Client_Fill );
        	////////////////////////////////////////////////////////////////////
        }

~Instrument()
{
	m_Client.Disconnect();
	m_Client.ReadThread.Abort();
	m_Client.ReadThread.Join();
	m_Client.TickPrice -= new EventHandler< TickPriceEventArgs >( S_OnPriceDataUpdate );
	m_Client.TickSize -= new EventHandler< TickSizeEventArgs >( S_OnSizeDataUpdate);
	m_Client.Error -= new EventHandler< ErrorEventArgs >( S_OnError );
	m_Client.NextValidId -= new EventHandler< NextValidIdEventArgs >( S_OnNextValidId );
	//m_Client.OrderStatus -= new EventHandler< OrderStatusEventArgs >( &OnOrderStatus);
	m_Client.ExecDetails -= new EventHandler< ExecDetailsEventArgs >( S_OnFill);
	
	m_Contract = null;
	
	m_Client = null;
}

private void S_OnError( Object sender, Krs.Ats.IBNet.ErrorEventArgs e )
{
	//MessageBox.Show( e.ErrorMsg );
}

private void S_OnNextValidId(Object sender, NextValidIdEventArgs e)
{
    NextOrderId = e.OrderId;
}
        
///////////////////////////////////////////////////////////////////////////////////
//////////////////////// Real time tick data //////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////////
///////  Switch event update to main thread ///////////////////////////////////////

private void S_OnPriceDataUpdate(Object sender, TickPriceEventArgs e)
{
	Instrument p = contracts[ e.TickerId ];
	p.I_OnPriceDataUpdate( sender, e );
}
private void I_OnPriceDataUpdate(Object sender, TickPriceEventArgs e)
{
	this.BeginInvoke( OnPriceUpdateDelegate, sender, e );
}

private void S_OnSizeDataUpdate(Object sender, TickSizeEventArgs e)
{
	Instrument p = contracts[ e.TickerId ];
	p.I_OnSizeDataUpdate( sender, e );
}
private void S_OnFill(Object sender, ExecDetailsEventArgs e)
{
	foreach( KeyValuePair< int, Instrument  > x in contracts )
	{
		if ( e.Contract.Symbol == x.Value.Symbol )
		{
			x.Value.I_OnFill( sender, e );
			break;
		}
	}
}
private void I_OnSizeDataUpdate(Object sender, TickSizeEventArgs e)
{
	this.BeginInvoke( OnSizeUpdateDelegate, sender, e );
}
private void I_OnFill(Object sender, ExecDetailsEventArgs e)
{
	this.BeginInvoke( OnFillUpdateDelegate, sender, e );
}		
/////////  Update form from the main thread ////////////////////////////////////////

private void Client_TickSize(Object sender, TickSizeEventArgs e)
{
	 this.Visible = false;

	 switch ( e.TickType )
	 {
		 case Krs.Ats.IBNet.TickType.BidSize:
			m_BidQty = Convert.ToDouble( e.Size );
			break;
		 case Krs.Ats.IBNet.TickType.AskSize:
			m_AskQty = Convert.ToDouble( e.Size );
			break;
		 case Krs.Ats.IBNet.TickType.LastSize:
			m_LastQty = Convert.ToDouble( e.Size );
			break;
		 default:
			 break;
	 }
     Tick m_Tick = new Tick(DateTime.Now, m_Last, m_LastQty);
     BidAskUpdate(m_Tick);
}

private void Client_TickPrice(Object sender, TickPriceEventArgs e)
{
	switch ( e.TickType )
	{
		case Krs.Ats.IBNet.TickType.BidPrice:
			m_Bid = Convert.ToDouble( e.Price );
			break;
		 case Krs.Ats.IBNet.TickType.AskPrice:
			m_Ask = Convert.ToDouble( e.Price );
			break;
		 case Krs.Ats.IBNet.TickType.LastPrice:
			m_Last = Convert.ToDouble( e.Price );
			break;
		 default:
			 break;
	}
    Tick m_Tick = new Tick(DateTime.Now, m_Last, m_LastQty);
    BidAskUpdate(m_Tick);
 }

///////////////////////////////////////////////////


private void Client_Fill(Object sender, ExecDetailsEventArgs e)
{
	Fill m_Fill = new Fill();
	m_Fill.TradeID = e.Execution.OrderId.ToString();
	m_Fill.BuySell = e.Execution.Side.ToString().Substring(0,1);
	m_Fill.Price = e.Execution.Price;
	m_Fill.Qty = e.Execution.Shares;
	m_Fill.Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
	m_Fill.Symbol = e.Contract.Symbol;

	FillUpdate( m_Fill );
}


public void CancelOrder( String m_ID  )
{
	m_Client.CancelOrder( Convert.ToInt32( m_ID ) );
}

public void EnterMarketOrder( String BS, String m_Qty )
{
	Krs.Ats.IBNet.Order m_Order = new Order();
	m_Order.Action = ( BS == "B" ? ActionSide.Buy : ActionSide.Sell );
	m_Order.OutsideRth = false;
	m_Order.OrderType = OrderType.Market;
	m_Order.TotalQuantity = Convert.ToInt32( m_Qty );
	m_Client.PlaceOrder( NextOrderId, m_Contract, m_Order );

	++NextOrderId;
}

public void EnterLimitOrder( String BS, String m_Qty, String m_Px )
{
	Krs.Ats.IBNet.Order m_Order = new Order();
	m_Order.Action = ( BS == "B" ? ActionSide.Buy : ActionSide.Sell );
	m_Order.OutsideRth = false;
	m_Order.LimitPrice = Convert.ToDecimal( m_Px );
	m_Order.OrderType = OrderType.Limit;
	m_Order.TotalQuantity = Convert.ToInt32( m_Qty );
	m_Client.PlaceOrder( NextOrderId, m_Contract, m_Order );

	//LimitOrderUpdate( new Order( NextOrderId.ToString(), m_Contract.Symbol, m_Px, m_Qty, BS, DateTime.Now.ToString(), null ) );
	++NextOrderId;
}



public void GetHistoricalData()
{

	//System.Void RequestHistoricalData(System.Int32 tickerId, 
	                                 // Krs.Ats.IBNet.Contract contract, 
	                                 // System.DateTime endDateTime, 
	                                 // System.String duration, 
	                                 // Krs.Ats.IBNet.BarSize barSizeSetting, 
	                                 // Krs.Ats.IBNet.HistoricalDataType whatToShow, 
	                                 // System.Int32 useRth)
 
	Krs.Ats.IBNet.BarSize barSizeSetting = Krs.Ats.IBNet.BarSize.FifteenMinutes;
	Krs.Ats.IBNet.HistoricalDataType whatToShow = Krs.Ats.IBNet.HistoricalDataType.Trades;

	DateTime endDateTime = new DateTime(2012, 4, 6, 3, 00, 0);
		
	m_Client.RequestHistoricalData( m_ID, m_Contract, endDateTime, "1 M", barSizeSetting, whatToShow, 0 );

}

private void OnHistoricalDataUpdate(Object o, HistoricalDataEventArgs m_Args)
{

	Debug.WriteLine( m_Args.Open.ToString() );

}

public double get_TickSize()
{
	return 10.0;//m_Contract.Multiplier;
}

public double get_Bid()
{
	return m_Bid;
}

public double get_Ask()
{
	return m_Ask;
}
public double get_BidQty()
{
	return m_BidQty;
}

public double get_AskQty()
{
	return m_AskQty;
}
public double get_Last()
{
	return m_Last;
}
public double get_LastQty()
{
	return m_LastQty;
}


    }
}
