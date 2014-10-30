/*
* QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals, V0.1
* Basic Indicator Interface
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

//QuantConnect Project Libraries:
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Indicators {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Extensions Function Collections - Group all static extensions functions here.
    /// </summary>
    public interface IBasicIndicator 
    {
        /******************************************************** 
        * INTERFACE PROPERTIES
        *********************************************************/
        /// <summary>
        /// Output result value
        /// </summary>
        decimal Value 
        { 
            get; 
        }


        /******************************************************** 
        * INTERFACE METHODS
        *********************************************************/

        /// <summary>
        /// Update the indicator value with a decimal value
        /// </summary>
        /// <param name="value">decimal value of the update</param>
        /// <returns>Bool true when its ready</returns>
        bool Update(decimal value);

        /// <summary>
        /// Update the indicator with a TradeBar
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        bool Update(TradeBar bar);


        /// <summary>
        /// Update the indicator with a tick object
        /// </summary>
        /// <param name="tick"></param>
        /// <returns></returns>
        bool Update(Tick tick);
    }
}
