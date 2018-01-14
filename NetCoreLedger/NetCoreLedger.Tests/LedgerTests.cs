using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreLedger.Business;
using NetCoreLedger.Domain;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Tests
{
    [TestClass]
    public class LedgerTests
    {
        [TestMethod]
        public void EmptyLedgerTest()
        {
            //var ledger = new Ledger();

            //Assert.AreNotEqual(ledger, null);
            //Assert.IsFalse(ledger.EnumerateChain().Any());

            //RemoveDataFile();
        }

        [TestMethod]
        public void GenisisLedgerTest()
        {
            var ledger = new Ledger();
            ledger.Initialize();

            Assert.AreNotEqual(ledger, null);
            Assert.IsTrue(ledger.EnumerateChain().Any());
            Assert.IsTrue(ledger.EnumerateChain().Count() == 1);

            RemoveDataFile();
        }

        [TestMethod]
        public void ItemsLedgerTest()
        {
            var ledger = new Ledger();
            ledger.Initialize();

            Assert.AreNotEqual(ledger, null);
            Assert.IsTrue(ledger.EnumerateChain().Any());
            Assert.IsTrue(ledger.EnumerateChain().Count() == 1);

            ledger.AddBlockByData("This is some test data for block1!");
            ledger.AddBlockByData("This is some test data for block2!");
            ledger.AddBlockByData("This is some test data for block3!");

            Assert.IsTrue(ledger.EnumerateChain().Any());
            Assert.IsTrue(ledger.EnumerateChain().Count() == 4);

            RemoveDataFile();
        }

        [TestMethod]
        public void ItemsAndSyncTest()
        {
            var ledger = new Ledger();
            ledger.Initialize();

            Assert.AreNotEqual(ledger, null);
            Assert.IsTrue(ledger.EnumerateChain().Any());
            Assert.IsTrue(ledger.EnumerateChain().Count() == 1);

            ledger.AddBlockByData("This is some test data for block1!");
            ledger.AddBlockByData("This is some test data for block2!");
            ledger.AddBlockByData("This is some test data for block3!");

            Assert.IsTrue(ledger.EnumerateChain().Any());
            Assert.IsTrue(ledger.EnumerateChain().Count() == 4);

            ledger = new Ledger();
            ledger.Initialize();

            Assert.IsTrue(ledger.EnumerateChain().Any());
            Assert.IsTrue(ledger.EnumerateChain().Count() == 4);

            RemoveDataFile();
        }

        private static void RemoveDataFile()
        {
            if (File.Exists("data/ridb.rdb"))
            {
                File.Delete("data/ridb.rdb");
            }
        }
    }
}
