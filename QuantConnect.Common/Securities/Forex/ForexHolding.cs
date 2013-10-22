/*
* QUANTCONNECT.COM: FOREX Holding Class
* FOREX Holding Class - Track portfolio, holdings, leverage, cash
*/
/**********************************************************
 * USING NAMESPACES
 **********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// FOREX Holdings Override: Any Properties specifically for FOREX Holdings Cases:
    /// </summary>
    public class ForexHolding : SecurityHolding {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/


        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Forex Holding Class
        /// </summary>
        public ForexHolding(string symbol, ISecurityTransactionModel transactionModel) :
            base(symbol, transactionModel)
        {
            //Nothing to do.
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
            

        /******************************************************** 
        * CLASS METHODS 
        *********************************************************/
            


    } // End Equity Holdings:
} //End Namespace