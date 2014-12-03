using System;
using System.Collections;
using System.Collections.Generic;

namespace QuantConnect
{
    using QuantConnect.Securities;
    using QuantConnect.Models;

    public partial class TestOnEndOfDay : QCAlgorithm, IAlgorithm
    {
        string symbol = "SPY";

        public override void Initialize()
        {
            SetStartDate(2013, 1, 1);
            SetEndDate(2014, 1, 1);
            SetCash(30000);
            AddSecurity(SecurityType.Equity, symbol, Resolution.Minute);
        }

        public override void OnTradeBar(Dictionary<string, TradeBar> data)
        {
            if (Portfolio.HoldStock == false)
            {
                Order(symbol, 50);
            }
        }

        public override void OnEndOfDay()
        {
            Debug(Time.Date.ToShortDateString() + " EOD Message.");
        }
    }
}