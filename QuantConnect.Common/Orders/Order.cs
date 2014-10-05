/*
 * QUANTCONNECT.COM - 
 * Order Models -- Common enums and classes for orders
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Generic;

namespace QuantConnect {
    
    /******************************************************** 
    * ORDER CLASS DEFINITION
    *********************************************************/
    /// <summary>
    /// Type of the Order: Market, Limit or Stop
    /// </summary>
    public enum OrderType {

        /// <summary>
        /// Market Order Type
        /// </summary>
        Market,

        /// <summary>
        /// Limit Order Type
        /// </summary>
        Limit,

        /// <summary>
        /// Stop Market Order Type - Fill at market price when break target price
        /// </summary>
        StopMarket
    }


    /// <summary>
    /// Order Duration in Market
    /// </summary>
    public enum OrderDuration
    { 
        /// <summary>
        /// Order good until its filled.
        /// </summary>
        GTC,

        /// <summary>
        /// Order valid for today only: -- CURRENTLY ONLY GTC ORDER DURATION TYPE IN BACKTESTS.
        /// </summary>
        //Day
    }


    /// <summary>
    /// Direction of the Order:
    /// </summary>
    public enum OrderDirection {

        /// <summary>
        /// Buy Order 
        /// </summary>
        Buy,

        /// <summary>
        /// Sell Order
        /// </summary>
        Sell,

        /// <summary>
        /// Default Value - No Order Direction
        /// </summary>
        Hold
    }


    /// <summary>
    /// Status of the order class.
    /// </summary>
    public enum OrderStatus {
        
        /// <summary>
        /// New order pre-submission to the order processor.
        /// </summary>
        New,

        /// <summary>
        /// Order flagged for updating the inmarket order.
        /// </summary>
        Update,

        /// <summary>
        /// Order submitted to the market
        /// </summary>
        Submitted,

        /// <summary>
        /// Partially filled, In Market Order.
        /// </summary>
        PartiallyFilled,

        /// <summary>
        /// Completed, Filled, In Market Order.
        /// </summary>
        Filled,

        /// <summary>
        /// Order cancelled before it was filled
        /// </summary>
        Canceled,

        /// <summary>
        /// No Order State Yet
        /// </summary>
        None,

        /// <summary>
        /// Order invalidated before it hit the market (e.g. insufficient capital)..
        /// </summary>
        Invalid
    }

    /// <summary>
    /// Indexed Order Codes:
    /// </summary>
    public static class OrderErrors {
        /// <summary>
        /// Order Validation Errors
        /// </summary>
        public static Dictionary<int, string> ErrorTypes = new Dictionary<int, string>() {
            {-1, "Order quantity must not be zero"},
            {-2, "There is no data yet for this security - please wait for data (market order price not available yet)"},
            {-3, "Attempting market order outside of market hours"},
            {-4, "Insufficient capital to execute order"},
            {-5, "Exceeded maximum allowed orders for one analysis period"},
            {-6, "Order timestamp error. Order appears to be executing in the future"},
            {-7, "General error in order"},
            {-8, "Order has already been filled and cannot be modified"},
        };


    }


    /// <summary>
    /// Trade Struct for Recording Results:
    /// </summary>
    public class Order {

        /// <summary>
        /// Order ID.
        /// </summary>
        public int Id;

        /// <summary>
        /// Brokerage Id for this order.
        /// </summary>
        public long BrokerId;

        /// <summary>
        /// Symbol of the Asset
        /// </summary>
        public string Symbol;
        
        /// <summary>
        /// Price of the Order.
        /// </summary>
        public decimal Price;

        /// <summary>
        /// Time the order was created.
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// Number of shares to execute.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// Order Type
        /// </summary>
        public OrderType Type;

        /// <summary>
        /// Status of the Order
        /// </summary>
        public OrderStatus Status;

        /// <summary>
        /// Order duration - GTC or Day. Day not supported in backtests.
        /// </summary>
        public OrderDuration Duration = OrderDuration.GTC;

        /// <summary>
        /// Tag the order with some custom data
        /// </summary>
        public string Tag = "";

        /// <summary>
        /// Order Direction Property based off Quantity.
        /// </summary>
        public OrderDirection Direction {
            get {
                if (Quantity > 0) {
                    return OrderDirection.Buy;
                } else if (Quantity < 0) {
                    return OrderDirection.Sell;
                } else {
                    return OrderDirection.Hold;
                }
            }
        }

        /// <summary>
        /// Get the Absolute (non-negative) quantity.
        /// </summary>
        public decimal AbsoluteQuantity {
            get {
                return Math.Abs(Quantity);
            }
        }

        /// <summary>
        /// Value of the Order:
        /// </summary>
        public decimal Value {
            get {
                return Convert.ToDecimal(Quantity) * Price;
            }
        }

        /// <summary>
        /// Order constructor:
        /// </summary>
        public Order(string symbol, int quantity, OrderType order, DateTime time, decimal price = 0, string tag = "") {
            this.Time = time;
            this.Price = price;
            this.Type = order;
            this.Quantity = quantity;
            this.Symbol = symbol;
            this.Status = OrderStatus.None;
            this.Tag = tag;
            this.Duration = OrderDuration.GTC;
            this.BrokerId = 0;
        }
    }

} // End QC Namespace:
