/*
 * QUANTCONNECT.COM - 
 * GLobal Enums
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using Newtonsoft.Json;

//QuantConnect Project Libraries:
using QuantConnect.Models;
using QuantConnect.Logging;

namespace QuantConnect 
{
    /******************************************************** 
    * GLOBAL CONST
    *********************************************************/
    /// <summary>
    /// ShortCut Date Format Strings:
    /// </summary>
    public static class DateFormat 
    {
        /// Year-Month-Date 6 Character Date Representation
        public static string SixCharacter = "yyMMdd";
        /// YYYY-MM-DD Eight Character Date Representation
        public static string EightCharacter = "yyyyMMdd";
        /// JSON Format Date Representation
        public static string JsonFormat = "yyyy-MM-ddThh:mm:ss";
        /// MySQL Format Date Representation
        public const string DB = "yyyy-MM-dd HH:mm:ss";
        /// QuantConnect UX Date Representation
        public const string UI = "yyyyMMdd HH:mm:ss";
        /// EXT Web Date Representation
        public const string EXT = "yyyy-MM-dd HH:mm:ss";
    }


    /******************************************************** 
    * GLOBAL STRUCT DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Single Parent Chart Object for Custom Charting
    /// </summary>
    [JsonObjectAttribute]
    public class Chart {

        /// Name of the Chart:
        public string Name = "";

        /// Type of the Chart, Overlayed or Stacked.
        public ChartType ChartType = ChartType.Overlay;

        /// List of Series Objects for this Chart:
        public Dictionary<string, Series> Series = new Dictionary<string,Series>();

        /// <summary>
        /// Default constructor for chart:
        /// </summary>
        public Chart() { }

        /// <summary>
        /// Chart Constructor:
        /// </summary>
        /// <param name="name">Name of the Chart</param>
        /// <param name="type"> Type of the chart</param>
        public Chart(string name, ChartType type = QuantConnect.ChartType.Overlay) 
        {
            this.Name = name;
            this.Series = new Dictionary<string, Series>();
            this.ChartType = type;
        }

        /// <summary>
        /// Add a reference to this chart series:
        /// </summary>
        /// <param name="series">Chart series class object</param>
        public void AddSeries(Series series) 
        {
            //If we dont already have this series, add to the chrt:
            if (!Series.ContainsKey(series.Name))
            {
                this.Series.Add(series.Name, series);
            }
            else 
            {
                throw new Exception("Chart.AddSeries(): Chart series name already exists");
            }
        }

        /// <summary>
        /// Fetch the updates of the chart, and save the index position.
        /// </summary>
        /// <returns></returns>
        public Chart GetUpdates() 
        {
            Chart _copy = new Chart(Name, ChartType);
            try
            {   
                foreach (Series _series in Series.Values)
                {
                    _copy.AddSeries(_series.GetUpdates());
                }
            }
            catch (Exception err) {
                Log.Error("Chart.GetUpdates(): " + err.Message);
            }
            return _copy;
        }
    }


    /// <summary>
    /// Chart Series Object - Series data and properties for a chart:
    /// </summary>
    [JsonObjectAttribute]
    public class Series
    {
        /// Name of the Series:
        public string Name = "";

        /// Values for the series plot:
        public List<ChartPoint> Values = new List<ChartPoint>();

        /// Chart type for the series:
        public SeriesType SeriesType = SeriesType.Line;

        /// Get the index of the last fetch update request to only retrieve the "delta" of the previous request.
        private int updatePosition = 0;

        /// <summary>
        /// Default constructor for chart series
        /// </summary>
        public Series() { }

        /// <summary>
        /// Constructor method for Chart Series
        /// </summary>
        /// <param name="name">Name of the chart series</param>
        /// <param name="type">Type of the chart series</param>
        public Series(string name, SeriesType type = SeriesType.Line) 
        {
            this.Name = name;
            this.Values = new List<ChartPoint>();
            this.SeriesType = type;
        }

        /// <summary>
        /// Add a new point to this series:
        /// </summary>
        /// <param name="time">Time of the chart point</param>
        /// <param name="value">Value of the chart point</param>
        public void AddPoint(DateTime time, decimal value) 
        {
            //Round off the chart values to significant figures:
            double v = ((double)value).RoundToSignificantDigits(5);

            if (Values.Count < 4000)
            {
                Values.Add(new ChartPoint(time, value));
            } else { 
                //Cannot add more than 4000 points per chart
            }
        }


        /// <summary>
        /// Get the updates since the last call to this function.
        /// </summary>
        /// <returns>List of the updates from the series</returns>
        public Series GetUpdates() 
        {
            Series _copy = new Series(Name, SeriesType);
            try
            {
                //Add the updates since the last 
                for (int i = updatePosition; i < this.Values.Count; i++)
                {
                    _copy.Values.Add(this.Values[i]);
                }
                //Shuffle the update point to now:
                updatePosition = this.Values.Count;
            }
            catch (Exception err) {
                Log.Error("Series.GetUpdates(): " + err.Message);
            }
            return _copy;
        }
    }


    /// <summary>
    /// Single Chart Point Value Type for QCAlgorithm.Plot();
    /// </summary>
    [JsonObjectAttribute]
    public struct ChartPoint
    {
        /// Time of this chart point:
        public long x;

        /// Value of this chart point:
        public decimal y;

        ///Constructor for datetime-value arguements:
        public ChartPoint(DateTime time, decimal value) 
        {
            this.x = Convert.ToInt64(Time.DateTimeToUnixTimeStamp(time));
            this.y = value;
        }

        ///Cloner Constructor:
        public ChartPoint(ChartPoint point) 
        {
            this.x = point.x;
            this.y = point.y;
        }
    }


    /// <summary>
    /// Available types of charts
    /// </summary>
    public enum SeriesType 
    { 
        /// Line Plot for Value Types
        Line,
        /// Scatter Plot for Chart Distinct Types
        Scatter,
        /// Charts
        Candle
    }

    /// <summary>
    /// Type of chart - should we draw the series as overlayed or stacked
    /// </summary>
    public enum ChartType 
    { 
        /// Overlayed stacked
        Overlay,
        /// Stacked series on top of each other.
        Stacked
    }

    /******************************************************** 
    * GLOBAL ENUMS DEFINITIONS
    *********************************************************/
    
    /// <summary>
    /// Types of Run Mode: Series, Parallel or Auto.
    /// </summary>
    public enum RunMode 
    { 
        /// Automatically detect the runmode of the algorithm: series for minute data, parallel for second-tick
        Automatic,
        /// Series runmode for the algorithm
        Series,
        /// Parallel runmode for the algorithm
        Parallel
    }


    /// <summary>
    /// Added multilanguage support.
    /// </summary>
    public enum Language 
    { 
        /// C# Language Project
        CSharp,
        /// Java Language Project
        Java,
        /// Python Language Project
        Python
    }


    /// <summary>
    /// Some features / performance depend on users plan
    /// </summary>
    public enum UserPlan 
    {
        /// Free User 
        Free,
        /// Hobbyist User 
        Hobbyist,
        /// Professional User 
        Professional,
        /// Team Based Plan
        Team,
        /// Institutional User 
        Institutional
    }


    /// <summary>
    /// Type of Tradable Security / Underlying Asset
    /// </summary>
    public enum SecurityType 
    {
        /// Base class for all security types: 
        Base,
        /// US Equity Security
        Equity,
        /// Option Security Type
        Option,
        /// Commodity Security Type
        Commodity,
        /// FOREX Security 
        Forex,
        /// Future Security Type
        Future
    }

    /// <summary>
    /// Market Data Type Definition:
    /// </summary>
    public enum MarketDataType 
    {
        /// Base market data type
        Base,
        /// TradeBar market data type
        TradeBar,
        /// Tick Market Data Type
        Tick
    }

    /// <summary>
    /// Which data feed are we using.
    /// </summary>
    public enum DataFeedEndpoint 
    { 
        /// Backtesting Datafeed Endpoint
        Backtesting,
        /// Loading files off the local system
        FileSystem,
        /// Getting datafeed from a QC-Live-Cloud
        LiveTrading,
        /// Tradier Supplied Free Data Feed 
        Tradier
    }

    /// <summary>
    /// Destination of Algorithm Node Result, Progress Messages
    /// </summary>
    public enum ResultHandlerEndPoint
    {
        /// Send Results to the Backtesting Web Application
        Backtesting,
        /// Send the Results to the Local Console
        Console,
        /// Send Results to the Live Web Application
        LiveTrading
    }

    /// <summary>
    /// Setup Handler - Configure algorithm internal state for backtesting, console or live trading.
    /// </summary>
    public enum SetupHandlerEndPoint
    {
        /// Configure algorithm+job for backtesting:
        Backtesting,
        /// Configure algorithm+job for the console:
        Console,
        /// Paper trading algorithm+job internal state configuration
        PaperTrading,
        /// Tradier Setup Handler
        Tradier
    }

    /// <summary>
    /// Destination of Transaction Models
    /// </summary>
    public enum TransactionHandlerEndpoint 
    { 
        /// Use Backtesting Models to Process Transactions
        Backtesting,
        /// Use Paper Trading Model to Process Transactions
        PaperTrading,
        /// Use Tradier to Process Transactions
        Tradier,
        /// Use Interactive Brokers to Process Transactions
        InteractiveBrokers,
        /// Use FXCM to Process Transactions
        FXCM
    }

    /// <summary>
    /// Data types available from spryware decoding
    /// </summary>
    public enum TickType 
    {
        /// Trade Type Tick -  QC Supports Full Trade-Quote Ticks but we only have Trade Data.
        Trade,
        /// Quote Type Tick - QC Supports Full Trade-Quote Ticks but we only have Trade Data.
        Quote
    }

    /// <summary>
    /// Resolution of data requested:
    /// </summary>
    public enum Resolution 
    {
        /// Tick Resolution
        Tick,
        /// Second Resolution
        Second,
        /// Minute Resolution
        Minute,
        /// Hour Resolution
        Hour,
        /// Daily Resolution
        Daily
    }

    /// <summary>
    /// State of the Instance:
    /// </summary>
    public enum State 
    {
        /// Server Controls - Is the Instance Busy?
        Busy,
        /// Server Controls - Is the Instance Idle?
        Idle
    }

    /// <summary>
    /// Database Model File status
    /// </summary>
    public enum FileStatus 
    {
        /// File currently active
        Active,
        /// File Deleted.
        Deleted
    }


    /// <summary>
    /// If the backtest has been deleted
    /// </summary>
    public enum BacktestStatus
    {
        /// Active
        Active,
        /// Deleted/Cancelled.
        Deleted
    }

    /// <summary>
    /// Use standard HTTP Status Codes for communication between servers:
    /// </summary>
    public enum ResponseCode 
    {
        /// 200 Server OK
        OK = 200,
        /// 401 Unauthorized Request
        Unauthorized = 401,
        /// 404 Not Found
        NotFound = 404,
        /// 501 Request Not Implemented
        NotImplemented = 501,
        /// 502 Malformed Request
        MalformedRequest = 502,
        /// 503 Server Compiler Error
        CompilerError = 503
    }


    /// <summary>
    /// enum Period - Enum of all the analysis periods, AS integers. Reference "Period" Array to access the values
    /// </summary>
    public enum Period 
    {
        /// Period Short Codes - 10 
        TenSeconds = 10,
        /// Period Short Codes - 30 Second 
        ThirtySeconds = 30,
        /// Period Short Codes - 60 Second 
        OneMinute = 60,
        /// Period Short Codes - 120 Second 
        TwoMinutes = 120,
        /// Period Short Codes - 180 Second 
        ThreeMinutes = 180,
        /// Period Short Codes - 300 Second 
        FiveMinutes = 300,
        /// Period Short Codes - 600 Second 
        TenMinutes = 600,
        /// Period Short Codes - 900 Second 
        FifteenMinutes = 900,
        /// Period Short Codes - 1200 Second 
        TwentyMinutes = 1200,
        /// Period Short Codes - 1800 Second 
        ThirtyMinutes = 1800,
        /// Period Short Codes - 3600 Second 
        OneHour = 3600,
        /// Period Short Codes - 7200 Second 
        TwoHours = 7200,
        /// Period Short Codes - 14400 Second 
        FourHours = 14400,
        /// Period Short Codes - 21600 Second 
        SixHours = 21600
    }


    /******************************************************** 
    * GLOBAL MARKETS
    *********************************************************/
    /// <summary>
    /// Global Market Short Codes and their full versions: (used in tick objects)
    /// </summary>
    public static class MarketCodes 
    {
        /// US Market Codes
        public static Dictionary<string, string> US = new Dictionary<string, string>() 
        {
            {"A", "American Stock Exchange"},
            {"B", "Boston Stock Exchange"},
            {"C", "National Stock Exchange"},
            {"D", "FINRA ADF"},
            {"I", "International Securities Exchange"},
            {"J", "Direct Edge A"},
            {"K", "Direct Edge X"},
            {"M", "Chicago Stock Exchange"},
            {"N", "New York Stock Exchange"},
            {"P", "Nyse Arca Exchange"},
            {"Q", "NASDAQ OMX"},
            {"T", "NASDAQ OMX"},
            {"U", "OTC Bulletin Board"},
            {"u", "Over-the-Counter trade in Non-NASDAQ issue"},
            {"W", "Chicago Board Options Exchange"},
            {"X", "Philadelphia Stock Exchange"},
            {"Y", "BATS Y-Exchange, Inc"},
            {"Z", "BATS Exchange, Inc"}
        };

        /// Canada Market Short Codes:
        public static Dictionary<string, string> Canada = new Dictionary<string, string>() 
        {
            {"T", "Toronto"},
            {"V", "Venture"}
        };
    }


    /// <summary>
    /// US Public Holidays - Not Tradeable:
    /// </summary>
    public static class USHoliday 
    {
        /// <summary>
        /// Public Holidays
        /// </summary>
        public static List<DateTime> Dates = new List<DateTime>() 
        { 
            /* New Years Day*/
            new DateTime(1998, 01, 01),
            new DateTime(1999, 01, 01),
            new DateTime(2001, 01, 01),
            new DateTime(2002, 01, 01),
            new DateTime(2003, 01, 01),
            new DateTime(2004, 01, 01),
            new DateTime(2006, 01, 02),
            new DateTime(2007, 01, 01),
            new DateTime(2008, 01, 01),
            new DateTime(2009, 01, 01),
            new DateTime(2010, 01, 01),
            new DateTime(2011, 01, 01),
            new DateTime(2012, 01, 02),
            new DateTime(2013, 01, 01),
            new DateTime(2014, 01, 01),
            
            /* Day of Mouring */
            new DateTime(2007, 01, 02),

            /* World Trade Center */
            new DateTime(2001, 09, 11),
            new DateTime(2001, 09, 12),
            new DateTime(2001, 09, 13),
            new DateTime(2001, 09, 14),

            /* Regan Funeral */
            new DateTime(2004, 06, 11),

            /* Hurricane Sandy */
            new DateTime(2012, 10, 29),
            new DateTime(2012, 10, 30),

            /* Martin Luther King Jnr Day*/
            new DateTime(1998, 01, 19),
            new DateTime(1999, 01, 18),
            new DateTime(2000, 01, 17),
            new DateTime(2001, 01, 15),
            new DateTime(2002, 01, 21),
            new DateTime(2003, 01, 20),
            new DateTime(2004, 01, 19),
            new DateTime(2005, 01, 17),
            new DateTime(2006, 01, 16),
            new DateTime(2007, 01, 15),
            new DateTime(2008, 01, 21),
            new DateTime(2009, 01, 19),
            new DateTime(2010, 01, 18),
            new DateTime(2011, 01, 17),
            new DateTime(2012, 01, 16),
            new DateTime(2013, 01, 21),
            new DateTime(2014, 01, 20),

            /* Washington / Presidents Day */
            new DateTime(1998, 02, 16),
            new DateTime(1999, 02, 15),
            new DateTime(2000, 02, 21),
            new DateTime(2001, 02, 19),
            new DateTime(2002, 02, 18),
            new DateTime(2003, 02, 17),
            new DateTime(2004, 02, 16),
            new DateTime(2005, 02, 21),
            new DateTime(2006, 02, 20),
            new DateTime(2007, 02, 19),
            new DateTime(2008, 02, 18),
            new DateTime(2009, 02, 16),
            new DateTime(2010, 02, 15),
            new DateTime(2011, 02, 21),
            new DateTime(2012, 02, 20),
            new DateTime(2013, 02, 18),
            new DateTime(2014, 02, 17),

            /* Good Friday */
            new DateTime(1998, 04, 10),
            new DateTime(1999, 04, 02),
            new DateTime(2000, 04, 21),
            new DateTime(2001, 04, 13),
            new DateTime(2002, 03, 29),
            new DateTime(2003, 04, 18),
            new DateTime(2004, 04, 09),
            new DateTime(2005, 03, 25),
            new DateTime(2006, 04, 14),
            new DateTime(2007, 04, 06),
            new DateTime(2008, 03, 21),
            new DateTime(2009, 04, 10),
            new DateTime(2010, 04, 02),
            new DateTime(2011, 04, 22),
            new DateTime(2012, 04, 06),
            new DateTime(2013, 03, 29),
            new DateTime(2014, 04, 18),

            /* Memorial Day */
            new DateTime(1998, 05, 25),
            new DateTime(1999, 05, 31),
            new DateTime(2000, 05, 29),
            new DateTime(2001, 05, 28),
            new DateTime(2002, 05, 27),
            new DateTime(2003, 05, 26),
            new DateTime(2004, 05, 31),
            new DateTime(2005, 05, 30),
            new DateTime(2006, 05, 29),
            new DateTime(2007, 05, 28),
            new DateTime(2008, 05, 26),
            new DateTime(2009, 05, 25),
            new DateTime(2010, 05, 31),
            new DateTime(2011, 05, 30),
            new DateTime(2012, 05, 28),
            new DateTime(2013, 05, 27),
            new DateTime(2014, 05, 26),

            /* Independence Day */
            new DateTime(1998, 07, 03),
            new DateTime(1999, 07, 05),
            new DateTime(2000, 07, 04),
            new DateTime(2001, 07, 04),
            new DateTime(2002, 07, 04),
            new DateTime(2003, 07, 04),
            new DateTime(2004, 07, 05),
            new DateTime(2005, 07, 04),
            new DateTime(2006, 07, 04),
            new DateTime(2007, 07, 04),
            new DateTime(2008, 07, 04),
            new DateTime(2009, 07, 03),
            new DateTime(2010, 07, 05),
            new DateTime(2011, 07, 04),
            new DateTime(2012, 07, 04),
            new DateTime(2013, 07, 04),
            new DateTime(2014, 07, 04),
            new DateTime(2014, 07, 04),

            /* Labour Day */
            new DateTime(1998, 09, 07),
            new DateTime(1999, 09, 06),
            new DateTime(2000, 09, 04),
            new DateTime(2001, 09, 03),
            new DateTime(2002, 09, 02),
            new DateTime(2003, 09, 01),
            new DateTime(2004, 09, 06),
            new DateTime(2005, 09, 05),
            new DateTime(2006, 09, 04),
            new DateTime(2007, 09, 03),
            new DateTime(2008, 09, 01),
            new DateTime(2009, 09, 07),
            new DateTime(2010, 09, 06),
            new DateTime(2011, 09, 05),
            new DateTime(2012, 09, 03),
            new DateTime(2013, 09, 02),
            new DateTime(2014, 09, 01),

            /* Thanksgiving Day */
            new DateTime(1998, 11, 26),
            new DateTime(1999, 11, 25),
            new DateTime(2000, 11, 23),
            new DateTime(2001, 11, 22),
            new DateTime(2002, 11, 28),
            new DateTime(2003, 11, 27),
            new DateTime(2004, 11, 25),
            new DateTime(2005, 11, 24),
            new DateTime(2006, 11, 23),
            new DateTime(2007, 11, 22),
            new DateTime(2008, 11, 27),
            new DateTime(2009, 11, 26),
            new DateTime(2010, 11, 25),
            new DateTime(2011, 11, 24),
            new DateTime(2012, 11, 22),
            new DateTime(2013, 11, 28),
            new DateTime(2014, 11, 27),

            /* Christmas 1998-2014 */
            new DateTime(1998, 12, 25),
            new DateTime(1999, 12, 24),
            new DateTime(2000, 12, 25),
            new DateTime(2001, 12, 25),
            new DateTime(2002, 12, 25),
            new DateTime(2003, 12, 25),
            new DateTime(2004, 12, 24),
            new DateTime(2005, 12, 26),
            new DateTime(2006, 12, 25),
            new DateTime(2007, 12, 25),
            new DateTime(2008, 12, 25),
            new DateTime(2009, 12, 25),
            new DateTime(2010, 12, 24),
            new DateTime(2011, 12, 26),
            new DateTime(2012, 12, 25),
            new DateTime(2013, 12, 25),
            new DateTime(2014, 12, 25)
        };
    }
} // End QC Namespace:
