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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Models {


    /// <summary>
    /// BaseData Collection of Ticks
    /// </summary>
    public class Ticks : BaseData, IDictionary<string, List<Tick>> 
    {

        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        //Private storage of ticks collection
        private Dictionary<string, List<Tick>> _ticks = new Dictionary<string, List<Tick>>();


        /******************************************************** 
        * CONSTRUCTOR METHODS
        *********************************************************/
        /// <summary>
        /// Default constructor for the ticks collection
        /// </summary>
        /// <param name="frontier"></param>
        public Ticks(DateTime frontier) {
            base.Time = frontier;
            base.Symbol = "";
            base.Value = 0;
            base.DataType = MarketDataType.Tick;
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/        
        /// <summary>
        /// Ticks Array Reader: Fetch the data from the QC storage and feed it line by line into the 
        /// system.
        /// </summary>
        /// <param name="datafeed">Who is requesting this data, backtest or live streamer</param>
        /// <param name="config">Symbols, Resolution, DataType</param>
        /// <param name="line">Line from the data file requested</param>
        /// <param name="date">Date of the reader day:</param>
        /// <returns>Enumerable iterator for returning each line of the required data.</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Clonable Interface; create a new instance of the object 
        /// - Don't need to implement for Ticks array, each symbol-subscription is treated separately.
        /// </summary>
        /// <returns>BaseData clone of the Ticks Array</returns>
        public override BaseData Clone()
        {
            throw new NotImplementedException();
        }



        /// <summary>
        /// Get the source file for this tick subscription
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
        /// IDictionary:: Add Implementation
        /// </summary>
        /// <param name="key">String ticker</param>
        /// <param name="value">TradeBar value</param>
        public void Add(string key, List<Tick> value)
        {
            _ticks.Add(key, value);
        }

        /// <summary>
        /// IDictionary :: GetEnumerator Implementation
        /// </summary>
        /// <returns></returns>
        IEnumerator<KeyValuePair<string, List<Tick>>> IEnumerable<KeyValuePair<string, List<Tick>>>.GetEnumerator()
        {
            return _ticks.GetEnumerator();
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IDictionary)this).GetEnumerator();
        }

        /// <summary>
        /// IDictionary :: IsReadOnly Implementation
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// IDictionary :: Count Implementation
        /// </summary>
        public int Count
        {
            get
            {
                return _ticks.Count;
            }
        }

        /// <summary>
        /// IDictionary :: Remove Implemenation
        /// </summary>
        /// <param name="key">Key ticker</param>
        public bool Remove(string key)
        {
            return _ticks.Remove(key);
        }

        /// <summary>
        /// IDictionary :: Remove Implemenation
        /// </summary>
        /// <param name="kvp">KVP Remove</param>
        /// <returns>True</returns>
        public bool Remove(KeyValuePair<string, List<Tick>> kvp)
        {
            return _ticks.Remove(kvp.Key);
        }

        /// <summary>
        /// IDictionary :: Contains Implementation
        /// </summary>
        /// <param name="kvp"></param>
        /// <returns>True</returns>
        public bool Contains(KeyValuePair<string, List<Tick>> kvp)
        {
            return _ticks.ContainsKey(kvp.Key);
        }

        /// <summary>
        /// IDictionary :: ContainsKey Implementation
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _ticks.ContainsKey(key);
        }

        /// <summary>
        /// IDictionary :: Clear Implementation
        /// </summary>
        public void Clear()
        {
            _ticks.Clear();
        }

        /// <summary>
        /// IDictionary :: Add Implementation
        /// </summary>
        /// <param name="kvp"></param>
        public void Add(KeyValuePair<string, List<Tick>> kvp)
        {
            _ticks.Add(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// IDictionary :: Values Implementation
        /// </summary>
        public ICollection<List<Tick>> Values
        {
            get
            {
                return _ticks.Values;
            }
        }

        /// <summary>
        /// IDictionary :: Keys Collection
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                return _ticks.Keys;
            }
        }

        /// <summary>
        /// IDictionary :: Indexer Implementation
        /// </summary>
        /// <param name="key">string key</param>
        /// <returns></returns>
        public List<Tick> this[string key]
        {
            get
            {
                return _ticks[key];
            }
            set
            {
                _ticks[key] = value;
            }
        }

        /// <summary>
        /// IDictionary :: TryGetValue Implementation
        /// </summary>
        /// <param name="key">Key of </param>
        /// <param name="bar"></param>
        /// <returns></returns>
        public bool TryGetValue(string key, out List<Tick> bar)
        {
            if (_ticks.ContainsKey(key))
            {
                bar = _ticks[key];
                return true;
            }
            else
            {
                bar = null;
                return false;
            }
        }

        /// <summary>
        /// IDictionary :: CopyTo Implementation
        /// </summary>
        /// <param name="array">Destination Array</param>
        /// <param name="arrayIndex">Starting index</param>
        public void CopyTo(KeyValuePair<string, List<Tick>>[] array, int arrayIndex)
        {
            Copy(this, array, arrayIndex);
        }

        /// <summary>
        /// Copy Generic Implementation
        /// </summary>
        /// <typeparam name="T">Type of copy to.</typeparam>
        /// <param name="source">Source of the copy.</param>
        /// <param name="array">Array destinations</param>
        /// <param name="arrayIndex">Index of current copy.</param>
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
