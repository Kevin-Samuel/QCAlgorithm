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
    /// Subscription data required including the type of data.
    /// </summary>
    public struct SubscriptionDataConfig
    {
        /******************************************************** 
        * STRUCT PUBLIC VARIABLES
        *********************************************************/
        /// Type of data
        public Type Type;
        /// Security type of this data subscription
        public SecurityType Security;
        /// Symbol of the asset we're requesting.
        public string Symbol;
        /// Resolution of the asset we're requesting, second minute or tick
        public Resolution Resolution;
        /// True if wish to send old data when time gaps in data feed.
        public bool FillDataForward;
        /// Boolean Send Data from between 4am - 8am (Equities Setting Only)
        public bool ExtendedMarketHours;
        /// Price Scaling Factor:
        public decimal PriceScaleFactor;
        ///Symbol Mapping: When symbols change over time (e.g. CHASE-> JPM) need to update the symbol requested.
        public string MappedSymbol;

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Constructor for Data Subscriptions
        /// </summary>
        /// <param name="objectType">Type of the data objects.</param>
        /// <param name="securityType">SecurityType Enum Set Equity/FOREX/Futures etc.</param>
        /// <param name="symbol">Symbol of the asset we're requesting</param>
        /// <param name="resolution">Resolution of the asset we're requesting</param>
        /// <param name="fillForward">Fill in gaps with historical data</param>
        /// <param name="extendedHours">Equities only - send in data from 4am - 8pm</param>
        public SubscriptionDataConfig(Type objectType, SecurityType securityType = SecurityType.Equity, string symbol = "", Resolution resolution = Resolution.Minute, bool fillForward = true, bool extendedHours = false)
        {
            this.Type = objectType;
            this.Security = securityType;
            this.Resolution = resolution;
            this.Symbol = symbol;
            this.FillDataForward = fillForward;
            this.ExtendedMarketHours = extendedHours;
            this.PriceScaleFactor = 1;
            this.MappedSymbol = symbol;
        }

        /// <summary>
        /// User defined source of data configuration
        /// </summary>
        /// <param name="objectType">Type the user defines</param>
        /// <param name="symbol">Symbol of the asset we'll trade</param>
        /// <param name="source">String source of the data.</param>
        public SubscriptionDataConfig(Type objectType, string symbol, string source)
        {
            this.Type = objectType;
            this.Security = SecurityType.Base;
            this.Resolution = Resolution.Second;
            this.Symbol = symbol;

            //NOT NEEDED FOR USER DATA:*********//
            this.FillDataForward = true;        //
            this.ExtendedMarketHours = false;   //
            this.PriceScaleFactor = 1;          //
            this.MappedSymbol = symbol;         //
        }

        /// <summary>
        /// Update the price scaling factor for this subscription:
        /// -> Used for backwards scaling _equity_ prices to adjust for splits and dividends. Unused
        /// </summary>
        public void SetPriceScaleFactor(decimal newFactor) {
            PriceScaleFactor = newFactor;
        }

        /// <summary>
        /// Update the mapped symbol stored here: 
        /// </summary>
        /// <param name="newSymbol"></param>
        public void SetMappedSymbol(string newSymbol) {
            MappedSymbol = newSymbol;
        }

    } // End Base Data Class

} // End QC Namespace
