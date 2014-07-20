/*
* QUANTCONNECT.COM: Secuity Cache
* Common caching class for storing historical ticks etc.
*/

/**********************************************************
 * USING NAMESPACES
 **********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Common Caching Spot For Market Data and Averaging. 
    /// </summary>
    public class SecurityCache {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// <summary>
        /// Cache for the orders processed
        /// </summary>
        public List<Order> OrderCache;                //Orders Cache
       
        /// <summary>
        /// Last data for this security.
        /// </summary>
        private BaseData _lastData;

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Start a new Cache for the set Index Code
        /// </summary>
        public SecurityCache() {
            //ORDER CACHES:
            OrderCache = new List<Order>();           
        }


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/

        /// <summary>
        /// Add a list of new MarketData samples to the cache
        /// </summary>
        public virtual void AddData(BaseData data)
        {
            //Record as Last Added Packet:
            if (data != null) _lastData = data;
        }



        /// <summary>
        /// Get Last Data Packet Recieved for this Vehicle.
        /// </summary>
        /// <returns></returns>
        public virtual BaseData GetData()
        {
            return _lastData;
        }



        /// <summary>
        /// Add a TransOrderDirection
        /// </summary>
        public virtual void AddOrder(Order order) {
            lock (OrderCache) {
                OrderCache.Add(order);
            }
        }



        /// <summary>
        /// Reset as many of the Cache's as possible.
        /// </summary>
        public virtual void Reset() {
            //Data Cache
            _lastData = null; //Tick or TradeBar
            //Order Cache:
            OrderCache = new List<Order>();
        }


    } //End Cache

} //End Namespace