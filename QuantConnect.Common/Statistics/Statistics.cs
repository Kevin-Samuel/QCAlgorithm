/*
*   QUANTCONNECT.COM - 
*   QC.Statistics - Generate statistics on the equity and orders
*/

/**********************************************************
 * USING NAMESPACES
 **********************************************************/
using System;
using System.Linq;
using System.Net;
using System.IO;
using System.Collections.Generic;
using MathNet.Numerics.Statistics;
using MathNet;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Properties;

//QuantConnect Project Libraries:
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect
{

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// QuantConnect Summary Statistics Class
    /// </summary>
    public class Statistics
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        private static DateTime _benchmarkAge = new DateTime();
        private static SortedDictionary<DateTime, decimal> _benchmark = new SortedDictionary<DateTime, decimal>();

        /// <summary>
        /// Static S-P500 Benchmark
        /// </summary>
        public static SortedDictionary<DateTime, decimal> Benchmark
        {
            get
            {
                if (_benchmark.Count == 0 || (DateTime.Now - _benchmarkAge) > TimeSpan.FromDays(1))
                {
                    //Fetch & Set Benchmark:
                    _benchmark.Clear();
                    string url = "http://real-chart.finance.yahoo.com/table.csv?s=SPY&a=11&b=31&c=1997&d=" + (DateTime.Now.Month - 1) + "&e=" + DateTime.Now.Day + "&f=" + DateTime.Now.Year + "&g=d&ignore=.csv";
                    using (WebClient net = new WebClient())
                    {
                        string data = net.DownloadString(url);
                        bool first = true;
                        using (var sr = new StreamReader(data.ToStream()))
                        {
                            while (sr.Peek() >= 0)
                            {
                                string line = sr.ReadLine();
                                if (first) { first = false; continue; }
                                string[] csv = line.Split(',');
                                _benchmark.Add(DateTime.Parse(csv[0]), Convert.ToDecimal(csv[6]));
                            }
                        }
                    }
                    _benchmarkAge = DateTime.Now;
                }
                return _benchmark;
            }
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Convert the charting data into an equity array:
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        private static SortedDictionary<DateTime, decimal> ChartPointToDictionary(IEnumerable<ChartPoint> points)
        {
            var dictionary = new SortedDictionary<DateTime, decimal>();
            try
            {
                foreach (ChartPoint point in points)
                {
                    DateTime x = Time.UnixTimeStampToDateTime(point.x);
                    if (!dictionary.ContainsKey(x))
                    {
                        dictionary.Add(x, point.y);
                    }
                    else
                    {
                        dictionary[x] = point.y;
                    }
                }
            }
            catch (Exception err)
            {
                Log.Error("Statistics.ChartPointToDictionary(): " + err.Message);
            }
            return dictionary;
        }


        /// <summary>
        /// Run a full set of orders and return a 
        /// </summary>
        /// <param name="pointsEquity">Equity value over time.</param>
        /// <param name="pointsPerformance"> Daily performance</param>
        /// <param name="profitLoss">profit loss from trades</param>
        /// <param name="startingCash">Amount of starting cash in USD </param>
        /// <returns>Statistics Array, Broken into Annual Periods</returns>
        /// <param name="tradingDaysPerYear">Number of trading days per year</param>
        public static Dictionary<string, string> Generate(IEnumerable<ChartPoint> pointsEquity, SortedDictionary<DateTime, decimal> profitLoss, IEnumerable<ChartPoint> pointsPerformance, decimal startingCash, double tradingDaysPerYear = 252)
        {
            //Initialise the response:
            double riskFreeRate = 0;
            decimal totalTrades = 0;
            decimal totalWins = 0;
            decimal totalLosses = 0;
            decimal averageWin = 0;
            decimal averageLoss = 0;
            decimal averageWinRatio = 0;
            decimal winRate = 0;
            decimal lossRate = 0;
            decimal totalNetProfit = 0;
            decimal averageAnnualReturn = 0;
            double fractionOfYears = 1;
            decimal profitLossValue = 0, runningCash = startingCash;
            decimal algoCompoundingPerformance = 0;
            decimal finalBenchmarkCash = 0;
            decimal benchCompoundingPerformance = 0;
            List<int> years = new List<int>();
            SortedDictionary<int, int> annualTrades = new SortedDictionary<int, int>();
            SortedDictionary<int, int> annualWins = new SortedDictionary<int, int>();
            SortedDictionary<int, int> annualLosses = new SortedDictionary<int, int>();
            SortedDictionary<int, decimal> annualLossTotal = new SortedDictionary<int, decimal>();
            SortedDictionary<int, decimal> annualWinTotal = new SortedDictionary<int, decimal>();
            SortedDictionary<int, decimal> annualNetProfit = new SortedDictionary<int, decimal>();
            Dictionary<string, string> statistics = new Dictionary<string, string>();
            DateTime dtPrevious = new DateTime();
            List<double> listPerformance = new List<double>();
            List<double> listBenchmark = new List<double>();
            SortedDictionary<DateTime, decimal> equity = new SortedDictionary<DateTime, decimal>();
            SortedDictionary<DateTime, decimal> performance = new SortedDictionary<DateTime, decimal>();

            try
            {
                //Get array versions of the performance:
                performance = ChartPointToDictionary(pointsPerformance);
                equity = ChartPointToDictionary(pointsEquity);
                performance.Values.ToList().ForEach(i => listPerformance.Add((double)(i / 100)));

                //Get benchmark performance array for same period:
                Benchmark.Keys.ToList().ForEach(dt =>
                {
                    if (dt >= equity.Keys.FirstOrDefault().AddDays(-1) && dt < equity.Keys.LastOrDefault())
                    {
                        if (Benchmark.ContainsKey(dtPrevious))
                        {
                            decimal deltaBenchmark = (Benchmark[dt] - Benchmark[dtPrevious]) / Benchmark[dtPrevious];
                            listBenchmark.Add((double)(deltaBenchmark));
                        }
                        else
                        {
                            listBenchmark.Add(0);
                        }
                        dtPrevious = dt;
                    }
                });

                //THIS SHOULD NEVER HAPPEN --> But if it does, log it and fail silently.
                while (listPerformance.Count < listBenchmark.Count)
                {
                    listPerformance.Add(0);
                    Log.Error("Statistics.Generate(): Padded Performance");
                }
                while (listPerformance.Count > listBenchmark.Count)
                {
                    listBenchmark.Add(0);
                    Log.Error("Statistics.Generate(): Padded Benchmark");
                }
            }
            catch (Exception err)
            {
                Log.Error("Statistics.Generate.Dic-Array Convert: " + err.Message);
            }

            try
            {
                //Number of years in this dataset:
                fractionOfYears = (equity.Keys.LastOrDefault() - equity.Keys.FirstOrDefault()).TotalDays / 365;
            }
            catch (Exception err)
            {
                Log.Error("Statistics.Generate(): Fraction of Years: " + err.Message);
            }

            try
            {
                algoCompoundingPerformance = CompoundingAnnualPerformance(startingCash, equity.Values.LastOrDefault(), (decimal)fractionOfYears);
                finalBenchmarkCash = ((Benchmark.Values.Last() - Benchmark.Values.First()) / Benchmark.Values.First()) * startingCash;
                benchCompoundingPerformance = CompoundingAnnualPerformance(startingCash, finalBenchmarkCash, (decimal)fractionOfYears);
            }
            catch (Exception err)
            {
                Log.Error("Statistics.Generate(): Compounding: " + err.Message);
            }

            try
            {
                //Run over each equity day:
                foreach (DateTime closedTrade in profitLoss.Keys)
                {
                    profitLossValue = profitLoss[closedTrade];

                    //Check if this date is in the "years" array:
                    int year = closedTrade.Year;
                    if (!years.Contains(year))
                    {
                        //Initialise a new year holder:
                        years.Add(year);
                        annualTrades.Add(year, 0);
                        annualWins.Add(year, 0);
                        annualWinTotal.Add(year, 0);
                        annualLosses.Add(year, 0);
                        annualLossTotal.Add(year, 0);
                    }

                    //Add another trade:
                    annualTrades[year]++;

                    //Profit loss tracking:
                    if (profitLossValue > 0)
                    {
                        annualWins[year]++;
                        annualWinTotal[year] += profitLossValue / runningCash;
                    }
                    else
                    {
                        annualLosses[year]++;
                        annualLossTotal[year] += profitLossValue / runningCash;
                    }

                    //Increment the cash:
                    runningCash += profitLossValue;
                }

                //Get the annual percentage of profit and loss:
                foreach (int year in years)
                {
                    annualNetProfit[year] = (annualWinTotal[year] + annualLossTotal[year]);
                }

                //Sum the totals:
                try
                {
                    if (profitLoss.Keys.Count > 0)
                    {
                        totalTrades = annualTrades.Values.Sum();
                        totalWins = annualWins.Values.Sum();
                        totalLosses = annualLosses.Values.Sum();
                        totalNetProfit = (equity.Values.LastOrDefault() / startingCash) - 1;

                        try
                        {
                            if (fractionOfYears > 0)
                            {
                                averageAnnualReturn = totalNetProfit / Convert.ToDecimal(fractionOfYears);
                            }
                            else
                            {
                                averageAnnualReturn = totalNetProfit;
                            }
                        }
                        catch (Exception err)
                        {
                            Log.Error("Statistics() Annual Average Return: " + err.Message);
                            averageAnnualReturn = annualNetProfit.Values.Average();
                        }

                        //-> Handle Div/0 Errors
                        if (totalWins == 0)
                        {
                            averageWin = 0;
                        }
                        else
                        {
                            averageWin = annualWinTotal.Values.Sum() / totalWins;
                        }
                        if (totalLosses == 0)
                        {
                            averageLoss = 0;
                            averageWinRatio = 0;
                        }
                        else
                        {
                            averageLoss = annualLossTotal.Values.Sum() / totalLosses;
                            averageWinRatio = Math.Abs(averageWin / averageLoss);
                        }
                        if (totalTrades == 0)
                        {
                            winRate = 0;
                            lossRate = 0;
                        }
                        else
                        {
                            winRate = Math.Round(totalWins / totalTrades, 5);
                            lossRate = Math.Round(totalLosses / totalTrades, 5);
                        }
                    }

                }
                catch (Exception err)
                {
                    Log.Error("Statistics.RunOrders(): Second Half: " + err.Message);
                }

                decimal profitLossRatio = Statistics.ProfitLossRatio(averageWin, averageLoss);
                string profitLossRatioHuman = profitLossRatio.ToString();
                if (profitLossRatio == -1) profitLossRatioHuman = "0";

                //Add the over all results first, break down by year later:
                statistics = new Dictionary<string, string>() { 
                    { "Total Trades", Math.Round(totalTrades, 0).ToString() },
                    { "Average Win", Math.Round(averageWin * 100, 2) + "%"  },
                    { "Average Loss", Math.Round(averageLoss * 100, 2) + "%" },
                    { "Compounding Annual Return", Math.Round(algoCompoundingPerformance * 100, 3) + "%" },
                    { "Drawdown", (Statistics.Drawdown(equity, 3) * 100) + "%" },
                    { "Expectancy", Math.Round((winRate * averageWinRatio) - (lossRate), 3).ToString() },
                    { "Net Profit", Math.Round(totalNetProfit * 100, 3) + "%"},
                    { "Sharpe Ratio", Math.Round(Statistics.SharpeRatio(listPerformance, riskFreeRate), 3).ToString() },
                    { "Loss Rate", Math.Round(lossRate * 100) + "%" },
                    { "Win Rate", Math.Round(winRate * 100) + "%" }, 
                    { "Profit-Loss Ratio", profitLossRatioHuman },
                    { "Alpha", Math.Round(Statistics.Alpha(listPerformance, listBenchmark, riskFreeRate), 3).ToString() },
                    { "Beta", Math.Round(Statistics.Beta(listPerformance, listBenchmark), 3).ToString() },
                    { "Annual Standard Deviation", Math.Round(Statistics.AnnualStandardDeviation(listPerformance, tradingDaysPerYear), 3).ToString() },
                    { "Annual Variance", Math.Round(Statistics.AnnualVariance(listPerformance, tradingDaysPerYear), 3).ToString() },
                    { "Information Ratio", Math.Round(Statistics.InformationRatio(listPerformance, listBenchmark), 3).ToString() },
                    { "Tracking Error", Math.Round(Statistics.TrackingError(listPerformance, listBenchmark), 3).ToString() },
                    { "Treynor Ratio", Math.Round(Statistics.TreynorRatio(listPerformance, listBenchmark, riskFreeRate), 3).ToString() }
                };
            }
            catch (Exception err)
            {
                Log.Error("QC.Statistics.RunOrders(): " + err.Message + err.InnerException + err.TargetSite);
            }
            return statistics;
        }

        /// <summary>
        /// Return profit loss ratio safely.
        /// </summary>
        /// <param name="averageWin"></param>
        /// <param name="averageLoss"></param>
        /// <returns></returns>
        public static decimal ProfitLossRatio(decimal averageWin, decimal averageLoss)
        {
            if (averageLoss == 0)
            {
                return -1;
            }
            else
            {
                return Math.Round(averageWin / Math.Abs(averageLoss), 2);
            }
        }

        /// <summary>
        /// Get the Drawdown Statistic for this Period.
        /// </summary>
        /// <param name="equityOverTime">Array of portfolio value over time.</param>
        /// <param name="rounding">Round the drawdown statistics </param>
        /// <returns>Draw down percentage over period.</returns>
        public static decimal Drawdown(SortedDictionary<DateTime, decimal> equityOverTime, int rounding = 2)
        {
            //Initialise:
            int priceMaximum = 0;
            int previousMinimum = 0;
            int previousMaximum = 0;

            try
            {
                List<decimal> lPrices = equityOverTime.Values.ToList<decimal>();
                for (int id = 0; id < lPrices.Count; id++)
                {
                    if (lPrices[id] >= lPrices[priceMaximum])
                    {
                        priceMaximum = id;
                    }
                    else
                    {
                        if ((lPrices[priceMaximum] - lPrices[id]) > (lPrices[previousMaximum] - lPrices[previousMinimum]))
                        {
                            previousMaximum = priceMaximum;
                            previousMinimum = id;
                        }
                    }
                }
                return Math.Round((lPrices[previousMaximum] - lPrices[previousMinimum]) / lPrices[previousMaximum], rounding);
            }
            catch (Exception err)
            {
                Log.Error("Statistics.Drawdown(): " + err.Message);
            }
            return 0;
        } // End Drawdown:


        /// <summary>
        /// Get the annual compounded returns:
        /// </summary>
        /// <returns></returns>
        public static decimal CompoundingAnnualPerformance(decimal startingCapital, decimal finalCapital, decimal years)
        {
            return (decimal)Math.Pow((double)finalCapital / (double)startingCapital, (1 / (double)years)) - 1;
        }

        /// <summary>
        /// Annualized Returns
        /// </summary>
        public static double AnnualPerformance(List<double> performance, double tradingDaysPerYear = 252)
        {
            return performance.Average() * tradingDaysPerYear;
        }

        /// <summary>
        /// Annualized Variance
        /// </summary>
        public static double AnnualVariance(List<double> performance, double tradingDaysPerYear = 252)
        {
            return (MathNet.Numerics.Statistics.Statistics.Variance(performance)) * tradingDaysPerYear;
        }

        /// <summary>
        /// Annualized Standard Deviation
        /// </summary>
        public static double AnnualStandardDeviation(List<double> performance, double tradingDaysPerYear = 252)
        {
            return Math.Sqrt(MathNet.Numerics.Statistics.Statistics.Variance(performance) * tradingDaysPerYear);
        }

        /// <summary>
        /// BETA: Covariance between the Algorith and Benchmark performance, divided by Benchmark's variance
        /// </summary>
        public static double Beta(List<double> algoPerformance, List<double> benchmarkPerformance)
        {
            return MathNet.Numerics.Statistics.Statistics.Covariance(algoPerformance, benchmarkPerformance) / MathNet.Numerics.Statistics.Statistics.Variance(benchmarkPerformance);
        }

        /// <summary>
        /// ALPHA: Abnormal returns over the risk free rate and the relationshio (beta) with the benchmark returns
        /// </summary>
        public static double Alpha(List<double> algoPerformance, List<double> benchmarkPerformance, double riskFreeRate)
        {
            return AnnualPerformance(algoPerformance) - (riskFreeRate + Beta(algoPerformance, benchmarkPerformance) * (AnnualPerformance(benchmarkPerformance) - riskFreeRate));
        }

        /// <summary>
        /// Tracking Error Volatility (TEV): Measure of how closely a portfolio follows the index to which it is benchmarked (e.g. If algo = benchmark, TEV = 0)
        /// </summary>
        public static double TrackingError(List<double> algoPerformance, List<double> benchmarkPerformance)
        {
            return Math.Sqrt(AnnualVariance(algoPerformance) - 2 * Correlation.Pearson(algoPerformance, benchmarkPerformance) * AnnualStandardDeviation(algoPerformance) * AnnualStandardDeviation(benchmarkPerformance) + AnnualVariance(benchmarkPerformance));
        }

        /// <summary>
        /// Information Ratio: Risk Adjusted Return (Risk = TEV, a volatility measures that considers the volatility of both algo and benchmark)
        /// </summary>
        public static double InformationRatio(List<double> algoPerformance, List<double> benchmarkPerformance)
        {
            return (AnnualPerformance(algoPerformance) - AnnualPerformance(benchmarkPerformance)) / (TrackingError(algoPerformance, benchmarkPerformance));
        }

        /// <summary>
        /// Sharpe Ratio wrt Risk Free Rate: Measures excess of return per unit of risk (risk = Algo's volatility)
        /// </summary>
        public static double SharpeRatio(List<double> algoPerformance, double riskFreeRate)
        {
            return (AnnualPerformance(algoPerformance) - riskFreeRate) / (AnnualStandardDeviation(algoPerformance));
        }

        /// <summary>
        /// Treynor Ratio:  Measurement of the returns earned in excess of that which could have been earned on an investment that has no diversifiable risk
        /// </summary>
        public static double TreynorRatio(List<double> algoPerformance, List<double> benchmarkPerformance, double riskFreeRate)
        {
            return (AnnualPerformance(algoPerformance) - riskFreeRate) / (Beta(algoPerformance, benchmarkPerformance));
        }


    } // End of Statistics

} // End of Namespace