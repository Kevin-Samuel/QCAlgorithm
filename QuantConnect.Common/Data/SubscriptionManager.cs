/*
* QUANTCONNECT.COM - 
* QC.Algorithm - Base Class for Algorithm.
* DataManager - Helper routines for the algorithm controller.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using QuantConnect.Logging;

namespace QuantConnect.Models {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Enumerable Subscription Management Class
    /// </summary>
    public class SubscriptionManager {

        /******************************************************** 
        * PUBLIC VARIABLES
        *********************************************************/
        /// Generic Market Data Requested and Object[] Arguements to Get it:
        public List<SubscriptionDataConfig> Subscriptions;

        /******************************************************** 
        * PRIVATE VARIABLES
        *********************************************************/
        
        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Generic Data Manager Class
        /// </summary>
        public SubscriptionManager() 
        {
            //Generic Type Data Holder:
            Subscriptions = new List<SubscriptionDataConfig>();

        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Get the count of assets:
        /// </summary>
        public int Count 
        {
            get 
            { 
                return Subscriptions.Count; 
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Add Market Data Required (Overloaded method for backwards compatibility).
        /// </summary>
        /// <param name="security">Market Data Asset</param>
        /// <param name="symbol">Symbol of the asset we're like</param>
        /// <param name="resolution">Resolution of Asset Required</param>
        /// <param name="fillDataForward">when there is no data pass the last tradebar forward</param>
        /// <param name="extendedMarketHours">Request premarket data as well when true </param>
        public void Add(SecurityType security, string symbol, Resolution resolution = Resolution.Minute, bool fillDataForward = true, bool extendedMarketHours = false)
        {
            //Set the type: market data only comes in two forms -- ticks(trade by trade) or tradebar(time summaries)
            Type dataType = typeof(TradeBar);

            if (resolution == Resolution.Tick) 
            {
                dataType = typeof(Tick);
            }

            this.Add(dataType, security, symbol, resolution, fillDataForward, extendedMarketHours);
        }


        /// <summary>
        /// Add Market Data Required - generic data typing support as long as Type implements IBaseData.
        /// </summary>
        /// <param name="dataType">Set the type of the data we're subscribing to.</param>
        /// <param name="security">Market Data Asset</param>
        /// <param name="symbol">Symbol of the asset we're like</param>
        /// <param name="resolution">Resolution of Asset Required</param>
        /// <param name="fillDataForward">when there is no data pass the last tradebar forward</param>
        /// <param name="extendedMarketHours">Request premarket data as well when true </param>
        public void Add(Type dataType, SecurityType security, string symbol, Resolution resolution = Resolution.Minute, bool fillDataForward = true, bool extendedMarketHours = false) 
        {
            //Clean:
            symbol = symbol.ToUpper();

            //Create:
            SubscriptionDataConfig newConfig = new SubscriptionDataConfig(dataType, security, symbol, resolution, fillDataForward, extendedMarketHours);

            //Add to subscription list: make sure we don't have his symbol:
            Subscriptions.Add(newConfig);
        }


        /// <summary>
        /// Get the settings object for this ticker:
        /// </summary>
        /// <param name="symbol">Symbol we're searching for in the subscriptions list</param>
        /// <returns>SubscriptionDataConfig Configuration Object</returns>
        private SubscriptionDataConfig GetSetting(string symbol)
        {
            return (from config in Subscriptions 
                    where config.Symbol == symbol.ToUpper() 
                    select config).SingleOrDefault();
        }

    } // End Algorithm MetaData Manager Class

} // End QC Namespace
