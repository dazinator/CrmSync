using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmAdo;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.Server;

namespace CrmSync
{
    class Program
    {
        static void Main(string[] args)
        {

            
            //The SampleStats class handles information from the SyncStatistics
            //object that the Synchronize method returns.
            SampleStats sampleStats = new SampleStats();

            //Request a password for the client database, and delete
            //and re-create the database. The client synchronization
            //provider also enables you to create the client database 
            //if it does not exist.
            Utility.SetPassword_SqlCeClientSync();
            Utility.DeleteAndRecreateCompactDatabase(Utility.ConnStr_SqlCeClientSync, true);

            //Initial synchronization. Instantiate the SyncAgent
            //and call Synchronize.
            DynamicsCrmSyncAgent sampleSyncAgent = new DynamicsCrmSyncAgent();
            SyncStatistics syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "initial");

            //Make changes on the server and client.
            Utility.MakeDataChangesOnServer(DynamicsCrmServerSyncProvider.EntityName);
            Utility.MakeDataChangesOnClient(DynamicsCrmServerSyncProvider.EntityName);

            //Subsequent synchronization.
            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "subsequent");

            //Exit.
            Console.Write("\nPress Enter to do another sync..");
            Console.ReadLine();

            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "subsequent");
            
            //Return server data back to its original state.
            Utility.CleanUpServer();

            //Exit.
            Console.Write("\nPress Enter to close the window.");
            Console.ReadLine();
        }

    }
}
