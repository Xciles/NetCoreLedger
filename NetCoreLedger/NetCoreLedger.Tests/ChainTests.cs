using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreLedger.Business;
using NetCoreLedger.Domain;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Tests
{
    [TestClass]
    public class ChainTests
    {
        private static string ZeroHash => "0000000000000000000000000000000000000000000000000000000000000000";
        private static string EmptyDataHash => "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";


        [TestMethod]
        public void EmptyChainTest()
        {
            var chain = new Chain();

            Assert.IsTrue(chain.Count == 0);
            Assert.AreEqual(chain.Genesis, null);
            Assert.AreEqual(chain.Last, null);

            foreach (var blockHeader in chain.Enumerate())
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void GenesisChainTest()
        {
            var time = DateTime.UtcNow.ToUnixTimeSeconds();
            var genesis = new Block(0, DateTime.UtcNow.ToUnixTimeSeconds());
            var chain = new Chain(genesis.Header);

            Assert.IsTrue(chain.Count == 1);
            Assert.AreEqual(chain.Genesis, chain.Last);
            Assert.AreSame(chain.Genesis, chain.Last);

            foreach (var blockHeader in chain.Enumerate())
            {
                Assert.AreEqual(blockHeader.BlockTimestamp, time);
                Assert.AreEqual(blockHeader.PreviousHash, ZeroHash);
                Assert.AreEqual(blockHeader.DataHash, EmptyDataHash);
            }
        }

        [TestMethod]
        public void BasicChainTest()
        {
            var time = DateTime.UtcNow.ToUnixTimeSeconds();
            var genesisBlock = new Block(0, DateTime.UtcNow.ToUnixTimeSeconds());

            // save genesis to blockStore
            // save genesis to Chain

            var chain = new Chain(genesisBlock.Header);
            var firstBlock = new Block(chain.Last.Header.GetHash(), chain.Count, time + 1000)
            {
                Data = "Some Test Data!"
            };
            chain.AddLast(firstBlock.Header);
            var secondBlock = new Block(chain.Last.Header.GetHash(), chain.Count, time + 2000)
            {
                Data = "More Test Data!"
            };
            chain.AddLast(secondBlock.Header);
            var thirdBlock = new Block(chain.Last.Header.GetHash(), chain.Count, time + 3000)
            {
                Data = "Even More Test Data!"
            };
            chain.AddLast(thirdBlock.Header);

            Assert.IsTrue(chain.Count == 4);
            Assert.AreNotEqual(chain.Genesis, chain.Last);

            // Genesis
            var genesis = chain[0];
            Assert.AreEqual(genesisBlock.Header, genesis.Header);
            Assert.AreEqual(genesisBlock.Header.GetHash(), genesis.BlockHash);
            Assert.AreEqual(genesis.Header.BlockTimestamp, time);
            Assert.AreEqual(genesis.Header.PreviousHash, ZeroHash);
            Assert.AreEqual(genesis.Header.DataHash, EmptyDataHash);

            // First
            var first = chain[1];
            Assert.AreEqual(firstBlock.Header, first.Header);
            Assert.AreEqual(firstBlock.Header.GetHash(), first.BlockHash);
            Assert.AreEqual(first.Header.BlockTimestamp, time + 1000);
            Assert.AreEqual(first.Header.PreviousHash, firstBlock.Header.PreviousHash);
            Assert.AreEqual(first.Header.DataHash, firstBlock.Header.DataHash);
            Assert.AreNotEqual(first.Header.DataHash, EmptyDataHash);

            // Second 
            var second = chain[2];
            Assert.AreEqual(secondBlock.Header, second.Header);
            Assert.AreEqual(secondBlock.Header.GetHash(), second.BlockHash);
            Assert.AreEqual(second.Header.BlockTimestamp, time + 2000);
            Assert.AreEqual(second.Header.PreviousHash, secondBlock.Header.PreviousHash);
            Assert.AreEqual(second.Header.DataHash, secondBlock.Header.DataHash);
            Assert.AreNotEqual(second.Header.DataHash, EmptyDataHash);

            // Thrid 
            var third = chain[3];
            Assert.AreEqual(thirdBlock.Header, third.Header);
            Assert.AreEqual(thirdBlock.Header.GetHash(), third.BlockHash);
            Assert.AreEqual(third.Header.BlockTimestamp, time + 3000);
            Assert.AreEqual(third.Header.PreviousHash, thirdBlock.Header.PreviousHash);
            Assert.AreEqual(third.Header.DataHash, thirdBlock.Header.DataHash);
            Assert.AreNotEqual(third.Header.DataHash, EmptyDataHash);
        }
    }
}
