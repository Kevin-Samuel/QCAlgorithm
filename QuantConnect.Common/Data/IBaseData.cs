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

namespace QuantConnect.Models
{

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Base Data Class: Type, Timestamp, Key -- Base Features.
    /// </summary>
    public interface IBaseData
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
        MarketDataType DataType
        {
            get;
            set;
        }
        
        /// <summary>
        /// Time keeper of data -- all data is timeseries based.
        /// </summary>
        DateTime Time
        {
            get;
            set;
        }
        
        
        /// <summary>
        /// Symbol for underlying Security
        /// </summary>
        string Symbol
        {
            get;
            set;
        }


        /// <summary>
        /// All timeseries data is a time-value pair:
        /// </summary>
        decimal Value
        {
            get;
            set;
        }


        /// <summary>
        /// Alias of Value.
        /// </summary>
        decimal Price
        {
            get;
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Reader Method :: using set of arguements we specify read out type. Enumerate
        /// until the end of the data stream or file. E.g. Read CSV file line by line and convert
        /// into data types.
        /// </summary>
        /// <returns>BaseData type set by Subscription Method.</returns>
        BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed);


        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream 
        /// </summary>
        /// <param name="datafeed">Type of datafeed we're reqesting - backtest or live</param>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <returns>String URL of source file.</returns>
        string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed);


        /// <summary>
        /// Return a new instance clone of this object
        /// </summary>
        /// <returns></returns>
        BaseData Clone();

    } // End Base Data Class

} // End QC Namespace
