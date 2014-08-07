/*
* QUANTCONNECT.COM: Security Holding Class
* Track portfolio, cash, leverage, buying power etc.
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
    /// Market Holding - Base class for common method of purchasing and holding
    /// a market item.
    /// </summary>
    public class SecurityHolding {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        
        /// <summary>
        /// Public Property for the Underlying Market Asset:
        /// </summary>
        public ISecurityTransactionModel Model {
            get {
                return _model;
            }
        }
        

        /// <summary>
        /// Public Holdings Property for the average holdings price:
        /// </summary>
        public decimal AveragePrice {
            get {
                return _averagePrice;
            }
        }
        

        /// <summary>
        /// Public Holdings Property for the Quantity of Asset Held.
        /// </summary>
        public int Quantity {
            get {
                return _quantity;
            }
        }


        /// <summary>
        /// Public Holdings Property for the symbol of the underlying asset:
        /// </summary>
        public string Symbol {
            get {
                return _symbol;
            }
        }
        

        //Working Variables
        private decimal _averagePrice = 0;
        private int     _quantity = 0;
        private decimal _price = 0;
        private string  _symbol = "";
        private decimal _totalSaleVolume = 0;
        private decimal _profit = 0;
        private decimal _lastTradeProfit = 0;
        private decimal _totalFees = 0;
        private ISecurityTransactionModel _model;

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Launch a new holding class with Trader Dealer:
        /// </summary>
        public SecurityHolding(string symbol, ISecurityTransactionModel transactionModel)
        {
            this._model = transactionModel;
            this._symbol = symbol;

            //Total Sales Volume for the day
            this._totalSaleVolume = 0;
            this._lastTradeProfit = 0;

        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Return the acquisition cost of the companies total holdings.
        /// </summary>
        public virtual decimal HoldingsCost 
        {
            get 
            {
                return AveragePrice * Convert.ToDecimal(Quantity);
            }
        }

        /// <summary>
        /// Absolute of the Holdings: Unlevered
        /// </summary>
        public virtual decimal AbsoluteHoldingsCost 
        {
            get 
            {
                return Math.Abs(HoldingsCost);
            }
        }

        /// <summary>
        /// Market value of our holdings
        /// </summary>
        public virtual decimal HoldingsValue
        {
            get
            {
                return _price * Convert.ToDecimal(Quantity);
            }
        }

        /// <summary>
        /// Absolute of the market value of our holdings.
        /// </summary>
        public virtual decimal AbsoluteHoldingsValue
        {
            get
            {
                return Math.Abs(HoldingsValue);
            }
        }
        
        /// <summary>
        /// If we have stock, return boolean true.
        /// </summary>
        public virtual bool HoldStock 
        {
            get 
            {
                return (AbsoluteQuantity > 0);
            }
        }


        /// <summary>
        /// Alias of Hold Stock
        /// </summary>
        public virtual bool Invested
        {
            get
            {
                return HoldStock;
            }
        }

        /// <summary>
        /// The total volume today for this stock.
        /// </summary>
        public virtual decimal TotalSaleVolume 
        {
            get 
            {
                return _totalSaleVolume;
            }
        }

        /// <summary>
        /// Total fees for this company
        /// </summary>
        public virtual decimal TotalFees 
        {
            get 
            {
                return _totalFees;
            }
        }

        /// <summary>
        /// Bool for when we hold long stock
        /// </summary>
        public virtual bool IsLong 
        {
            get {
                if (Quantity > 0) 
                {
                    return true;
                } 
                else 
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Bool indicating currently short.
        /// </summary>
        public virtual bool IsShort 
        {
            get {
                if (Quantity < 0) 
                {
                    return true;
                } 
                else
                {
                    return false;
                }
            }            
        }

        /// <summary>
        /// Absolute quantity
        /// </summary>
        public virtual decimal AbsoluteQuantity 
        {
            get 
            {
                return Math.Abs(Quantity);
            }
        }

        /// <summary>
        /// Using what was the profit from the last trade (buy or sell, need to account for shorts..)
        /// </summary>
        public virtual decimal LastTradeProfit 
        {
            get 
            {
                return _lastTradeProfit;
            }
        }

        /// <summary>
        /// Calculate the total profit for this equity.
        /// </summary>
        public virtual decimal Profit 
        {
            get 
            {
                return _profit;
            }
        }

        /// <summary>
        /// Return the net for this company
        /// </summary>
        public virtual decimal NetProfit 
        {
            get 
            {
                return Profit - TotalFees;
            }
        }

        /// <summary>
        /// The profit/loss figure from the holdings we currently have.
        /// </summary>
        public virtual decimal UnrealizedProfit 
        {
            get 
            {
                return TotalCloseProfit();
            }
        }

        /******************************************************** 
        * CLASS METHODS 
        *********************************************************/
        /// <summary>
        /// Add this extra fee to the running total:
        /// </summary>
        /// <param name="newFee"></param>
        public void AddNewFee(decimal newFee) 
        {
            this._totalFees += newFee;
        }

        /// <summary>
        /// Add a new Profit or Loss to the running total:
        /// </summary>
        /// <param name="profitLoss">The change in portfolio from closing a position</param>
        public void AddNewProfit(decimal profitLoss) 
        {
            this._profit += profitLoss;
        }

        /// <summary>
        /// Add a new sale value to the running total trading volume.
        /// </summary>
        /// <param name="saleValue"></param>
        public void AddNewSale(decimal saleValue) 
        {
            this._totalSaleVolume += saleValue;
        }

        /// <summary>
        /// Setter Method for Last Trade Profit.
        /// </summary>
        /// <param name="lastTradeProfit"></param>
        public void SetLastTradeProfit(decimal lastTradeProfit) 
        {
            this._lastTradeProfit = lastTradeProfit;
        }
            
        /// <summary>
        /// Set the quantity - useful if running a model in running total mode and have overnight holdings.
        /// </summary>
        public virtual void SetHoldings(decimal averagePrice, int quantity) 
        {
            this._averagePrice = averagePrice;
            this._quantity = quantity;
        }

        /// <summary>
        /// Update local copy of closing price variable
        /// </summary>
        /// <param name="closingPrice">price of the underlying asset</param>
        public virtual void UpdatePrice(decimal closingPrice)
        {
            _price = closingPrice;
        }

        /// <summary>
        /// Profit if we closed the holdings right now. If relative per dollar is true, will return the efficiency
        /// </summary>
        public virtual decimal TotalCloseProfit() 
        {
            decimal gross = 0, net = 0;
            decimal orderFee = 0;

            if (AbsoluteQuantity > 0) 
            {
                _model.GetOrderFee(AbsoluteQuantity, _price);
            }

            if (IsLong) 
            {
                //if we're long on a position, profit from selling off $10,000 stock:
                gross = (_price - AveragePrice) * AbsoluteQuantity;
            } 
            else if (IsShort) 
            {
                //if we're short on a position, profit from buying $10,000 stock:
                gross = (AveragePrice - _price) * AbsoluteQuantity;
            } 
            else 
            {
                //no holdings, 0 profit.
                return 0;
            }

            net = gross - orderFee;

            return net;
        }

    }
} //End Namespace