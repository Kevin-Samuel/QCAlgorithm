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
        private int _orderId = 1;
        private decimal _minimumOrderSize = 0;
        private int _minimumOrderQuantity = 1;

        /******************************************************** 
        * CLASS PUBLIC VARIABLES
        *********************************************************/
        /// <summary>
        /// Processing Line for Orders Not Sent To Transaction Handler:
        /// </summary>
        public ConcurrentQueue<Order> OrderQueue = new ConcurrentQueue<Order>();

        /// <summary>
        /// Trade record of profits and losses for each trade statistics calculations
        /// </summary>
        public Dictionary<DateTime, decimal> TransactionRecord = new Dictionary<DateTime, decimal>();

        /******************************************************** 
        * CLASS CONSTRUCTOR
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm Transaction Class
        /// </summary>
        public SecurityTransactionManager(SecurityManager security) {

            //Private reference for processing transactions
            this.Securities = security;

            //Initialise the Order Cache -- Its a mirror of the TransactionHandler.
            this._orders = new ConcurrentDictionary<int, Order>();

            //Temporary Holding Queue of Orders to be Processed.
            this.OrderQueue = new ConcurrentQueue<Order>();
        }


        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Holding All Orders: Clone of the TransactionHandler.Orders
        /// -> Read only.
        /// </summary>
        public ConcurrentDictionary<int, Order> Orders {
            get {
                return _orders;
            }
        }

        /// <summary>
        /// Configurable Minimum Order Size to override bad orders, Default 0:
        /// </summary>
        public decimal MinimumOrderSize {
            get {
                return _minimumOrderSize;
            }
        }

        /// <summary>
        /// Configurable Minimum Order Quantity: Default 0
        /// </summary>
        public int MinimumOrderQuantity {
            get {
                return _minimumOrderQuantity;
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Set the order cache, 
        /// </summary>
        /// <param name="orders">New orders cache</param>
        public void SetOrderCache(ConcurrentDictionary<int, Order> orders) 
        {
            _orders = orders;
        }
        
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
            try {
                //Update the order from the behaviour
                int id = order.Id;
                order.Time = Securities[order.Symbol].Time;

                //Run through a list of prepurchase checks, if any are false stop the transaction
                int orderError = ValidateOrder(order, portfolio, order.Time, int.MaxValue, order.Price);
                if (orderError < 0) {
                    return orderError;
                }

                if (_orders.ContainsKey(id))
                {
                    //-> If its already filled return false; can't be updated
                    if (_orders[id].Status == OrderStatus.Filled || _orders[id].Status == OrderStatus.Canceled)
                    {
                        return -5;
                    } else {
                        //Flag the order to be resubmitted.
                        order.Status = OrderStatus.Update;
                        _orders[id] = order;
                        //Send the order to transaction handler to be processed.
                        OrderQueue.Enqueue(order);
                    }
                } else {
                    //-> Its not in the orders cache, shouldn't get here
                    return -6;
                }
            } catch (Exception err) {
                Log.Error("Algorithm.Transactions.UpdateOrder(): " + err.Message);
                return -7;
            }
            return 0;
        }

        /// <summary>
        /// Scan through all the order events and update the user's portfolio
        /// </summary>
        /// <returns>.</returns>
        public virtual void ProcessOrderEvents(ConcurrentQueue<OrderEvent> orderEvents, SecurityPortfolioManager portfolio, int maxOrders, bool skipValidations = false) 
        {
            int orderEventsLoopCounter = 0;
            //Initialize:
            while (orderEvents.Count > 0 && orderEventsLoopCounter < 10000)
            {
                OrderEvent orderData;
                if (orderEvents.TryDequeue(out orderData)) 
                {
                    Order order = _orders[orderData.Id];

                    //Update the order:
                    order.Price = orderData.FillPrice;
                    order.Status = orderData.Status;
                    order.Time = Securities[order.Symbol].Time;

                    //Update the portfolio.
                    if (order.Status == OrderStatus.Filled)
                    {
                        portfolio.ProcessFill(order);
                    }

                    //Set it back:
                    _orders[orderData.Id] = order;
                }
                //Log.Debug("SecurityTransactionManager.ProcessOrderFillEvents(): Processed Order Event.");
            }
        }

        /// <summary>
        /// Remove this order from outstanding queue: its been filled or cancelled.
        /// </summary>
        /// <param name="orderId">Specific order id to remove</param>
        public virtual void RemoveOrder(int orderId) {
            try
            {
                //Error check
                if (!Orders.ContainsKey(orderId)) {
                    Log.Error("Security.Holdings.RemoveOutstandingOrder(): Cannot find this id.");
                    return;
                }

                if (Orders[orderId].Status != OrderStatus.Submitted) {
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
        /// Validate the transOrderDirection is a sensible choice, factoring in basic limits.
        /// </summary>
        /// <param name="order">Order to Validate</param>
        /// <param name="portfolio">Security portfolio object we're working on.</param>
        /// <param name="time">Current actual time</param>
        /// <param name="maxOrders">Maximum orders per day/period before rejecting.</param>
        /// <param name="price">Current actual price of security</param>
        /// <returns>If negative its an error, or if 0 no problems.</returns>
        public int ValidateOrder(Order order, SecurityPortfolioManager portfolio, DateTime time, int maxOrders = 50, decimal price = 0) 
        {
            //-1: Order quantity must not be zero
            if (order.Quantity == 0 || order.Direction == OrderDirection.Hold) return -1;
            //-2: There is no data yet for this security - please wait for data (market order price not available yet)
            //if (order.Price <= 0) return -2; // Not valid anymore with custom data - need to accept negative data.
            //-3: Attempting market order outside of market hours
            if (!Securities[order.Symbol].Exchange.ExchangeOpen && order.Type == OrderType.Market) return -3;
            //-4: Insufficient capital to execute order
            if (GetSufficientCapitalForOrder(portfolio, order) == false) return -4;
            //-5: Exceeded maximum allowed orders for one analysis period.
            if (Orders.Count > maxOrders) return -5;
            //-6: Order timestamp error. Order appears to be executing in the future
            if (order.Time > time) return -6;
            return 0;
        }

        /// <summary>
        /// Check if there is sufficient capital to execute this order.
        /// </summary>
        /// <param name="portfolio">Our portfolio</param>
        /// <param name="order">Order we're checking</param>
        /// <returns>True if suficient capital.</returns>
        private bool GetSufficientCapitalForOrder(SecurityPortfolioManager portfolio, Order order)
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

        /// <summary>
        /// Given this portfolio and order, what would the final portfolio holdings be if it were filled.
        /// </summary>
        /// <param name="portfolio">Portfolio we're running</param>
        /// <param name="order">Order requested to process </param>
        /// <returns>decimal final holdings </returns>
        private decimal GetExpectedFinalHoldings(SecurityPortfolioManager portfolio, Order order)
        {
            decimal expectedFinalHoldings = 0;

            if (portfolio.TotalAbsoluteHoldings > 0) {
                foreach (Security company in Securities.Values) 
                {
                    if (order.Symbol == company.Symbol) 
                    {
                        //If the same holding, we must check if its long or short.
                        expectedFinalHoldings += Math.Abs(company.Holdings.HoldingValue + (order.Price * (decimal)order.Quantity));
                        //Log.Debug("HOLDINGS: " + company.Holdings.HoldingValue + " - " + "ORDER: (P: " + order.Price + " Q:" + order.Quantity + ") EXPECTED FINAL HOLDINGS: " + expectedFinalHoldings + " BUYING POWER: " + portfolio.GetBuyingPower(order.Symbol));
                    } else {
                        //If not the same asset, then just add the absolute holding to the final total:
                        expectedFinalHoldings += company.Holdings.AbsoluteHoldings;
                    }
                }
            } else {
                //First purchase: just make calc abs order size:
                expectedFinalHoldings = (order.Price * (decimal)order.Quantity);
            }

            return expectedFinalHoldings;
        }

    } // End Algorithm Transaction Filling Classes


} // End QC Namespace
