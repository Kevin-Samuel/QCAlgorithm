using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{
    /// <summary>
    /// 2.0 EXPONENTIAL MOVING AVERAGE TEMPLATE ALGORITHM
    /// 
    /// Minimalise example of the popular trading strategy - exponential moving average cross.
    /// 
    /// The code performs a rolling 200 minute - 50 minute rolling average and trades when the direction changes.
    /// 
    /// </summary>
    public class ExponentialMovingAverageAlgorithm : QCAlgorithm
    {
        //Algorithm Common Variables:
        decimal tolerance = 0m; //0.1% safety margin in prices to avoid bouncing.
        string symbol = "SPY";
        DateTime sampledToday = DateTime.Now;
        ExponentialMovingAverage emaShort;
        ExponentialMovingAverage emaLong;

        /// <summary>
        /// Called at the start of your algorithm to setup your requirements:
        /// </summary>
        public override void Initialize()
        {
            //Set the date range you want to run your algorithm:
            SetStartDate(1998, 1, 1);
            SetEndDate(DateTime.Today.AddDays(-1));

            //Set the starting cash for your strategy:
            SetCash(100000);

            //Add any stocks you'd like to analyse, and set the resolution:
            // Find more symbols here: http://quantconnect.com/data
            AddSecurity(SecurityType.Equity, "SPY", resolution: Resolution.Minute);

            //EMA Average Trackers:
            emaShort = new ExponentialMovingAverage(10);
            emaLong = new ExponentialMovingAverage(50);
        }


        /// <summary>
        /// New Trade Bar Data Passed into TradeBar Event Handler:
        /// This method checks if the EMA's have crossed by at least a minimum magnitude. 
        /// Having this minimum cross filters out noisy trade signals.
        /// </summary>
        /// <param name="data">TradeBars data type synchronized and pushed into this function. The tradebars are grouped in a dictionary.</param>
        public void OnData(TradeBars data)
        {
            //One data point per day: 
            if (sampledToday.Date == data[symbol].Time.Date) return;

            //Only take one data point per day (opening price)
            var price = Securities[symbol].Close;
            sampledToday = data[symbol].Time;

            //Wait until EMA's are ready:
            if (!emaShort.Ready || !emaLong.Ready) return;

            //Get fresh cash balance: Set purchase quantity to equivalent 10% of portfolio.
            var holdings = Portfolio[symbol].Quantity;
            var quantity = Convert.ToInt32(Portfolio.Cash / price);

            if (holdings > 0 || holdings == 0)
            {
                //If we're long, or flat: check if EMA crossed negative: and crossed outside our safety margin:
                if ((emaShort.EMA * (1 + tolerance)) < emaLong.EMA)
                {
                    //Now go short: Short-EMA signals a negative turn: reverse holdings
                    Order(symbol, -(holdings + quantity));
                    Log(Time.ToShortDateString() + " > Go Short > Holdings: " + holdings.ToString() + " Quantity:" + quantity.ToString() + " Samples: " + emaShort.Samples);
                }
            }
            else if (holdings < 0 || holdings == 0)
            {
                //If we're short, or flat: check if EMA crossed positive: and crossed outside our safety margin:
                if ((emaShort.EMA * (1 - tolerance)) > emaLong.EMA)
                {
                    //Now go long: Short-EMA crossed above long-EMA by sufficient margin
                    Order(symbol, Math.Abs(holdings) + quantity);
                    Log(Time.ToShortDateString() + "> Go Long >  Holdings: " + holdings.ToString() + " Quantity:" + quantity.ToString() + " Samples: " + emaShort.Samples);
                }
            }
        }
    }

    /*
    *   >> EMA Indicator: To use this indicator: 
    *
    *   1. Create an instance of it in your algorithm:
    *   ExponentialMovingAverage ema10 = new ExponentialMovingAverage(10);
    *   
    *   2. Push in data with AddSample:
    *   ema = ema10.AddSample(data['spy'].Close);
    *
    *   3. If you're sensitive to the precise EMA values you push wait until the indicator is Ready.
    */
    public class ExponentialMovingAverage 
    {
        private int _period, _samples;
		private decimal _ema, _ema_const;
		
		//Current value of the EMA.
		public decimal EMA {
			get{ return _ema;}
		}
		
		//Track the number of samples:
		public int Samples {
		    get { return _samples; }
		}
		
		//We've got sufficient data samples to know its the EMA.
		public bool Ready {
		    get { return _samples >= _period; }
		}
		
		/******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm
        /// </summary>
        public ExponentialMovingAverage(int period) {
            _period = period;
            _samples = 0;
            _ema = 0;
            _ema_const = (decimal) 2/(_period +1);
        }
        
	   /******************************************************** 
            * CLASS METHODS
        *********************************************************/
		/// <summary>
		/// Calculate the exponential moving average 
        /// </summary>		
        public decimal AddSample(decimal quote)
        {
            _samples++;
		  
            if (_samples == 1) {
                _ema = quote;
            } else {
                _ema = (1-_ema_const)*_ema + _ema_const*quote;
            }
            
		    return _ema;
        }
    } 
}