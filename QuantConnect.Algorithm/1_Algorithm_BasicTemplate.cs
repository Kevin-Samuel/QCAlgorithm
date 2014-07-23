using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{
    /// <summary>
    /// 1.0 BASIC TEMPLATE ALGORITHM
    /// 
    /// This is a bare bones minimum example to run a backtest inside QuantConnect. 
    /// 
    /// Everything inside the QuantConnect.Algorithm project is included in your DLL
    /// 
    /// </summary>
    public class BasicTemplateAlgorithm : QCAlgorithm
    {
        DateTime date = DateTime.Now;

        /// <summary>
        /// Called at the start of your algorithm to setup your requirements:
        /// </summary>
        public override void Initialize()
        {
            //Set the date range you want to run your algorithm:
            SetStartDate(2010, 3, 3);
            SetEndDate(2014, 3, 3);

            //Set the starting cash for your strategy:
            SetCash(100000);

            //Add any stocks you'd like to analyse, and set the resolution:
            // Find more symbols here: http://quantconnect.com/data
            //AddSecurity(SecurityType.Forex, "EURUSD", resolution: Resolution.Tick);
            //AddSecurity(SecurityType.Forex, "NZDUSD", resolution: Resolution.Tick);
            AddSecurity(SecurityType.Equity, "SPY", resolution: Resolution.Minute);
        }


        int count = 0;
        public override void OnEndOfDay()
        {
            Plot("Counter", "Count", (decimal)count++);
        }


        /// <summary>
        /// On receiving new tradebar data it will be passed into this function. The general pattern is:
        /// "public void OnData( CustomType name ) {...s"
        /// </summary>
        /// <param name="data">TradeBars data type synchronized and pushed into this function. The tradebars are grouped in a dictionary.</param>
        public void OnData(TradeBars data)
        {
            if (!Portfolio.Invested)
            {
                Order("SPY", (int)(Portfolio.Cash / data["SPY"].Close));
            }

            //Portfolio.TotalPortfolioValue


            //if (data.ContainsKey("EURUSD"))
            //{
            //    // The portfolio object contains a lot of helper functions and method. Explore here for more information:
            //    // QuantConnect.Common\Securities\SecurityPortfolioManager.cs
            //    if (!Portfolio.Invested)
            //    {
            //        //The "SPY" tradebar inside the price data dictionary.
            //        // you can also access .High, .Low and .Open
            //        decimal price = data["EURUSD"][0].Value;

            //        // Send an order, you need the market data for the order requested.
            //        Order("EURUSD", (int)(Portfolio.Cash / price));

            //        //We override the console command and pipe the messages to the browser.
            //        Console.WriteLine("Buying EURUSD: " + price);
            //    }

            //    if (date.Date != data["EURUSD"][0].Time.Date)
            //    {
            //        Debug("Date Changed: " + date.Date.ToString("d MMM yyyy"));
            //        date = Time;
            //    }
            //}
        }
    }
}