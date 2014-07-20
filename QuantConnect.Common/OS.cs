/*
* QUANTCONNECT.COM - 
* QC.OS -- Operating System Checks for Cross Platform C#
*/

/**********************************************************
* USING NAMESPACES
**********************************************************/
using System;
using System.IO;
using System.Diagnostics;

namespace QuantConnect {

    /******************************************************** 
    * CLASS DEFINITIONS
    *********************************************************/
    /// <summary>
    /// Operating Systems Class Manager
    /// </summary>
    public static class OS {
        /******************************************************** 
        * CLASS VARIABLES
        *********************************************************/
        private static PerformanceCounter _RamTotalCounter;
        private static PerformanceCounter _RamAvailableBytes;

        /// <summary>
        /// Total Physical Ram on the Machine:
        /// </summary>
        private static PerformanceCounter RamTotalCounter {
            get {
                if (_RamTotalCounter == null) {
                    if (OS.IsLinux) {
                        _RamTotalCounter = new PerformanceCounter ("Mono Memory", "Total Physical Memory"); 
                    } else {
                        _RamTotalCounter = new PerformanceCounter("Memory", "Available Bytes");
                    }
                }
                return _RamTotalCounter;
            }
        }

        /// <summary>
        /// Memory free on the machine available for use:
        /// </summary>
        private static PerformanceCounter RamAvailableBytes {
            get {
                if (_RamAvailableBytes == null) {
                    if (OS.IsLinux) { 
                        _RamAvailableBytes = new PerformanceCounter("Mono Memory", "Allocated Objects");
                    } else {
                        _RamAvailableBytes = new PerformanceCounter("Memory", "Available Bytes");
                    }
                }
                return _RamAvailableBytes;
            }
        }

        /******************************************************** 
        * CLASS PROPERTIES
        *********************************************************/
        /// <summary>
        /// Global Flag :: Operating System
        /// </summary>
        public static bool IsLinux {
            get {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        /// <summary>
        /// Global Flag :: Operating System
        /// </summary>
        public static bool IsWindows {
            get {
                return !IsLinux;
            }
        }


        /// <summary>
        /// Character Separating directories in this OS:
        /// </summary>
        public static string PathSeparation {
            get {
                return Path.DirectorySeparatorChar.ToString();
            }
        }

        /// <summary>
        /// Get the drive space remaining on windows and linux in MB
        /// </summary>
        public static long DriveSpaceRemaining { 
            get {
                string driveName = "/";
                if (OS.IsWindows) {
                    driveName = "D";                   
                }
                DriveInfo d = new DriveInfo(driveName);
                return d.AvailableFreeSpace / (1024 * 1024);
            }
        }


        /// <summary>
        /// Get the RAM remaining on the machine:
        /// </summary>
        public static long ApplicationMemoryUsed {
            get {
                Process proc = Process.GetCurrentProcess();
                return (proc.PrivateMemorySize64 / (1024*1024));
            }
        }

        /// <summary>
        /// Get the RAM remaining on the machine:
        /// </summary>
        public static long TotalPhysicalMemory {
            get {
                return (long)(RamTotalCounter.NextValue() / (1024*1024));
            }
        }

        /// <summary>
        /// Get the RAM remaining on the machine:
        /// </summary>
        public static long TotalPhysicalMemoryUsed {
            get {
                return (long)(GC.GetTotalMemory(false) / (1024*1024));
            }
        }

    } // End OS Class
} // End QC Namespace
