/*
* QUANTCONNECT.COM - Equity Transaction Model
* Default Equities Transaction Model
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using QuantConnect.Logging;

namespace QuantConnect.Securities {

    /******************************************************** 
    * QUANTCONNECT PROJECT LIBRARIES
    *********************************************************/
    using QuantConnect.Models;


    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Default Transaction Model for User Defined Securities:
    /// </summary>
    public class SecurityTransactionModel : ISecurityTransactionModel {

        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/


        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/


        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm Transaction Class
        /// </summary>
        public SecurityTransactionModel() {

        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Perform neccessary check to see if the model has been filled, appoximate the best we can.
        /// </summary>
        /// <param name="vehicle">Asset we're working with</param>
        /// <param name="order">Order class to check if filled.</param>
        public virtual void Fill(Security vehicle, ref Order order) {
            try {
                switch (order.Type) {
                    case OrderType.Limit:
                        LimitFill(vehicle, ref order);
                        break;
                    case OrderType.Stop:
                        StopFill(vehicle, ref order);
                        break;
                    case OrderType.Market:
                        MarketFill(vehicle, ref order);
                        break;
                }
            } catch (Exception err) {
                Log.Error("SecurityTransactionModel.TransOrderDirection.Fill(): " + err.Message);
            }
        }



        /// <summary>
        /// Get the Slippage approximation for this order:
        /// </summary>
        public virtual decimal GetSlippageApproximation(Security security, Order order) {
            return 0;
        }



        /// <summary>
        /// Default market order model. Fill at last price
        /// </summary>
        /// <param name="security">Asset we're working with</param>
        /// <param name="order">Order to update</param>
        public virtual void MarketFill(Security security, ref Order order) {
            try {
                order.Price = security.Price;
                order.Status = OrderStatus.Filled;
            } catch (Exception err) {
                Log.Error("SecurityTransactionModel.TransOrderDirection.MarketFill(): " + err.Message);
            }
        }




        /// <summary>
        /// Check if the model has stopped out our position yet:
        /// </summary>
        /// <param name="security">Asset we're working with</param>
        /// <param name="order">Stop Order to Check, return filled if true</param>
        public virtual void StopFill(Security security, ref Order order) {
            try {
                //If its cancelled don't need anymore checks:
                if (order.Status == OrderStatus.Canceled) return;

                //Check if the Stop Order was filled: opposite to a limit order
                switch (order.Direction)
                {
                    case OrderDirection.Sell:
                        //-> 1.1 Sell Stop: If Price below setpoint, Sell:
                        if (security.Price < order.Price) {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                    case OrderDirection.Buy:
                        //-> 1.2 Buy Stop: If Price Above Setpoint, Buy:
                        if (security.Price > order.Price) {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                }

            } catch (Exception err) {
                Log.Error("SecurityTransactionModel.TransOrderDirection.StopFill(): " + err.Message);
            }
        }



        /// <summary>
        /// Check if the price MarketDataed to our limit price yet:
        /// </summary>
        /// <param name="security">Asset we're working with</param>
        /// <param name="order">Limit order in market</param>
        public virtual void LimitFill(Security security, ref Order order) {

            //Initialise;
            decimal marketDataMinPrice = 0;
            decimal marketDataMaxPrice = 0;

            try {
                //If its cancelled don't need anymore checks:
                if (order.Status == OrderStatus.Canceled) return;

                //Depending on the resolution, return different data types:
                BaseData marketData = security.GetLastData();

                if (marketData.DataType == MarketDataType.TradeBar)
                {
                    marketDataMinPrice = ((TradeBar)marketData).Low;
                    marketDataMaxPrice = ((TradeBar)marketData).High;
                } else {
                    marketDataMinPrice = marketData.Value;
                    marketDataMaxPrice = marketData.Value;
                }

                //-> Valid Live/Model Order: 
                switch (order.Direction)
                {
                    case OrderDirection.Buy:
                        //Buy limit seeks lowest price
                        if (marketDataMinPrice < order.Price) {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                    case OrderDirection.Sell:
                        //Sell limit seeks highest price possible
                        if (marketDataMaxPrice > order.Price) {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                }

            } catch (Exception err) {
                Log.Error("SecurityTransactionModel.TransOrderDirection.LimitFill(): " + err.Message);
            }
        }



        /// <summary>
        /// Default Security Transaction Model - No Fees.
        /// </summary>
        public virtual decimal GetOrderFee(decimal quantity, decimal price) {
            return 0;
        }

    } // End Algorithm Transaction Filling Classes

} // End QC Namespace
