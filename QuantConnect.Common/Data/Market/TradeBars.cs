/*
* QUANTCONNECT.COM - 
* QC.Algorithm - Base Class for Algorithm.
* TradeBar Data Objects Second and Minute Summaries - Extension of Market Data classes - for tradable market objects
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using QuantConnect;
using QuantConnect.Logging;


namespace QuantConnect.Models {


    /// <summary>
    /// Collection of TradeBars to create a data type for generic data handler:
    /// </summary>
    public class TradeBars : BaseData, IDictionary<string, TradeBar>
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// <summary>
        /// Id of this data tick
        /// </summary>
        public int Id = 0;

        //Internally store the tradebars in a basic dictionary:
        private Dictionary<string, TradeBar> _tradeBars = new Dictionary<string, TradeBar>();

        /******************************************************** 
        * CLASS CONSTRUCTOR:
        *********************************************************/
        /// <summary>
        /// TradeBars Default Initializer
        /// </summary>
        public TradeBars() 
        {
            base.Time = new DateTime();
            base.Value = 0;
            base.Symbol = "";
            base.DataType = MarketDataType.TradeBar;
        }
        
        /// <summary>
        /// Default constructor for tradebars collection
        /// </summary>
        /// <param name="frontier"></param>
        public TradeBars(DateTime frontier) {
            base.Time = frontier;
            base.Value = 0;
            base.Symbol = "";
            base.DataType = MarketDataType.TradeBar;
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// TradeBar Reader: Fetch the data from the QC storage and feed it line by line into the engine.
        /// </summary>
        /// <param name="datafeed">Where are we getting this datafeed from - backtesing or live.</param>
        /// <param name="config">Symbols, Resolution, DataType, </param>
        /// <param name="line">Line from the data file requested</param>
        /// <param name="date">Date of the reader request, only used when the source file changes daily.</param>
        /// <returns>Enumerable iterator for returning each line of the required data.</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            throw new Exception("TradeBars class not implemented. Use TradeBar reader instead.");
        }


        /// <summary>
        /// Get Source File URL for this TradeBar subscription request
        /// </summary>
        /// <param name="datafeed">Source of the datafeed / type of strings we'll be receiving</param>
        /// <param name="config">Configuration for the subscription</param>
        /// <param name="date">Date of the source file requested.</param>
        /// <returns>String URL Source File</returns>
        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Not Implemented Clone Function Command 
        /// - Don't need to implement for TradeBars array, each symbol-subscription is treated separately.
        /// </summary>
        /// <returns>BaseData Clone of TradeBars Array</returns>
        public override BaseData Clone()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// TradeBar IDictionary:: Add Implementation
        /// </summary>
        /// <param name="key">String ticker</param>
        /// <param name="value">TradeBar value</param>
        public void Add(string key, TradeBar value) {
            _tradeBars.Add(key, value);
        }

        /// <summary>
        /// TradeBar IDictionary :: GetEnumerator Implementation
        /// </summary>
        /// <returns></returns>
        IEnumerator<KeyValuePair<string, TradeBar>> IEnumerable<KeyValuePair<string, TradeBar>>.GetEnumerator()
        {
            return _tradeBars.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IDictionary)this).GetEnumerator();
        }

        /// <summary>
        /// TradeBar IDictionary :: IsReadOnly Implementation
        /// </summary>
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        /// <summary>
        /// TradeBar IDictionary :: Count Implementation
        /// </summary>
        public int Count {
            get {
                return _tradeBars.Count;
            }
        }

        /// <summary>
        /// TradeBar IDictionary :: Remove Implemenation
        /// </summary>
        /// <param name="key">Key ticker</param>
        public bool Remove(string key) {
            return _tradeBars.Remove(key);
        }

        /// <summary>
        /// TradeBars IDictionary :: Remove Implemenation 
        /// </summary>
        /// <param name="kvp"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<string, TradeBar> kvp) {
            return _tradeBars.Remove(kvp.Key);
        }

        /// <summary>
        /// TradeBar IDictionary :: Contains Implementation
        /// </summary>
        /// <param name="kvp">Key Value Pair to Search For</param>
        /// <returns>True if Found.</returns>
        public bool Contains(KeyValuePair<string, TradeBar> kvp) {
            return _tradeBars.ContainsKey(kvp.Key);
        }

        /// <summary>
        /// TradeBar IDictionary :: Contains Implementation
        /// </summary>
        /// <param name="key">Dictionary key</param>
        /// <returns>True if found.</returns>
        public bool ContainsKey(string key) {
            return _tradeBars.ContainsKey(key);
        }

        /// <summary>
        /// TradeBar IDictionary :: Clear Implementation
        /// </summary>
        public void Clear() {
            _tradeBars.Clear();
        }


        /// <summary>
        /// TradeBar IDictionary :: Add Implementation
        /// </summary>
        /// <param name="kvp"></param>
        public void Add(KeyValuePair<string, TradeBar> kvp) {
            _tradeBars.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// TradeBar IDictionary :: Values Implementation
        /// </summary>
        public ICollection<TradeBar> Values {
            get {
                return _tradeBars.Values;
            }
        }

        /// <summary>
        /// TradeBar IDictionary :: Keys Collection
        /// </summary>
        public ICollection<string> Keys {
            get {
                return _tradeBars.Keys;
            }
        }

        /// <summary>
        /// TradeBar IDictionary :: Indexer Implementation
        /// </summary>
        /// <param name="key">string key</param>
        /// <returns></returns>
        public TradeBar this[string key]
        {
            get
            {
                return _tradeBars[key];
            }
            set
            {
                _tradeBars[key] = value;
            }
        }

        /// <summary>
        /// TradeBar IDictionary :: TryGetValue Implementation
        /// </summary>
        /// <param name="key">Key/Ticker of TradeBar</param>
        /// <param name="bar">Tradebar</param>
        /// <returns>True if finds this key</returns>
        public bool TryGetValue(string key, out TradeBar bar) {
            if (_tradeBars.ContainsKey(key))
            {
                bar = _tradeBars[key];
                return true;
            }
            else 
            {
                bar = null;
                return false;
            }
        }

        /// <summary>
        /// TradeBar IDictionary :: CopyTo Implementation
        /// </summary>
        /// <param name="array">Destination Array</param>
        /// <param name="arrayIndex">Starting index</param>
        public void CopyTo(KeyValuePair<string, TradeBar>[] array, int arrayIndex)
        {
            Copy(this, array, arrayIndex);
        }

        private static void Copy<T>(ICollection<T> source, T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if ((array.Length - arrayIndex) < source.Count)
                throw new ArgumentException("Destination array is not large enough. Check array.Length and arrayIndex.");

            foreach (T item in source)
                array[arrayIndex++] = item;
        }
    }


} // End QC Namespace
