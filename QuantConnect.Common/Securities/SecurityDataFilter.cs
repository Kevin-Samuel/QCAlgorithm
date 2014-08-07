/*
* QUANTCONNECT.COM: Secuity Cache
* Common caching class for storing historical ticks etc.
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
    /// Packet by Packet Data Filtering Mechanism for Dynamically Detecting Bad Ticks.
    /// </summary>
    public class SecurityDataFilter : ISecurityDataFilter
    {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/

        /******************************************************** 
        * CONSTRUCTOR/DELEGATE DEFINITIONS
        *********************************************************/
        /// <summary>
        /// Initialize Data Filter Class:
        /// </summary>
        public SecurityDataFilter()
        {
            
        }

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Filter the data: true - accept, false - fail.
        /// </summary>
        /// <param name="data">Data class</param>
        /// <param name="vehicle">Security asset</param>
        public virtual bool Filter(Security vehicle, BaseData data)
        {
            return true;
        }

    } //End Filter

} //End Namespace