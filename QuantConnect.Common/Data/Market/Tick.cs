/*
* QUANTCONNECT.COM - 
* QC.Algorithm - Base Class for Algorithm.
* Tick base class. Extension of Market Data base class for tick resolution market items.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.IO;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantConnect.Logging;

namespace QuantConnect.Models
{
    /// <summary>
    /// MarketData MarketData Class for MarketData resolution data:
    /// </summary>
    public class Tick : BaseData
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// Type of the Tick: Trade or Quote.
        public TickType TickType = TickType.Trade;

        /// Quantity of the tick sale or quote.
        public int Quantity = 0;

        /// Exchange we are executing on.
        public string Exchange = "";

        /// Sale condition for the tick.
        public string SaleCondition = "";

        /// Bool whether this is a suspicious tick
        public bool Suspicious = false;

        /// Bid Price for Tick - NOTE: We don't currently have quote data
        public decimal BidPrice = 0;

        /// Asking Price for the Tick - NOTE: We don't currently have quote data
        public decimal AskPrice = 0;

        // In Base Class: Last Trade Tick:
        //public decimal Price = 0;

        // In Base Class: Ticker String Symbol of the Asset
        //public string Symbol = "";

        // In Base Class: DateTime of this SnapShot
        //public DateTime Time = new DateTime();




        /******************************************************** 
        * CLASS CONSTRUCTORS
        *********************************************************/
        /// <summary>
        /// Initialize Tick Class
        /// </summary>
        public Tick()
        {
            base.Value = 0;
            base.Time = new DateTime();
            base.DataType = MarketDataType.Tick;
            base.Symbol = "";
            TickType = TickType.Trade;
            Quantity = 0;
            Exchange = "";
            SaleCondition = "";
            Suspicious = false;
        }

        /// <summary>
        /// Cloner Constructor - Clone the original tick into this new tick:
        /// </summary>
        /// <param name="original">Original tick we're cloning</param>
        public Tick(Tick original) {
            base.Symbol = original.Symbol;
            base.Time = new DateTime(original.Time.Ticks);
            this.BidPrice = original.BidPrice;
            this.AskPrice = original.AskPrice;
            this.Exchange = original.Exchange;
            this.SaleCondition = original.SaleCondition;
            this.Quantity = original.Quantity;
            this.Suspicious = original.Suspicious;
        }

        /// <summary>
        /// Simple FX Tick 
        /// </summary>
        /// <param name="time">Full date and time</param>
        /// <param name="symbol">Underlying Asset.</param>
        /// <param name="bid">Bid value</param>
        /// <param name="ask">Ask Value</param>
        public Tick(DateTime time, string symbol, decimal bid, decimal ask)
        {
            base.DataType = MarketDataType.Tick;
            base.Time = time;
            base.Symbol = symbol;
            base.Value = bid + (ask - bid) / 2;
            TickType = TickType.Quote;
            BidPrice = bid;
            AskPrice = ask;
        }


        /// <summary>
        /// FXCM Loader
        /// </summary>
        public Tick(string symbol, string line)
        {
            string[] csv = line.Split(',');
            base.DataType = MarketDataType.Tick;
            base.Symbol = symbol;
            base.Time = DateTime.ParseExact(csv[0], "yyyyMMdd HH:mm:ss.ffff", CultureInfo.InvariantCulture); //// REVERT THIS BACK TO HH
            base.Value = BidPrice + (AskPrice - BidPrice) / 2;
            TickType = TickType.Quote;
            BidPrice = Convert.ToDecimal(csv[1]);
            AskPrice = Convert.ToDecimal(csv[2]);
        }


        /// <summary>
        /// Parse a tick data line from Zip files.
        /// </summary>
        /// <param name="line">CSV Line</param>
        /// <param name="date">Base date for the tick</param>
        /// <param name="config">Subscription configuration object</param>
        /// <param name="datafeed">Datafeed for </param>
        public Tick(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            try
            {
                string[] csv = line.Split(',');

                switch (config.Security)
                { 
                    case SecurityType.Equity:
                        base.Symbol = config.Symbol;
                        base.Time = date.Date.AddMilliseconds(Convert.ToInt64(csv[0]));
                        base.Value = (csv[1].ToDecimal() / 10000m) * config.PriceScaleFactor;
                        base.DataType = MarketDataType.Tick;
                        this.TickType = TickType.Trade;
                        this.Quantity = Convert.ToInt32(csv[2]);
                        if (csv.Length > 3)
                        {
                            this.Exchange = csv[3];
                            this.SaleCondition = csv[4];
                            this.Suspicious = (csv[5] == "1") ? true : false;
                        }
                        break;

                    case SecurityType.Forex:
                        base.Symbol = config.Symbol;
                        TickType = TickType.Quote;
                        Time = DateTime.ParseExact(csv[0], "yyyyMMdd HH:mm:ss.ffff", CultureInfo.InvariantCulture);
                        BidPrice = csv[1].ToDecimal();
                        AskPrice = csv[2].ToDecimal();
                        Value = BidPrice + (AskPrice - BidPrice) / 2;
                        break;
                }
            }
            catch (Exception err)
            {
                Log.Error("Error Generating Tick: " + err.Message);
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Tick Implementation of Reader Method: read a line and convert it to a tick.
        /// </summary>
        /// <param name="datafeed">Source of the datafeed</param>
        /// <param name="config">Configuration object for algorith</param>
        /// <param name="line">Line of the datafeed</param>
        /// <param name="date">Date of this reader request</param>
        /// <returns>New Initialized tick</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            Tick _tick = new Tick();

            //Select the URL source of the data depending on where the system is trading.
            switch (datafeed)
            {
                //Local File System Storage and Backtesting QC Data Store Feed use same files:
                case DataFeedEndpoint.FileSystem:
                case DataFeedEndpoint.Backtesting:
                    //Create a new instance of our tradebar:
                    _tick = new Tick(config, line, date, datafeed);
                    break;
                case DataFeedEndpoint.LiveTrading:
                    break;
                case DataFeedEndpoint.Tradier:
                    break;
            }

            return _tick;
        }


        /// <summary>
        /// Get Source for Custom Data File
        /// >> What source file location would you prefer for each type of usage:
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source request if source spread across multiple files</param>
        /// <param name="datafeed">Source of the datafeed</param>
        /// <returns>String source location of the file</returns>
        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            string source = "";

            switch (datafeed) {  
                //Source location for backtesting. Commonly a dropbox or FTP link
                case DataFeedEndpoint.Backtesting:
                    break;

                //Source location for local testing: Not yet released :) Coming soon.
                case DataFeedEndpoint.FileSystem:
                    break;

                //Source location for live trading: do you have an endpoint for streaming data?
                case DataFeedEndpoint.LiveTrading:
                    break;
            }

            return source;
        }


        /// <summary>
        /// Clone Implementation for Tick Class:
        /// </summary>
        /// <returns></returns>
        public override BaseData Clone()
        {
            return new Tick(this);
        }

    } // End Tick Class:
}
