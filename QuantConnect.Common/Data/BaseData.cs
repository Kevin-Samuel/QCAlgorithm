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
    public abstract class BaseData
    {
        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/
        /// <summary>
        /// Market Data Type of this data - does it come in individual price packets or is it grouped into OHLC.
        /// </summary>
        public MarketDataType DataType = MarketDataType.Base;
        
        /// <summary>
        /// Time keeper of data -- all data is timeseries based.
        /// </summary>
        public DateTime Time;
        
        
        /// <summary>
        /// Symbol for underlying Security
        /// </summary>
        public string Symbol;


        /// <summary>
        /// All timeseries data is a time-value pair:
        /// </summary>
        public decimal Value;
        

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Base Data Class
        /// </summary>
        public BaseData() { }


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
        /// Return a new instance clone of this object
        /// </summary>
        /// <returns></returns>
        public abstract BaseData Clone();

    } // End Base Data Class

} // End QC Namespace
