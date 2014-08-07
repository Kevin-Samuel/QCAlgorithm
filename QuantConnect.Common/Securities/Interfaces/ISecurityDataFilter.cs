/*
* QUANTCONNECT.COM: Interface for Security Transaction Model Classes
* Implement this interface to define your own transaction model
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//QuantConnect Libraries:
using QuantConnect;
using QuantConnect.Logging;
using QuantConnect.Models;

namespace QuantConnect.Securities {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Security Data Filter Interface
    /// </summary>
    public interface ISecurityDataFilter 
    {

        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
        /// <summary>
        /// Filter out a tick from this vehicle, with this new data:
        /// </summary>
        /// <param name="data">New data packet:</param>
        /// <param name="vehicle">Vehicle of this filter.</param>
        bool Filter(Security vehicle, BaseData data);


    } // End Data Filter Interface

} // End QC Namespace
