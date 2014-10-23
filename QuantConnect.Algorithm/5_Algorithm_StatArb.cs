using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using QuantConnect.Securities;
using QuantConnect.Models;
using Accord.Math.Optimization;
using System.Globalization;

namespace QuantConnect
{
    /// <summary>
    /// 1.0 BASIC TEMPLATE ALGORITHM
    /// 
    /// This is a bare bones minimum example to run a backtest inside QuantConnect. 
    /// 
    /// Everything inside the QuantConnect.Algorithm project is included in your DLL
    /// 
    /// </summary>
    public class NickStatArb : QCAlgorithm
    {
        string[] symbols = new string[] { "CL1", "CL2" };

        //int balRisk = 3000;
        int numBars = 10;
        double cLevel = 0.038;
        double resid = 0, mult;
        PriceQueue prices = new PriceQueue(10);

        public override void Initialize()
        {
            SetStartDate(1984, 01, 01);
            SetEndDate(DateTime.Now.Date.AddDays(-1));
            SetCash(5000);
            for (int i = 0; i < symbols.Length; i++)
            {
                AddData<Futures>(symbols[i]);
            }

            decimal leverage = 1;

            Securities["CL1"] = new Security("CL1", SecurityType.Base, Resolution.Minute, false, leverage, false, false);
            Securities["CL2"] = new Security("CL2", SecurityType.Base, Resolution.Minute, false, leverage, false, false);

            Securities["CL1"].Model = new CustomModel(this);
            Securities["CL2"].Model = new CustomModel(this);
        }

        public void OnData(Futures data1)
        {
            if (Time.Year == 2006) System.Diagnostics.Debugger.Break();

            if (data1.Symbol == symbols[0])
            {
                prices.Push1(data1.Open);
            }

            if (data1.Symbol == symbols[1])
            {
                prices.Push2(data1.Open);
            }

            if (prices.QLength1 == numBars && prices.QLength2 == numBars)
            {
                mult = BrentSearch.FindRoot(prices.multFunc, -2, 2, 1e-8);
                resid = prices.Open1 - (mult * prices.Open2);

                if (Portfolio[symbols[0]].IsLong && resid > 0)
                {
                    Order(symbols[0], -1);
                    Order(symbols[1], 1);
                    //Debug(data1.Time.ToString()+" Resid: "+resid.ToString()+" Close BuySell-Open1: "+prices.Open1.ToString()+" Open2: "+prices.Open2.ToString());
                }

                if (Portfolio[symbols[0]].IsShort && resid < 0)
                {
                    Order(symbols[0], 1);
                    Order(symbols[1], -1);
                    //Debug(data1.Time.ToString()+" Resid: "+resid.ToString()+" Close SellBuy-Open1: "+prices.Open1.ToString()+" Open2: "+prices.Open2.ToString());
                }

                if (!Portfolio[symbols[0]].HoldStock && !Portfolio[symbols[1]].HoldStock)
                {
                    if (resid > cLevel)
                    {
                        //int tradeSize = (int)(Portfolio.Cash / balRisk);
                        Order(symbols[0], -1);
                        Order(symbols[1], 1);
                        //Debug(data1.Time.ToString()+" Resid: "+resid.ToString()+" SellBuy-Open1: "+prices.Open1.ToString()+" Open2: "+prices.Open2.ToString());
                    }

                    if (resid < -cLevel)
                    {
                        //int tradeSize = (int)(Portfolio.Cash / balRisk);
                        Order(symbols[0], 1);
                        Order(symbols[1], -1);
                        //Debug(data1.Time.ToString()+" Resid: "+resid.ToString()+" BuySell-Open1: "+prices.Open1.ToString()+" Open2: "+prices.Open2.ToString());
                    }
                }
            }
        }
    }


    public class PriceQueue
    {
        private Queue<double> prices1, prices2;
        private int numBars;
        public Func<double, double> multFunc;

        public PriceQueue(int numBars1)
        {
            numBars = numBars1;
            this.prices1 = new Queue<double>(numBars);
            this.prices2 = new Queue<double>(numBars);
            this.multFunc = (x) => multCalc(x);
        }

        public int QLength1
        {
            get
            {
                return prices1.Count;
            }
        }

        public int QLength2
        {
            get
            {
                return prices2.Count;
            }
        }

        public void Push1(decimal newPrice1)
        {
            if (prices1.Count >= numBars)
            {
                prices1.Dequeue();
            }
            prices1.Enqueue((double)newPrice1);
        }

        public void Push2(decimal newPrice2)
        {
            if (prices2.Count >= numBars)
            {
                prices2.Dequeue();
            }
            prices2.Enqueue((double)newPrice2);
        }

        public double multCalc(double x)
        {
            double tempResid = 0;
            for (int i = 0; i < numBars; i++)
            {
                tempResid += Math.Abs(prices1.ElementAt(i) - (x * prices2.ElementAt(i)));
            }
            return tempResid;
        }

        public double Open1
        {
            get
            {
                return prices1.ElementAt(numBars - 1);
            }
        }

        public double Open2
        {
            get
            {
                return prices2.ElementAt(numBars - 1);
            }
        }
    }



    public class Futures : BaseData
    {
        public decimal Open = 0;
        public decimal High = 0;
        public decimal Low = 0;
        public decimal Close = 0;

        public Futures()
        {

        }

        public override string GetSource(SubscriptionDataConfig config, DateTime date, DataFeedEndpoint datafeed)
        {
            string sourceURL = "";

            if (config.Symbol == "CL1")
            {
                sourceURL = "https://www.dropbox.com/s/cf1c9wehmc9q8ar/CL1.csv?dl=1";
            }

            if (config.Symbol == "CL2")
            {
                sourceURL = "https://www.dropbox.com/s/uitywhz18qkiq1m/CL2.csv?dl=1";
            }

            return sourceURL;
        }

        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, DataFeedEndpoint datafeed)
        {
            Futures contract = new Futures();

            try
            {
                string[] data = line.Split(',');
                contract.Time = DateTime.ParseExact(data[0], "M/d/yyyy", CultureInfo.InvariantCulture);
                contract.Open = Convert.ToDecimal(data[1]);
                contract.High = Convert.ToDecimal(data[2]);
                contract.Low = Convert.ToDecimal(data[3]);
                contract.Close = Convert.ToDecimal(data[4]);
                contract.Symbol = config.Symbol;
                contract.Value = contract.Close;

            }
            catch { /* Do nothing, skip first title row */ }

            return contract;
        }
    }

    public class CustomModel : ISecurityTransactionModel
    {
        QCAlgorithm algo;

        public CustomModel(QCAlgorithm algoNamespace)
        {
            //Setup your custom model.
            algo = algoNamespace;
        }

        //FILL THE ORDER
        public virtual void Fill(Security vehicle, ref Order order)
        {
            try
            {
                switch (order.Type)
                {
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
            }
            catch (Exception err)
            {
                algo.Error("CustomTransactionModel.TransOrderDirection.Fill():" + err.Message);
            }
        }

        //TRANSACTION FEE:
        public virtual decimal GetOrderFee(decimal quantity, decimal price)
        {
            return .002m;
        }

        //SLIPPAGE MODEL:
        public virtual decimal GetSlippageApproximation(Security security, Order order)
        {
            decimal slippage = .01m;
            //Calc slippage here: Based on order and security.
            return slippage;
        }


        //MARKET FILL MODEL:
        public virtual void MarketFill(Security security, ref Order order)
        {

            try
            {
                //Calculate the model slippage: e.g. 0.01c
                decimal slip = GetSlippageApproximation(security, order);

                switch (order.Direction)
                {
                    case OrderDirection.Buy:
                        order.Price = security.Price;
                        order.Price += slip;
                        break;
                    case OrderDirection.Sell:
                        order.Price = security.Price;
                        order.Price -= slip;
                        break;
                }

                //Market orders fill instantly.
                order.Status = OrderStatus.Filled;
            }
            catch (Exception err)
            {
                algo.Error("CustomTransactionModel.TransOrderDirection.MarketFill():" + err.Message);
            }
        }


        //STOP FILL MODEL - 
        public virtual void StopFill(Security security, ref Order order)
        {
            try
            {
                //If its cancelled don't need anymore checks:
                if (order.Status == OrderStatus.Canceled) return;

                //Check if the Stop Order was filled: opposite to a limit order
                if (order.Direction == OrderDirection.Sell)
                {
                    //-> 1.1 Sell Stop: If Price below setpoint, Sell:
                    if (security.Price > order.Price)
                    {
                        order.Status = OrderStatus.Filled;
                    }
                }
                else if (order.Direction == OrderDirection.Buy)
                {
                    //-> 1.2 Buy Stop: If Price Above Setpoint, Buy:
                    if (security.Price < order.Price)
                    {
                        order.Status = OrderStatus.Filled;
                    }
                }
            }
            catch (Exception err)
            {
                algo.Error("CustomTransactionModel.TransOrderDirection.StopFill():" + err.Message);
            }
        }


        //LIMIT FILL MODEL:
        public virtual void LimitFill(Security security, ref Order order)
        {

            //Initialise;
            decimal marketDataMinPrice = 0;
            decimal marketDataMaxPrice = 0;

            try
            {
                //If its cancelled don't need anymore checks:
                if (order.Status == OrderStatus.Canceled) return;

                //Depending on the resolution, return different data types:
                Futures contract = security.GetLastData() as Futures;

                if (contract == null)
                {
                    //Shouldnt happen.
                }

                marketDataMinPrice = contract.Low;
                marketDataMaxPrice = contract.High;

                //Valid Live/Model Order: 
                if (order.Direction == OrderDirection.Buy)
                {
                    //Buy limit seeks lowest price
                    if (marketDataMinPrice < order.Price)
                    {
                        order.Status = OrderStatus.Filled;
                    }

                }
                else if (order.Direction == OrderDirection.Sell)
                {
                    //Sell limit seeks highest price possible
                    if (marketDataMaxPrice > order.Price)
                    {
                        order.Status = OrderStatus.Filled;
                    }
                }
            }
            catch (Exception err)
            {
                algo.Error("CustomTransactionModel.TransOrderDirection.LimitFill():" + err.Message);
            }
        }
    } // End Algorithm Transaction Filling Classes


}