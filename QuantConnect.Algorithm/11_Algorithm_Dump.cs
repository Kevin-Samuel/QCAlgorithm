using System;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{

    //Your Algorithm Class: Replace the name "MyFirstAlgorithm"
    public class MyFirstAlgorithm : QCAlgorithm
    {
        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            AddSecurity(SecurityType.Equity, "MSFT", Resolution.Minute, true, true);
            SetCash(30000);
            SetStartDate(2013, 4, 1);
            SetEndDate(DateTime.Now.Date.AddDays(-1));
            //Series or Parallel for intraday strategies. NO LONGER AVIABLE, REMOVE?
            SetRunMode(RunMode.Series);
        }

        //Second and Minute Resolution Event Handler:
        public void OnData(TradeBars data)
        {
            //If MSFT stock open at a price greater than 50, sell all MSFT stocks in portfolio
            if (data["MSFT"].Open > 44.5m && Portfolio.HoldStock)
            {
                Order("MSFT", -Portfolio["MSFT"].Quantity);
            }
            if (data["MSFT"].Open < 30 && !Portfolio.HoldStock)
            {
                var quantity = (int)(Portfolio.Cash / data["MSFT"].Open);
                Order("MSFT", quantity);
            }
        }
    }
}