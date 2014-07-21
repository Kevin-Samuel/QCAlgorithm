using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{
    /// <summary>
    /// 3.0 CUSTOM DATA SOURCE: USE YOUR OWN MARKET DATA (OPTIONS, FOREX, FUTURES, DERIVATIVES etc).
    /// 
    /// The new QuantConnect Lean Backtesting Engine is incredibly flexible and allows you to define your own data source. 
    /// 
    /// This includes any data source which has a TIME and VALUE. These are the *only* requirements. To demonstrate this we're loading
    /// in "Weather" data. This by itself isn't special, the cool part is next:
    /// 
    /// We load the "Weather" data as a tradable security we're calling "NYCTEMP".
    /// 
    /// </summary>
    public class CustomDataSourceAlgorithm : QCAlgorithm
    {
        public override void Initialize()
        {
            //Weather data we have is within these days:
            SetStartDate(2013, 1, 1);
            SetEndDate(2014, 5, 30);

            //Set the cash for the strategy:
            SetCash(100000);

            //Define the symbol and "type" of our generic data:
            AddData<Weather>("NYCTEMP");
        }

        /// <summary>
        /// Event Handler for Weather Data Events: These weather objects are created from our 
        /// "Weather" type below and fired into this event handler.
        /// </summary>
        /// <param name="data">One(1) Weather Object, streamed into our algorithm synchronised in time with our other data streams</param>
        public void OnData(Weather data)
        {
            //If we don't have any weather "SHARES" -- invest"
            if (!Portfolio.Invested)
            {
                //Weather used as a tradable asset, like stocks, futures etc. 
                Order("NYCTEMP", (int)(Portfolio.Cash / data.MeanC));

                Console.WriteLine("Buying Weather 'Shares': $" + data.MeanC);
            }
        }
    }


    /// <summary>
    /// Our Custom Data And Security. The Weather Class. 
    /// In C# a class is a type of data, like a double, int or string..
    /// </summary>
    public class Weather : BaseData
    {
        // Public variables for weather: Maximum Celcius in Zip 10065
        public decimal MaxC;
        //Mean Celcius Temperature in Zip 10065
        public decimal MeanC;
        //Minimum Celcius Temperature in Zip 10065
        public decimal MinC;

        /// <summary>
        /// 1. WE NEED A DEFAULT CONSTRUCTOR: 
        /// We search for a default constructor so please provide one here. It won't be used for data, just to generate the "Factory".
        /// </summary>
        public Weather()
        {
            this.MeanC = 0; this.MinC = 0;
            this.MaxC = 0; this.Symbol = "NYCTEMP";
        }

        /// <summary>
        /// 2. RETURN THE STRING URL SOURCE LOCATION FOR YOUR DATA:
        /// This is a powerful and dynamic select source file method. If you have a large dataset, 10+mb we recommend you break it into smaller files. E.g. One zip per year.
        /// We can accept raw text or ZIP files. We read the file extension to determine if it is a zip file.
        /// </summary>
        /// <param name="config">Subscription data, symbol name, data type</param>
        /// <param name="date">Current date we're requesting. This allows you to break up the data source into daily files.</param>
        /// <param name="datafeed">Datafeed type: Backtesting or the Live data broker who will provide live data. You can specify a different source for live trading! </param>
        /// <returns>string URL end point.</returns>
        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            switch (datafeed) 
            { 
                //Backtesting Data Source: Example of a data source which varies by day (commented out)
                default:
                case DataFeedEndpoint.Backtesting:
                    //return "http://my-ftp-server.com/futures-data-" + date.ToString("Ymd") + ".zip";

                    // OR simply return a fixed small data file. Large files will slow down your backtest
                    return "https://www.dropbox.com/s/txgqzv2vp5lzpqc/10065.csv?dl=1";

                case DataFeedEndpoint.LiveTrading:
                    //Alternative live socket data source for live trading (soon)/
                    return "https://www.weather.com:2092";
            }
        }

        /// <summary>
        /// 3. READER METHOD: Read 1 line from data source and convert it into Object.
        /// Each line of the CSV File is presented in here. The backend downloads your file, loads it into memory and then line by line
        /// feeds it into your algorithm
        /// </summary>
        /// <param name="line">string line from the data source file submitted above</param>
        /// <param name="config">Subscription data, symbol name, data type</param>
        /// <param name="date">Current date we're requesting. This allows you to break up the data source into daily files.</param>
        /// <param name="datafeed">Datafeed type - Backtesting or LiveTrading</param>
        /// <returns>New Weather Object which extends BaseData.</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            //New weather object
            Weather point = new Weather();

            try {
                //Example File Format:
                //EST,	        Max TemperatureC,	Mean TemperatureC,	Min TemperatureC
                //1/1/2013,	    4,	                1,	                -3,
                string[] data = line.Split(',');
                point.Time = DateTime.Parse(data[0]);
                point.MaxC = Convert.ToDecimal(data[1]);
                point.MeanC = Convert.ToDecimal(data[2]);
                point.MinC = Convert.ToDecimal(data[3]);
                point.Value = MeanC;
                point.Symbol = "NYCTEMP";
            } catch { /* Do nothing, skip first title row */ }

            return point;
        }

        /// <summary>
        /// 4. CLONE METHOD: Deep clone a weather object: only required for fillforward, not applicable here:
        /// </summary>
        /// <returns>New 'deep' exact copy of this weather object</returns>
        public override BaseData Clone()
        {
            return new Weather();
        }
    }
}