/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals, V0.1
 * Isolate memory and time usage of algorithm
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace QuantConnect {

    /******************************************************** 
    * QUANTCONNECT PROJECT LIBRARY
    *********************************************************/
    using QuantConnect.Logging;

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    public class Isolator {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        //Task Cancellation:
        public static CancellationTokenSource cancellation = new CancellationTokenSource();
        public static CancellationToken cancelToken = new CancellationToken();


        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Check if this task isolator is cancelled, and exit the analysis
        /// </summary>
        public static bool IsCancellationRequested
        {
            get {
                return cancelToken.IsCancellationRequested;
            }
        }


        /******************************************************** 
        * CLASS METHODS
        *********************************************************/
           
        /// <summary>
        /// Create a MD5 Hash of a string.
        /// </summary>
        public static string MD5(string stringToHash) {

            string hash = "";
                
            try {
                MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(stringToHash);
                byte[] hashArray = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashArray.Length; i++) {
                    sb.Append(hashArray[i].ToString("X2"));
                }
                hash = sb.ToString();
            } catch (Exception err) {
                Log.Error("QC.Security.MD5(): Error creating MD5: " + err.Message);
            }

            return hash;
        }


        /// <summary>
        /// Reset the cancellation token variables for a new task:
        /// </summary>
        public static void ResetCancelToken() {
            cancellation = new CancellationTokenSource();
            cancelToken = cancellation.Token;
        }



        /// <summary>
        /// Execute a code block with a maximum timeout.
        /// </summary>
        /// <param name="timeSpan">Timeout.</param>
        /// <param name="codeBlock">Code to execute</param>
        /// <param name="memoryCap">Maximum memory allocation, default 1GB</param>
        /// <returns>True if successful, False if Cancelled.</returns>
        public static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock, long memoryCap = 1024)
        {
            string message = "";
            DateTime dtEnd = DateTime.Now + timeSpan;

            //Convert to bytes
            memoryCap *= 1024 * 1024;

            ResetCancelToken();

            //Thread:
            Task task = Task.Factory.StartNew(() => codeBlock(), cancelToken);            

            while (!task.IsCompleted && DateTime.Now < dtEnd)
            {
                if (GC.GetTotalMemory(false) > memoryCap)
                {
                    if (GC.GetTotalMemory(true) > memoryCap)
                    {
                        message = "Execution Security Error: Memory Maxed Out - " + Math.Round(Convert.ToDouble(memoryCap / (1024 * 1024))) + "MB max. Check for recursive loops.";
                        Console.WriteLine("Isolator.ExecuteWithTimeLimit(): " + message);
                        break;
                    }
                }
            }

            if (task.IsCompleted == false && message == "")
            {
                message = "Execution Security Error: Operation timed out - " + timeSpan.Minutes + " minutes max. Check for recursive loops.";
                Console.WriteLine("Isolator.ExecuteWithTimeLimit(): " + message);
            }

            if (message != "")
            {
                cancellation.Cancel();
                Log.Error("Security.ExecuteWithTimeLimit(): " + message);
                Log.Trace("Security.ExecuteWithTimeLimit(): " + message);
                throw new Exception(message);
            }

            return task.IsCompleted;
        }

    }
}
