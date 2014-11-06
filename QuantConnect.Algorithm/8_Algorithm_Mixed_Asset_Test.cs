using System;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;
using System.Globalization;

namespace QuantConnect
{
    // Name your algorithm class anything, as long as it inherits QCAlgorithm
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        private decimal _vix = 0;
        private decimal _scale = 0;
        private DateTime _lastRebalance = new DateTime();

        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            SetStartDate(2000, 1, 1);
            SetEndDate(2014, 10, 31);
            SetCash(250000);
            AddSecurity(SecurityType.Equity, "SPY", Resolution.Minute);
            AddData<VIX>("VIX");
        }

        // Data Event Handler: New data arrives here. "TradeBars" type is a dictionary of strings so you can access it by symbol.
        public void OnData(TradeBars data)
        {
            //Rebalance once per day:
            if (Time.Date > _lastRebalance.Date.AddDays(5))
            {
                if (_scale > 0)
                {
                    SetHoldings("SPY", _scale, false, "Scale: " + _scale);
                }
                else
                {
                    Liquidate();
                }
                _lastRebalance = Time;
            }
        }

        // 
        public void OnData(VIX vix)
        {
            _vix = vix.Close;
            _scale = 1 - (_vix / 30m);
            if (_scale < 0) _scale = 0;
        }
    }


    /// <summary>
    /// Custom imported data -- VIX indicator:
    /// </summary>
    public class VIX : BaseData
    {
        public decimal Open = 0;
        public decimal High = 0;
        public decimal Low = 0;
        public decimal Close = 0;

        public VIX()
        { this.Symbol = "VIX"; }

        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            return "https://www.quandl.com/api/v1/datasets/YAHOO/INDEX_VIX.csv?trim_start=2000-01-01&trim_end=2014-10-31&sort_order=asc&exclude_headers=true";
        }
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            VIX fear = new VIX();
            //try
            //{
            //Date	Open	High	Low	Close	Volume	Adjusted Close
            //10/27/2014	17.24	17.87	16	16.04	0	16.04
            string[] data = line.Split(',');
            fear.Time = DateTime.ParseExact(data[0], "yyyy-MM-dd", CultureInfo.InvariantCulture);
            fear.Open = Convert.ToDecimal(data[1]); fear.High = Convert.ToDecimal(data[2]);
            fear.Low = Convert.ToDecimal(data[3]); fear.Close = Convert.ToDecimal(data[4]);
            fear.Symbol = "VIX"; fear.Value = fear.Close;
            //}
            //catch 
            //{ }
            return fear;
        }
    }
}