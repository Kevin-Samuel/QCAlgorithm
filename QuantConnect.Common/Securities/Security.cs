/*
* QUANTCONNECT.COM: Security Object
* This is a trable asset, that you build into a portfolio.
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// A base "Market" Vehicle Class for Providing a common interface to Indexes / Equities / FOREX Trading.
    /// </summary>
    public partial class Security {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        /// <summary>
        /// Public Symbol Property: Code for the asset:
        /// </summary>
        public string Symbol 
        {
            get 
            {
                return _symbol;
            }
        }
        
        /// <summary>
        /// Public Type of Market: Equity Forex etc.
        /// </summary>
        public SecurityType Type 
        {
            get 
            {
                return _type;
            }
        }

        /// <summary>
        /// Public Resolution of this Market Asset.
        /// </summary>
        public Resolution Resolution 
        {
            get 
            {
                return _resolution;
            }
        }

        /// <summary>
        /// Public Readonly Property: If there's no new data packets for each second, we'll fill in the last packet we recieved.
        /// </summary>
        public bool IsFillDataForward 
        {
            get 
            {
                return _isFillDataForward;
            }
        }

        /// <summary>
        /// When its an extended market hours vehidle return true. Read only, set this when requesting the assets.
        /// </summary>
        public bool IsExtendedMarketHours
        {
            get 
            {
                return _isExtendedMarketHours;
            }
        }

        /// <summary>
        /// Security Cache Class: Order and Data Storage:
        /// </summary>
        public virtual SecurityCache Cache { get; set; }

        /// <summary>
        /// Security Holdings Manager: Cash, Holdings, Quantity
        /// </summary>
        public virtual SecurityHolding Holdings { get; set; }

        /// <summary>
        /// Security Exchange Details Class: 
        /// </summary>
        public virtual SecurityExchange Exchange { get; set; }

        /// <summary>
        /// Security Transaction Model Storage
        /// </summary>
        public virtual ISecurityTransactionModel Model { get; set; }

        /// <summary>
        /// Customizable Data Filter to filter outlier ticks:
        /// </summary>
        public virtual ISecurityDataFilter DataFilter { get; set; }

        //Market Data Type:
        private string _symbol = "";
        private SecurityType _type = SecurityType.Equity;
        private Resolution _resolution = Resolution.Second;
        private bool _isFillDataForward = false;
        private bool _isExtendedMarketHours = false;
        private bool _isQuantConnectData = false;
        private decimal _leverage = 1;

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Construct the Market Vehicle
        /// </summary>
        public Security(string symbol, SecurityType type, Resolution resolution, bool fillDataForward, decimal leverage, bool extendedMarketHours, bool useQuantConnectData = false) 
        {
            //Set Basics:
            this._symbol = symbol;
            this._type = type;
            this._resolution = resolution;
            this._isFillDataForward = fillDataForward;
            this._leverage = leverage;
            this._isExtendedMarketHours = extendedMarketHours;
            this._isQuantConnectData = useQuantConnectData;

            //Setup Transaction Model for this Asset
            switch (type) 
            { 
                case SecurityType.Equity:
                    Model = new EquityTransactionModel();
                    DataFilter = new EquityDataFilter();
                    break;
                case SecurityType.Forex:
                    Model = new ForexTransactionModel();
                    DataFilter = new ForexDataFilter();
                    break;
                case SecurityType.Base:
                    Model = new SecurityTransactionModel();
                    DataFilter = new SecurityDataFilter();
                    break;
            }

            //Holdings for new Vehicle:
            Cache = new SecurityCache();
            Holdings = new SecurityHolding(symbol, Model);
            Exchange = new SecurityExchange();
        }



        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Read only property that checks if we currently own stock in the company.
        /// </summary>
        public virtual bool HoldStock 
        {
            //Get a boolean, true if we own this stock.
            get 
            {
                if (Holdings.AbsoluteQuantity > 0) 
                {
                    return true; //If we find stock in the holdings table we own stock, return true.
                } 
                else 
                {
                    return false; //No stock found. 
                }
            }
        }

        /// <summary>
        /// Alias for HoldStock - Do we have any of this security
        /// </summary>
        public virtual bool Invested 
        {
            get
            {
                return HoldStock;
            }
        }

        /// <summary>
        /// Local Time for this Market 
        /// </summary>
        public virtual DateTime Time 
        {
            get 
            {
                return Exchange.Time;
            }
        }

        /// <summary>
        /// Get the current value of a Market Code
        /// </summary>
        public virtual decimal Price {
            //Get the current Market value from the database
            get 
            {
                BaseData data = GetLastData();
                if (data != null) 
                {
                    return data.Value;
                } 
                else 
                {
                    //Error fetching depth
                    return 0;
                }
            }
        }

        /// <summary>
        /// Leverage for this Security.
        /// </summary>
        public virtual decimal Leverage
        {
            get 
            { 
                return _leverage; 
            }
        }

        /// <summary>
        /// Use QuantConnect data source flag, or is the security a user object
        /// </summary>
        public virtual bool IsQuantConnectData 
        {
            get
            {
                return _isQuantConnectData;
            }
        }

        /// <summary>
        /// If this uses tradebar data, return the most recent high.
        /// </summary>
        public virtual decimal High {
            get 
            { 
                BaseData data = GetLastData();
                if (data.DataType == MarketDataType.TradeBar) 
                {
                    return ((TradeBar)data).High;
                } 
                else 
                {
                    return data.Value;
                }
            }
        }

        /// <summary>
        /// If this uses tradebar data, return the most recent low.
        /// </summary>
        public virtual decimal Low {
            get {
                BaseData data = GetLastData();
                if (data.DataType == MarketDataType.TradeBar) 
                {
                    return ((TradeBar)data).Low;
                } 
                else 
                {
                    return data.Value;
                }
            }
        }

        /// <summary>
        /// If this uses tradebar data, return the most recent close.
        /// </summary>
        public virtual decimal Close 
        {
            get 
            {
                BaseData data = GetLastData();
                if (data == null) return 0;
                return data.Value;
            }
        }

        /// <summary>
        /// If this uses tradebar data, return the most recent open.
        /// </summary>
        public virtual decimal Open {
            get {
                BaseData data = GetLastData();
                if (data.DataType == MarketDataType.TradeBar) 
                {
                    return ((TradeBar)data).Open;
                } 
                else 
                {
                    return data.Value;
                }
            }
        }


        /// <summary>
        /// Access to the volume of the equity today
        /// </summary>
        public virtual long Volume
        {
            get
            {
                BaseData data = GetLastData();
                if (data.DataType == MarketDataType.TradeBar)
                {
                    return ((TradeBar)data).Volume;
                }
                else
                {
                    return 0;
                }
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Get a single data packet
        /// </summary>
        /// <returns></returns>
        public BaseData GetLastData() 
        {
            return this.Cache.GetData();
        }

        /// <summary>
        /// Update the Market Online Calculations:
        /// </summary>
        /// <param name="data">New Data packet:</param>
        /// <param name="frontier">time frontier / where we are in time.</param>
        public void Update(DateTime frontier, BaseData data) 
        { 
            //Update the Exchange/Timer:
            Exchange.SetDateTimeFrontier(frontier);

            //Add new point to cache:
            if (data != null)
            {
                Cache.AddData(data);
                Holdings.UpdatePrice(data.Value);
            }
        }


        /// <summary>
        /// Update the leverage parameter:
        /// </summary>
        /// <param name="leverage">Leverage for this asset:</param>
        public void SetLeverage(decimal leverage)
        {
            this._leverage = leverage;
        }

    } // End Market

} // End QC Namespace
