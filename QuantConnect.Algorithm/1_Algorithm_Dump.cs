using System;
using System.Collections;
using System.Collections.Generic;
using QuantConnect.Securities;
using QuantConnect.Models;

namespace QuantConnect
{

    /// <summary>
    /// Demonstration of QuantConnect Custom Charting: this algorithm plots 6 charts; 3 are by default with the new 2.0 LEAN engine:
    ///
    /// 1. Strategy Equity (Default) - Your current cash position
    //  1.1     With a second line to chart to demonstrate a stacked equity chart.
    ///
    /// 2. Daily Performance (Default) - Daily gains and losses in percent
    ///
    /// 3. Stock Plotter (Default) - Trades overlayed on asset price (equities only)
    ///
    /// 4. "Math Waves Overlay" - Sine and Cosine charts on a separate custom plot, overlayed on each other.
    ///
    /// 5. Stock Plotter (Custom, Manual) - Trades on asset price, manually constructed.
    ///
    //  6. Asset Pricing (Custom, Stacked) - Stacked charts  
    /// </summary>
    public class AlgoTest : QCAlgorithm
    {
        //Initializers:
        double dayCount = 0;
        TradeBars prices = new TradeBars();
        bool tradedToday = false;

        /// <summary>
        /// Setup our custom charting objects:
        /// Defaults to $100k cash from 2008 until Today. 
        /// See also SetCash(), SetStartDate() and SetEndDate().
        /// </summary>
        public override void Initialize()
        {
            AddSecurity(SecurityType.Equity, "SPY", resolution: Resolution.Minute);
            AddSecurity(SecurityType.Equity, "MSFT", resolution: Resolution.Second);
            AddSecurity(SecurityType.Forex, "EURUSD", resolution: Resolution.Tick);

            //#5 - Stock Plotter with Trades
            Chart plotter = new Chart("Plotter", ChartType.Overlay);
            plotter.AddSeries(new Series("Buy", SeriesType.Scatter));
            plotter.AddSeries(new Series("Sell", SeriesType.Scatter));
            AddChart(plotter);

            //#6 - Asset Pricing, Stacked:
            Chart assets = new Chart("Assets", ChartType.Stacked);
            assets.AddSeries(new Series("SPY", SeriesType.Candle));
            assets.AddSeries(new Series("MSFT", SeriesType.Candle));
            AddChart(assets);   //Don't forget to add the chart to the algorithm.
        }

        /// <summary>
        /// New Trade Bar Event Handler
        /// </summary>
        public void OnData(TradeBars data)
        {
            try
            {
                //Save price data:
                prices = data;

                //Because we're doing both equities and FX; wait for equities market open to trade:
                if (Securities["MSFT"].Exchange.ExchangeOpen == false) return;

                //Every 7th day go long everything:
                if (!Portfolio.Invested && (dayCount % 7 == 0) && !tradedToday)
                {
                    SetHoldings("SPY", 0.25);
                    SetHoldings("MSFT", 0.25);
                    tradedToday = true;
                }

                //Every 13th day close out portfolio:
                if (Portfolio.Invested && (dayCount % 13 == 0) && !tradedToday)
                {
                    Liquidate();
                    tradedToday = true;
                }
            }
            catch (Exception err)
            {
                Error("OnData Err: " + err.Message);
            }
        }


        public override void OnEndOfDay()
        {
            try
            {
                //Track # of Days:
                dayCount++;
                tradedToday = false;

                //#1 "Strategy Equity" plotted automatically (Stacked, Default)
                //#2 "Daily performance" plotted automatically (Stacked, Default)
                //#3 "Stock Plotter" created automatically for every asset with orders (Equities, Overlayed)

                //#1.1 is stacked on top of Strategy equity.
                Plot("Strategy Equity", "Portfolio", Portfolio.TotalPortfolioValue);

                //#4 Math Charts - Without initialization defaults to overlayed lines:
                Plot("Math Waves Overlay", "Sine", Math.Sin((2 * Math.PI) * ((double)Time.DayOfYear / 120)));
                Plot("Math Waves Overlay", "Cosine", Math.Sin((2 * Math.PI) * ((double)Time.DayOfYear / 120) + Math.PI * .5));

                //#6 Stacked Candle Plots of Assets
                if (prices.ContainsKey("MSFT")) Plot("Assets", "MSFT", prices["MSFT"].Price);
                if (prices.ContainsKey("SPY")) Plot("Assets", "SPY", prices["SPY"].Price);
            }
            catch (Exception err)
            {
                Error("OnEndOfDay Err:" + err.Message);
            }
        }
    }

}