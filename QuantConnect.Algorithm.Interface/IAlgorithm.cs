/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals, V0.1
 * Created by Jared Broad
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuantConnect  {

    /******************************************************** 
    * QUANTCONNECT PROJECT LIBRARIES
    *********************************************************/
    using QuantConnect.Securities;
    using QuantConnect.Models;

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Interface for Algorithm Class Libraries
    /// </summary>
    public interface IAlgorithm
    {

        /******************************************************** 
        * INTERFACE PROPERTIES:
        *********************************************************/
        /// <summary>
        /// Get/Set the Data Manager
        /// </summary>
        SubscriptionManager SubscriptionManager 
        {
            get;
            set;
        }

        /// <summary>
        /// Security Object Collection Class
        /// </summary>
        SecurityManager Securities 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Security Portfolio Management Class:
        /// </summary>
        SecurityPortfolioManager Portfolio 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Security Transaction Processing Class.
        /// </summary>
        SecurityTransactionManager Transactions
        { 
            get; 
            set;
        }

        /// <summary>
        /// Set a public name for the algorithm.
        /// </summary>
        string Name 
        {
            get;
            set;
        }

        /// <summary>
        /// Property indicating the transaction handler is currently processing an order and the algorithm should wait (syncrhonous order processing).
        /// </summary>
        bool ProcessingOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Get the current date/time.
        /// </summary>
        DateTime Time 
        {
            get;
        }

        /// <summary>
        /// Get Requested Backtest Start Date
        /// </summary>
        DateTime StartDate 
        {
            get;
        }

        /// <summary>
        /// Get Requested Backtest End Date
        /// </summary>
        DateTime EndDate 
        {
            get;
        }

        /// <summary>
        /// AlgorithmId for the backtest
        /// </summary>
        string AlgorithmId 
        {
            get;
        }

        /// <summary>
        /// Accessor for Filled Orders:
        /// </summary>
        ConcurrentDictionary<int, Order> Orders 
        {
            get;
        }

        /// <summary>
        /// Run Backtest Mode for the algorithm: Automatic, Parallel or Series.
        /// </summary>
        RunMode RunMode 
        {
            get;
        }

        /// <summary>
        /// Indicator if the algorithm has been initialised already. When this is true cash and securities cannot be modified.
        /// </summary>
        bool Locked 
        {
            get;
        }

        /// <summary>
        /// Debug messages from the strategy:
        /// </summary>
        List<string> DebugMessages
        {
            get;
            set;
        }

        /// <summary>
        /// Error messages from the strategy:
        /// </summary>
        List<string> ErrorMessages
        {
            get;
            set;
        }

        /// <summary>
        /// Log messages from the strategy:
        /// </summary>
        List<string> LogMessages
        {
            get;
            set;
        }

        /******************************************************** 
        * INTERFACE METHODS
        *********************************************************/
        /// <summary>
        /// Initialise the Algorithm and Prepare Required Data:
        /// </summary>
        void Initialize();

        // <summary>
        // v1.0 Handler for Tick Events [DEPRECATED June-2014]
        // </summary>
        // <param name="ticks">Tick Data Packet</param>
        //void OnTick(Dictionary<string, List<Tick>> ticks);

        // <summary>
        // v1.0 Handler for TradeBar Events [DEPRECATED June-2014]
        // </summary>
        // <param name="tradebars">TradeBar Data Packet</param>
        //void OnTradeBar(Dictionary<string, TradeBar> tradebars);

        // <summary>
        // v2.0 Handler for Generic Data Events
        // </summary>
        //void OnData(Ticks ticks);
        //void OnData(TradeBars tradebars);

        /// <summary>
        /// Send debug message
        /// </summary>
        /// <param name="message"></param>
        void Debug(string message);

        /// <summary>
        /// Save entry to the Log 
        /// </summary>
        /// <param name="message">String message</param>
        void Log(string message);

        /// <summary>
        /// Send an error message for the algorithm
        /// </summary>
        /// <param name="message">String message</param>
        void Error(string message);

        /// <summary>
        /// Call this method at the end of each day of data.
        /// </summary>
        void OnEndOfDay();

        /// <summary>
        /// Call this event at the end of the algorithm running.
        /// </summary>
        void OnEndOfAlgorithm();

        /// <summary>
        /// EXPERTS ONLY:: [-!-Async Code-!-] 
        /// New order event handler: on order status changes (filled, partially filled, cancelled etc).
        /// </summary>
        /// <param name="newEvent">Event information</param>
        void OnOrderEvent(OrderEvent newEvent);

        /// <summary>
        /// Set the DateTime Frontier: This is the master time and is 
        /// </summary>
        /// <param name="time"></param>
        void SetDateTime(DateTime time);

        /// <summary>
        /// Set the run mode of the algorithm: series, parallel or automatic.
        /// </summary>
        /// <param name="mode">Run mode to select, default Automatic</param>
        void SetRunMode(RunMode mode = RunMode.Automatic);

        /// <summary>
        /// Set the start date of the backtest period. This must be within available data.
        /// </summary>
        void SetStartDate(int year, int month, int day);

        /// <summary>
        /// Alias for SetStartDate() which accepts DateTime Class
        /// </summary>
        /// <param name="start">DateTime Object to Start the Algorithm</param>
        void SetStartDate(DateTime start);

        /// <summary>
        /// Set the end Backtest date for the algorithm. This must be within available data.
        /// </summary>
        void SetEndDate(int year, int month, int day);

        /// <summary>
        /// Alias for SetStartDate() which accepts DateTime Object
        /// </summary>
        /// <param name="end">DateTime End Date for Analysis</param>
        void SetEndDate(DateTime end);

        /// <summary>
        /// Set the algorithm Id for this backtest or live run. This can be used to identify the order and equity records.
        /// </summary>
        /// <param name="algorithmId">unique 32 character identifier for backtest or live server</param>
        void SetAlgorithmId(string algorithmId);

        /// <summary>
        /// Set the algorithm as initialized and locked. No more cash or security changes.
        /// </summary>
        void SetLocked();

        /// <summary>
        /// Get the chart updates since the last request:
        /// </summary>
        /// <returns>List of Chart Updates</returns>
        List<Chart> GetChartUpdates();

        /// <summary>
        /// Add a chart to the internal algorithm list.
        /// </summary>
        /// <param name="chart">Chart object to add</param>
        void AddChart(Chart chart);

        /// <summary>
        /// Set a required MarketType-symbol and resolution for algorithm
        /// </summary>
        /// <param name="securityType">MarketType Enum: Equity, Commodity, FOREX or Future</param>
        /// <param name="symbol">Symbol Representation of the MarketType, e.g. AAPL</param>
        /// <param name="resolution">Resolution of the MarketType required: MarketData, Second or Minute</param>
        /// <param name="fillDataForward">If true, returns the last available data even if none in that timeslice.</param>
        /// <param name="leverage">leverage for this security</param>
        /// <param name="extendedMarketHours">ExtendedMarketHours send in data from 4am - 8pm, not used for FOREX</param>
        void AddSecurity(SecurityType securityType, string symbol, Resolution resolution, bool fillDataForward, decimal leverage, bool extendedMarketHours);

        /// <summary>
        /// Set the starting capital for the strategy
        /// </summary>
        /// <param name="startingCash">decimal starting capital, default $100,000</param>
        void SetCash(decimal startingCash);

        /// <summary>
        /// Send an order to the transaction manager.
        /// </summary>
        /// <param name="symbol">Symbol we want to purchase</param>
        /// <param name="quantity">Quantity to buy, + is long, - short.</param>
        /// <param name="type">Market, Limit or Stop Order</param>
        /// <param name="asynchronous">Don't wait for the response, just submit order and move on.</param>
        /// <returns>Integer Order ID.</returns>
        int Order(string symbol, int quantity, OrderType type = OrderType.Market, bool asynchronous = false);

        /// <summary>
        /// Liquidate your portfolio holdings:
        /// </summary>
        /// <param name="symbolToLiquidate">Specific asset to liquidate, defaults to all.</param>
        /// <returns>list of order ids</returns>
        List<int> Liquidate(string symbolToLiquidate = "");

        /// <summary>
        /// Terminate the algorithm on exiting the current event processor. 
        /// If have holdings at the end of the algorithm/day they will be liquidated at market prices.
        /// If running a series analysis this command skips the current day (and doesn't liquidate).
        /// </summary>
        /// <param name="message">Exit message</param>
        void Quit(string message = "");

        /// <summary>
        /// Set the quit flag true / false.
        /// </summary>
        /// <param name="quit">When true quits the algorithm event loop for this day</param>
        void SetQuit(bool quit);

        /// <summary>
        /// Get the quit flag state. 
        /// </summary>
        /// <returns>Boolean quit flag</returns>
        bool GetQuit();
    }

}
