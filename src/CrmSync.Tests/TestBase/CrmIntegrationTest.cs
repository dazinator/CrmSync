using CrmSync.Dynamics;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmSync.Tests
{
    [Category("Integration")]
    public abstract class CrmIntegrationTest
    {

        public ICrmServiceProvider GetCrmServiceProvider()
        {
            return new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig());
        }

        [TestFixtureSetUp]
        public void TestSetup()
        {
            Console.WriteLine("Is running in 64 bit process? " + Environment.Is64BitProcess);
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
            Console.WriteLine(dataDirectory);
            SetUp();
        }

        [TestFixtureTearDown]
        public void TestTearDown()
        {
            TearDown();
        }

        protected abstract void SetUp();

        protected abstract void TearDown();


    }
}
