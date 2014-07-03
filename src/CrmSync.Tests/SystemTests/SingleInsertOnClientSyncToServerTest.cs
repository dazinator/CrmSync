using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Linq;
using System.Text;
using CrmAdo;
using CrmDeploy;
using CrmDeploy.Enums;
using CrmSync.Dynamics;
using CrmSync.Dynamics.Metadata;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Synchronization.Data;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;
using CrmSync.Plugin;

namespace CrmSync.Tests.SystemTests
{



    [Category("Sync")]
    [TestFixture]
    public class SingleInsertOnClientSyncToServerTest : CrmSyncIntegrationTest
    {

        public SingleInsertOnClientSyncToServerTest()
        {

        }

        [Test(Description = "Verifies that a single record inserted into the client database, can be synchronised with the server.")]
        public void Can_Sync_Single_Insert_On_Client_Roundtrip_With_Server()
        {       

            var sampleStats = new SampleStats();
            var crmConnString = GetCrmServiceProvider().ConnectionProvider.OrganisationServiceConnectionString;
            var sampleSyncAgent = new TestDynamicsCrmSyncAgent(SqlCompactDatabaseConnectionString, crmConnString);
          
            SyncStatistics syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "initial");

            // get number of existing records.
            // assert that the client only has one record and that the server only has 1 record.
            int existingCount = 0;
            using (var clientConn = new SqlCeConnection(SqlCompactDatabaseConnectionString))
            {
                clientConn.Open();
                using (var sqlCeCommand = clientConn.CreateCommand())
                {
                    sqlCeCommand.CommandText = string.Format("SELECT COUNT({0}) FROM {1}", TestDynamicsCrmServerSyncProvider.IdAttributeName, TestDynamicsCrmServerSyncProvider.TestEntityName);
                    existingCount = (int)sqlCeCommand.ExecuteScalar();
                    // Assert.That(rowCount, Is.EqualTo(1), string.Format("Only 1 record was synchronised however {0} records ended up in the client database!", rowCount));
                }
                clientConn.Close();
            }


            //Make changes on the client.
            // We will insert into the same columns on the client that the server sync provider includes in its server insert statement,
            // with exception of the sync client id field (thats only provided during a sync)

            var columnsForClientInsert = TestDynamicsCrmServerSyncProvider.InsertColumns.ToList();
            var syncClientIdColumn = columnsForClientInsert.First(c => c.AttributeName == SyncColumnInfo.CreatedBySyncClientIdAttributeName);
            columnsForClientInsert.Remove(syncClientIdColumn);
            sampleSyncAgent.ClientSyncProvider.InsertTestRecord(1, columnsForClientInsert);

            //Now sync the new record on the client to the server.
            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "second");

            // Verfiy new record is added on server.
            Assert.That(syncStatistics.DownloadChangesFailed, Is.EqualTo(0), "There were failed downloads during the sync.");
            Assert.That(syncStatistics.UploadChangesFailed, Is.EqualTo(0), "There were failed uploads during the sync.");
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            using (var orgService = service.GetOrganisationService() as OrganizationServiceContext)
            {
                var entity = (from a in orgService.CreateQuery(TestDynamicsCrmServerSyncProvider.TestEntityName) orderby a["createdon"] descending select a).FirstOrDefault();
                Assert.That(entity, Is.Not.Null);
                var clientId = entity.Attributes[SyncColumnInfo.CreatedBySyncClientIdAttributeName];
                Assert.That(clientId, Is.EqualTo(SelectIncrementalCreatesCommand.SyncClientId), "A record was inserted during synchronisation, however it did not have a client id set.");

                // verify all the other fields are set.
                foreach (var col in columnsForClientInsert)
                {

                    var testAttribute = entity.Attributes[col.AttributeName];
                    Assert.That(testAttribute, Is.Not.Null, "A record was inserted during synchronisation, however it did not have a value for the attribute:  " + col.AttributeName);
                    Assert.That(testAttribute.ToString(), Is.Not.EqualTo(""), "The attribute " + col.AttributeName + " had a value inserted to the client but was then synced to the server, and was blank on the server after the sync finished.");

                }

            }

            // Now sync one more time and verify that the new reocrd that has been applied to the server does not come back as an insert on the client again! 
            // The server updates the record as its saved to the server so it should come back as an update with the server generated values set on the record.
            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "third");

            // assert that the client only has one record and that the server only has 1 record.
            using (var clientConn = new SqlCeConnection(SqlCompactDatabaseConnectionString))
            {
                clientConn.Open();
                using (var sqlCeCommand = clientConn.CreateCommand())
                {
                    sqlCeCommand.CommandText = string.Format("SELECT COUNT({0}) FROM {1}", TestDynamicsCrmServerSyncProvider.IdAttributeName, TestDynamicsCrmServerSyncProvider.TestEntityName);
                    var rowCount = (int)sqlCeCommand.ExecuteScalar();
                    Assert.That(rowCount, Is.EqualTo(existingCount + 1), string.Format("Only 1 new record was created, however after a few synchronisations, {0} new records ended up in the client database!", rowCount - existingCount));
                }
                clientConn.Close();
            }


        }

    }



}
