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

namespace QuantConnect.Securities 
{
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
    public class SecurityTransactionModel : ISecurityTransactionModel 
    {
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
        public SecurityTransactionModel() 
        { 
        
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
        public virtual OrderEvent Fill(Security vehicle, Order order)
        {
            //Default order event to return.
            var fill = new OrderEvent(order);

            try 
            {
                switch (order.Type) 
                {
                    case OrderType.Limit:
                        fill = LimitFill(vehicle, order);
                        break;
                    case OrderType.StopMarket:
                        fill = StopFill(vehicle, order);
                        break;
                    case OrderType.Market:
                        fill = MarketFill(vehicle, order);
                        break;
                }
            } catch (Exception err) {
                Log.Error("SecurityTransactionModel.TransOrderDirection.Fill(): " + err.Message);
            }

            return fill;
        }


        /// <summary>
        /// Get the Slippage approximation for this order:
        /// </summary>
        public virtual decimal GetSlippageApproximation(Security security, Order order) 
        {
            return 0;
        }


        /// <summary>
        /// Default market order model. Fill at last price
        /// </summary>
        /// <param name="security">Asset we're working with</param>
        /// <param name="order">Order to update</param>
        public virtual OrderEvent MarketFill(Security security, Order order)
        {
            //Default order event to return.
            var fill = new OrderEvent(order);
            try {
                
                //Set the order price 
                order.Price = security.Price;
                order.Status = OrderStatus.Filled;

                //Set the order event fill: - Assuming 100% fill
                fill.FillPrice = security.Price;
                fill.FillQuantity = order.Quantity;
                fill.Status = order.Status;

            } catch (Exception err) {
                Log.Error("SecurityTransactionModel.TransOrderDirection.MarketFill(): " + err.Message);
            }
            return fill;
        }




        /// <summary>
        /// Check if the model has stopped out our position yet:
        /// </summary>
        /// <param name="security">Asset we're working with</param>
        /// <param name="order">Stop Order to Check, return filled if true</param>
        public virtual OrderEvent StopFill(Security security, Order order)
        {
            //Default order event to return.
            var fill = new OrderEvent(order);

            try 
            {
                //If its cancelled don't need anymore checks:
                if (fill.Status == OrderStatus.Canceled) return fill;

                //Check if the Stop Order was filled: opposite to a limit order
                switch (order.Direction)
                {
                    case OrderDirection.Sell:
                        //-> 1.1 Sell Stop: If Price below setpoint, Sell:
                        if (security.Price < order.Price) 
                        {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                    case OrderDirection.Buy:
                        //-> 1.2 Buy Stop: If Price Above Setpoint, Buy:
                        if (security.Price > order.Price) 
                        {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                }

                if (order.Status == OrderStatus.Filled || order.Status == OrderStatus.PartiallyFilled)
                {
                    fill.FillQuantity = order.Quantity;
                    fill.FillPrice = security.Price;        //Stop price as security price because can gap past stop price.
                    fill.Status = order.Status;
                }
            } 
            catch (Exception err) 
            {
                Log.Error("SecurityTransactionModel.TransOrderDirection.StopFill(): " + err.Message);
            }

            return fill;
        }



        /// <summary>
        /// Check if the price MarketDataed to our limit price yet:
        /// </summary>
        /// <param name="security">Asset we're working with</param>
        /// <param name="order">Limit order in market</param>
        public virtual OrderEvent LimitFill(Security security, Order order)
        {

            //Initialise;
            decimal marketDataMinPrice = 0;
            decimal marketDataMaxPrice = 0;
            var fill = new OrderEvent(order);

            try {
                //If its cancelled don't need anymore checks:
                if (fill.Status == OrderStatus.Canceled) return fill;

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
                        if (marketDataMinPrice < order.Price) 
                        {   
                            //Set order fill:
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                    case OrderDirection.Sell:
                        //Sell limit seeks highest price possible
                        if (marketDataMaxPrice > order.Price) 
                        {
                            order.Status = OrderStatus.Filled;
                            order.Price = security.Price;
                        }
                        break;
                }

                if (order.Status == OrderStatus.Filled || order.Status == OrderStatus.PartiallyFilled)
                {
                    fill.FillQuantity = order.Quantity;
                    fill.FillPrice = security.Price;
                    fill.Status = order.Status;
                }
            } 
            catch (Exception err) 
            {
                Log.Error("SecurityTransactionModel.TransOrderDirection.LimitFill(): " + err.Message);
            }
            return fill;
        }



        /// <summary>
        /// Default Security Transaction Model - No Fees.
        /// </summary>
        public virtual decimal GetOrderFee(decimal quantity, decimal price)
        {
            return 0;
        }

    } // End Algorithm Transaction Filling Classes

} // End QC Namespace
