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
    * ORDER EVENT CLASS DEFINITION
    *********************************************************/
    /// <summary>
    /// Order Event - Messaging class signifying a change in an order state. 
    /// </summary>
    public class OrderEvent {

        /// <summary>
        /// Id of the order this event comes from.
        /// </summary>
        public int OrderId;

        /// <summary>
        /// Easy access to the order symbol associated with this event.
        /// </summary>
        public string Symbol;

        /// <summary>
        /// Status message of the order.
        /// </summary>
        public OrderStatus Status;

        /// <summary>
        /// Fill price information about the order
        /// </summary>
        public decimal FillPrice;

        /// <summary>
        /// Number of shares of the order that was filled in this event.
        /// </summary>
        public int FillQuantity;


        /// <summary>
        /// Public Property Absolute Getter of Quantity -Filled
        /// </summary>
        public int AbsoluteFillQuantity 
        {
            get 
            {
                return Math.Abs(FillQuantity);
            }
        }

        /// <summary>
        /// Order direction.
        /// </summary>
        public OrderDirection Direction
        {
            get
            {
                return (FillQuantity > 0) ? OrderDirection.Buy : OrderDirection.Sell;
            }
        }

        /// <summary>
        /// Any message from the exchange.
        /// </summary>
        public string Message;

        /// <summary>
        /// Order Constructor.
        /// </summary>
        /// <param name="id">Id of the parent order</param>
        /// <param name="symbol">Asset Symbol</param>
        /// <param name="status">Status of the order</param>
        /// <param name="fillPrice">Fill price information if applicable.</param>
        /// <param name="fillQuantity">Fill quantity</param>
        /// <param name="message">Message from the exchange</param>
        public OrderEvent(int id = 0, string symbol = "", OrderStatus status = OrderStatus.None, decimal fillPrice = 0, int fillQuantity = 0, string message = "")
        {
            this.OrderId = id;
            this.Status = status;
            this.FillPrice = fillPrice;
            this.Message = message;
            this.FillQuantity = fillQuantity;
            this.Symbol = symbol;
        }

        /// <summary>
        /// Helper Constructor using Order to Initialize.
        /// </summary>
        /// <param name="order">Order for this order status</param>
        /// <param name="message">Message from exchange or QC.</param>
        public OrderEvent(Order order, string message = "") 
        {
            this.OrderId = order.Id;
            this.Status = order.Status;
            this.Message = message;
            this.Symbol = order.Symbol;

            //Initialize to zero, manually set fill quantity
            this.FillQuantity = 0;
            this.FillPrice = 0;
        }
    }

} // End QC Namespace:
