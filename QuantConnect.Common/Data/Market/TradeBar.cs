using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.IO;
using QuantConnect;
using QuantConnect.Logging;

namespace QuantConnect.Models {

    /// <summary>
    /// TradeBar MarketData Class for second and minute resolution data:
    /// </summary>
    public class TradeBar : BaseData
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// Public variable volume:
        public long Volume;

        /// Public variable opening price.
        public decimal Open;

        /// Public variable High Price:
        public decimal High;

        /// Public Variable Low Price
        public decimal Low;

        /// Closing price of the tradebar
        public decimal Close;

        //In Base Class: Alias of Closing:
        //public decimal Price;

        //Symbol of Asset.
        //In Base Class: public string Symbol;

        //In Base Class: DateTime Of this TradeBar
        //public DateTime Time;

        /******************************************************** 
        * CLASS CONSTRUCTORS
        *********************************************************/
        /// <summary>
        /// Default Initializer:
        /// </summary>
        public TradeBar()
        {
            base.Symbol = "";
            base.Time = new DateTime();
            base.Value = 0;
            base.DataType = MarketDataType.TradeBar;
            Open = 0; 
            High = 0;
            Low = 0; 
            Close = 0;
            Volume = 0;
        }

        /// <summary>
        /// Cloner Constructor - Return a new instance with the same values as this original
        /// </summary>
        /// <param name="original"></param>
        public TradeBar(TradeBar original) 
        {
            base.Time = new DateTime(original.Time.Ticks);
            base.Symbol = original.Symbol;
            base.Value = original.Close;
            this.Open = original.Open;
            this.High = original.High;
            this.Low = original.Low;
            this.Close = original.Close;
            this.Volume = original.Volume;
        }

        /// <summary>
        /// Parse a line from the CSV's into our trade bars.
        /// </summary>
        /// <param name="config">QC SecurityType of the tradebar</param>
        /// <param name="baseDate">Base date of this tick</param>
        /// <param name="line">CSV from data files.</param>
        /// <param name="datafeed">Datafeed this csv line is sourced from</param>
        public TradeBar(SubscriptionDataConfig config, string line,  DateTime baseDate, DataFeedEndpoint datafeed = DataFeedEndpoint.Backtesting)
        {
            try
            {
                //Parse the data into a trade bar:
                string[] csv = line.Split(',');
                const decimal scaleFactor = 10000m;
                base.Symbol = config.Symbol;

                switch (config.Security)
                {
                    //Equity File Data Format:
                    case SecurityType.Equity:
                        base.Time = baseDate.Date.AddMilliseconds(Convert.ToInt32(csv[0]));
                        Open = (csv[1].ToDecimal() / scaleFactor) * config.PriceScaleFactor;  //  Convert.ToDecimal(csv[1]) / scaleFactor;
                        High = (csv[2].ToDecimal() / scaleFactor) * config.PriceScaleFactor;  // Using custom "ToDecimal" conversion for speed.
                        Low = (csv[3].ToDecimal() / scaleFactor) * config.PriceScaleFactor;
                        Close = (csv[4].ToDecimal() / scaleFactor) * config.PriceScaleFactor;
                        Volume = Convert.ToInt64(csv[5]);
                        break;

                    //FOREX has a different data file format:
                    case SecurityType.Forex:
                        base.Time = DateTime.ParseExact(csv[0], "yyyyMMdd HH:mm:ss.ffff", CultureInfo.InvariantCulture);
                        Open = csv[1].ToDecimal();
                        High = csv[2].ToDecimal();
                        Low = csv[3].ToDecimal();
                        Close = csv[4].ToDecimal();
                        break;
                }
                base.Value = Close;
            }
            catch (Exception err)
            {
                Log.Error("DataModels: TradeBar(): Error Initializing - " + config.Security + " - " + err.Message + " - " + line);
            }
        }

        /// <summary>
        /// Initialize Trade Bar with OHLC Values:
        /// </summary>
        /// <param name="time">DateTime Timestamp of the bar</param>
        /// <param name="symbol">Market MarketType Symbol</param>
        /// <param name="open">Decimal Opening Price</param>
        /// <param name="high">Decimal High Price of this bar</param>
        /// <param name="low">Decimal Low Price of this bar</param>
        /// <param name="close">Decimal Close price of this bar</param>
        /// <param name="volume">Volume sum over day</param>
        public TradeBar(DateTime time, string symbol, decimal open, decimal high, decimal low, decimal close, long volume)
        {
            base.Time = time;
            base.Symbol = symbol;
            base.Value = close;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Volume = volume;
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// TradeBar Reader: Fetch the data from the QC storage and feed it line by line into the engine.
        /// </summary>
        /// <param name="datafeed">Destination for the this datafeed - live or backtesting</param>
        /// <param name="config">Symbols, Resolution, DataType, </param>
        /// <param name="line">Line from the data file requested</param>
        /// <param name="date">Date of this reader request</param>
        /// <returns>Enumerable iterator for returning each line of the required data.</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed) 
        {
            //Initialize:
            TradeBar _tradeBar = new TradeBar();

            //Select the URL source of the data depending on where the system is trading.
            switch (datafeed)
            {
                //Amazon S3 Backtesting Data:
                case DataFeedEndpoint.Backtesting:
                    //Create a new instance of our tradebar:
                    _tradeBar = new TradeBar(config, line, date, datafeed);
                    break;

                //Localhost Data Source
                case DataFeedEndpoint.FileSystem:
                    //Create a new instance of our tradebar:
                    _tradeBar = new TradeBar(config, line, date, datafeed);
                    break;

                //QuantConnect Live Tick Stream:
                case DataFeedEndpoint.LiveTrading:
                    break;
            }

            //Return initialized TradeBar:
            return _tradeBar;
        }


        /// <summary>
        /// Implement the Clone Method for the TradeBar:
        /// </summary>
        /// <returns></returns>
        public override BaseData Clone()
        {
            //Cleanest way to clone an object is to create a new instance using itself as the arguement.
            return new TradeBar(this);
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

            switch (datafeed)
            {
                //Source location for backtesting. Commonly a dropbox or FTP link
                case DataFeedEndpoint.Backtesting:
                    //E.g.:
                    //source = @"https://www.dropbox.com/";
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



    } // End Trade Bar Class
}
