/*
* QUANTCONNECT.COM: Transaction Manager
* Transaction Manager Processes and Verifes orders.
*/
/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

using QuantConnect.Logging;


namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Algorithm Transactions Manager - Recording Transactions
    /// </summary>
    public class SecurityTransactionManager {

        /******************************************************** 
        * CLASS PRIVATE VARIABLES
        *********************************************************/
        private SecurityManager Securities;
        private ConcurrentDictionary<int, Order> _orders = new ConcurrentDictionary<int, Order>();
        private ConcurrentQueue<Order> _orderQueue = new ConcurrentQueue<Order>();
        private ConcurrentDictionary<int, ConcurrentBag<OrderEvent>> _orderEvents = new ConcurrentDictionary<int, ConcurrentBag<OrderEvent>>();
        private Dictionary<DateTime, decimal> _transactionRecord = new Dictionary<DateTime, decimal>();
        private int _orderId = 1;
        private decimal _minimumOrderSize = 0;
        private int _minimumOrderQuantity = 1;

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm Transaction Class
        /// </summary>
        public SecurityTransactionManager(SecurityManager security)
        {
            //Private reference for processing transactions
            this.Securities = security;

            //Initialise the Order Cache -- Its a mirror of the TransactionHandler.
            this._orders = new ConcurrentDictionary<int, Order>();

            //Temporary Holding Queue of Orders to be Processed.
            this._orderQueue = new ConcurrentQueue<Order>();

            // Internal order events storage.
            this._orderEvents = new ConcurrentDictionary<int, ConcurrentBag<OrderEvent>>();

            //Interal storage for transaction records:
            this._transactionRecord = new Dictionary<DateTime, decimal>();
        }


        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Holding All Orders: Clone of the TransactionHandler.Orders
        /// -> Read only.
        /// </summary>
        public ConcurrentDictionary<int, Order> Orders 
        {
            get 
            {
                return _orders;
            }
            set
            {
                _orders = value;
            }
        }

        /// <summary>
        /// Temporary storage while waiting for orders to process.
        /// Processing Line for Orders Not Sent To Transaction Handler:
        /// </summary>
        public ConcurrentQueue<Order> OrderQueue
        {
            get
            {
                return _orderQueue;
            }
            set 
            {
                _orderQueue = value;
            }
        }

        /// <summary>
        /// New event from a partially-processed/pending order
        /// </summary>
        public ConcurrentDictionary<int, ConcurrentBag<OrderEvent>> OrderEvents
        {
            get
            {
                return _orderEvents;
            }
            set 
            {
                _orderEvents = value;
            }
        }

        /// <summary>
        /// Trade record of profits and losses for each trade statistics calculations
        /// </summary>
        public Dictionary<DateTime, decimal> TransactionRecord
        {
            get
            {
                return _transactionRecord;
            }
            set
            {
                _transactionRecord = value;
            }
        }

        /// <summary>
        /// Configurable Minimum Order Size to override bad orders, Default 0:
        /// </summary>
        public decimal MinimumOrderSize 
        {
            get 
            {
                return _minimumOrderSize;
            }
        }

        /// <summary>
        /// Configurable Minimum Order Quantity: Default 0
        /// </summary>
        public int MinimumOrderQuantity 
        {
            get 
            {
                return _minimumOrderQuantity;
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        
        /// <summary>
        /// Add an Order and return the Order ID or negative if an error.
        /// </summary>
        public virtual int AddOrder(Order order) 
        {
            try {
                //Ensure its flagged as a new order for the transaction handler.
                order.Id = _orderId++;
                order.Status = OrderStatus.New;

                //Add the order to the cache to monitor
                OrderQueue.Enqueue(order);

            } catch (Exception err) {
                Log.Error("Algorithm.Transaction.AddOrder(): " + err.Message);
            }
            return order.Id;
        }

        /// <summary>
        /// Update an order yet to be filled / stop / limit.
        /// </summary>
        /// <param name="order">Order to Update</param>
        /// <param name="portfolio"></param>
        /// <returns>id if the order we modified.</returns>
        public int UpdateOrder(Order order, SecurityPortfolioManager portfolio) 
        {
            try 
            {
                //Update the order from the behaviour
                int id = order.Id;
                order.Time = Securities[order.Symbol].Time;

                //Validate order:
                if (order.Price == 0 || order.Quantity == 0) return -1;

                if (_orders.ContainsKey(id))
                {
                    //-> If its already filled return false; can't be updated
                    if (_orders[id].Status == OrderStatus.Filled || _orders[id].Status == OrderStatus.Canceled)
                    {
                        return -5;
                    } 
                    else 
                    {
                        //Flag the order to be resubmitted.
                        order.Status = OrderStatus.Update;
                        _orders[id] = order;
                        //Send the order to transaction handler to be processed.
                        OrderQueue.Enqueue(order);
                    }
                } 
                else 
                {
                    //-> Its not in the orders cache, shouldn't get here
                    return -6;
                }
            } 
            catch (Exception err) 
            {
                Log.Error("Algorithm.Transactions.UpdateOrder(): " + err.Message);
                return -7;
            }
            return 0;
        }


        /// <summary>
        /// Remove this order from outstanding queue: its been filled or cancelled.
        /// </summary>
        /// <param name="orderId">Specific order id to remove</param>
        public virtual void RemoveOrder(int orderId) 
        {
            try
            {
                //Error check
                if (!Orders.ContainsKey(orderId)) 
                {
                    Log.Error("Security.Holdings.RemoveOutstandingOrder(): Cannot find this id.");
                    return;
                }

                if (Orders[orderId].Status != OrderStatus.Submitted) 
                {
                    Log.Error("Security.Holdings.RemoveOutstandingOrder(): Order already filled");
                    return;
                }

                Order orderToRemove = new Order("", 0, OrderType.Market, new DateTime());
                orderToRemove.Id = orderId;
                orderToRemove.Status = OrderStatus.Canceled;
                OrderQueue.Enqueue(orderToRemove);
            }
            catch (Exception err)
            {
                Log.Error("TransactionManager.RemoveOrder(): " + err.Message);
            }
        }

        /// <summary>
        /// Check if there is sufficient capital to execute this order.
        /// </summary>
        /// <param name="portfolio">Our portfolio</param>
        /// <param name="order">Order we're checking</param>
        /// <returns>True if suficient capital.</returns>
        public bool GetSufficientCapitalForOrder(SecurityPortfolioManager portfolio, Order order)
        {
            //First simple check, when don't hold stock, this will always increase portfolio regardless of direction
            if (Math.Abs(GetOrderRequiredBuyingPower(order)) > portfolio.GetBuyingPower(order.Symbol, order.Direction)) {
                //Log.Debug("Symbol: " + order.Symbol + " Direction: " + order.Direction.ToString() + " Quantity: " + order.Quantity);
                //Log.Debug("GetOrderRequiredBuyingPower(): " + Math.Abs(GetOrderRequiredBuyingPower(order)) + " PortfolioGetBuyingPower(): " + portfolio.GetBuyingPower(order.Symbol, order.Direction)); 
                return false;
            } else {
                return true;
            }
        }

        /// <summary>
        /// Using leverage property of security find the required cash for this order:
        /// </summary>
        /// <param name="order">Order to check</param>
        /// <returns>decimal cash required to purchase order</returns>
        private decimal GetOrderRequiredBuyingPower(Order order)
        {
            try {
                return Math.Abs(order.Value) / Securities[order.Symbol].Leverage;    
            } 
            catch(Exception err)
            {
                Log.Error("Security.TransactionManager.GetOrderRequiredBuyingPower(): " + err.Message);
            }
            //Prevent all orders if leverage is 0.
            return decimal.MaxValue;
        }


    } // End Algorithm Transaction Filling Classes


} // End QC Namespace
