/*
* QUANTCONNECT.COM - 
* QC.Algorithm - Base Class for Algorithm.
* Data Base - Base class for all data items.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;

namespace QuantConnect.Models {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Base Data Class: Type, Timestamp, Key -- Base Features.
    /// </summary>
    public abstract class BaseData : IBaseData
    {
        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/
        MarketDataType _dataType = MarketDataType.Base;
        DateTime _time = new DateTime();
        string _symbol = "";
        decimal _value = 0;

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/
        /// <summary>
        /// Market Data Type of this data - does it come in individual price packets or is it grouped into OHLC.
        /// </summary>
        public MarketDataType DataType
        {
            get 
            {
                return _dataType;
            }
            set 
            {
                _dataType = value;
            }
        }
        
        /// <summary>
        /// Time keeper of data -- all data is timeseries based.
        /// </summary>
        public DateTime Time
        {
            get
            {
                return _time;
            }
            set
            {
                _time = value;
            }
        }
        
        /// <summary>
        /// Symbol for underlying Security
        /// </summary>
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
            }
        }

        /// <summary>
        /// All timeseries data is a time-value pair:
        /// </summary>
        public decimal Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

        /// <summary>
        /// Alias of Value.
        /// </summary>
        public decimal Price {
            get 
            {
                return Value;
            }
        }
        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Base Data Class
        /// </summary>
        public BaseData() 
        { 
            //Empty constructor required.
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        
        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Reader Method :: using set of arguements we specify read out type. Enumerate
        /// until the end of the data stream or file. E.g. Read CSV file line by line and convert
        /// into data types.
        /// </summary>
        /// <returns>BaseData type set by Subscription Method.</returns>
        public abstract BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed);


        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream 
        /// </summary>
        /// <param name="datafeed">Type of datafeed we're reqesting - backtest or live</param>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <returns>String URL of source file.</returns>
        public abstract string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed);


        /// <summary>
        /// Update routine to build a bar/tick from a data update.
        /// </summary>
        /// <param name="lastTrade">The last trade price</param>
        /// <param name="bidPrice">Current bid price</param>
        /// <param name="askPrice">Current asking price</param>
        /// <param name="volume">Volume of this trade</param>
        public virtual void Update(decimal lastTrade, decimal bidPrice, decimal askPrice, decimal volume)
        {
            this.Value = lastTrade;
        }

        /// <summary>
        /// Return a new instance clone of this object
        /// </summary>
        /// <returns></returns>
        public virtual BaseData Clone() 
        { 
            //Optional implementation
            return default(BaseData);
        }

    } // End Base Data Class

} // End QC Namespace
