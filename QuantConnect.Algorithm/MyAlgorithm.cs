/*
* QUANTCONNECT.COM - 
* QC.Algorithm - Example starting point for a locally compiled user algorithm
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;


using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Example user algorithm class - use this as a base point for your strategy.
    /// </summary>
    public class BasicTemplateAlgorithm : QCAlgorithm, IAlgorithm
    {
        string symbol = "IBM";
        DateTime startDate = new DateTime(2013, 1, 1);

        //Initialize the data and resolution you require for your strategy:
        public override void Initialize()
        {
            //Initialize the start, end dates for simulation; cash and data required.
            SetStartDate(startDate);
            SetEndDate(DateTime.Now.Date.AddDays(-1));
            SetCash(78000); //Starting Cash in USD.
            AddSecurity(SecurityType.Equity, symbol, Resolution.Minute); //Minute,Second - Tick
            SetRunMode(RunMode.Series); //Series or Parallel for intraday strategies.
        }

        //Handle TradeBar Events: a TradeBar occurs on every time-interval
        public override void OnTradeBar(Dictionary<string, TradeBar> data)
        {
            if (!Portfolio.HoldStock)
            {
                Order(symbol, 50);
                Debug("Debug Purchased IBM 15 : " + (new Random()).NextDouble());
                Log("Log Purchased IBM 15 " + (new Random()).NextDouble());
            }
        }
    }

} // End QC Namespace
