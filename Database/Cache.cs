﻿using Database.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Database
{
    /*
     * The Cache class is responsible for downloading the data from the database and
     * returning it when requested. Overall, this should reduce the average latency
     * for fetching data from the database.
     */

    internal static class Cache
    {
        private static Timer Timer { get; set; }

        public static AutopurgeTable Autopurge { get; set; } = new AutopurgeTable();

        public static void Initialise() 
        // Start the automatic cache downloads
        {
            DownloadTables();

            Timer = new Timer(30000); // The cache will be updated every 30 seconds.
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        // At regular intervals, call the download tables method.
        {
            DownloadTables();
        }

        private static void DownloadTables()
        {
            Autopurge.LoadAsync();
        }
    }
}
