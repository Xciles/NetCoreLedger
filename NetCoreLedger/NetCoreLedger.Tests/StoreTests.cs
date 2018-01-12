using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetCoreLedger.Business;
using NetCoreLedger.Domain;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Tests
{
    [TestClass]
    public class StoreTests
    {
        [TestMethod]
        public void EmptyStoreTest()
        {
            var store = new Store("test");
            Assert.AreNotEqual(store, null);

            Assert.IsFalse(store.EnumerateFile().Any());
            
            store.RemoveBackingFile();
        }

        [TestMethod]
        public void GenisisStoreTest()
        {
            var store = new Store("test");
            var genesis = Block.GenerateGenesis();
            store.Append(genesis);

            Assert.IsTrue(store.EnumerateFile().Any());
            Assert.IsTrue(store.EnumerateFile().Count() == 1);

            var retrievedGenesis = store.EnumerateFile().First();

            Assert.AreEqual(retrievedGenesis.Header.IdHash, genesis.Header.GetHash());
            Assert.AreEqual(retrievedGenesis.Header.Size, genesis.Size());

            Assert.AreEqual(retrievedGenesis.Block.Header.GetHash(), genesis.Header.GetHash());
            Assert.AreEqual(retrievedGenesis.Block.Header.DataHash, genesis.Header.DataHash);
            Assert.AreEqual(retrievedGenesis.Block.Header.PreviousHash, genesis.Header.PreviousHash);

            Assert.AreEqual(retrievedGenesis.Block.Data, genesis.Data);

            store.RemoveBackingFile();
        }

        [TestMethod]
        public void ItemsStoreTest()
        {
            var store = new Store("test");

            Assert.IsFalse(store.EnumerateFile().Any());

            var genesis = Block.GenerateGenesis();
            var block1 = new Block(genesis.Header.GetHash(), 1, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block1!"
            };
            var block2 = new Block(block1.Header.GetHash(), 2, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block2!"
            };
            var block3 = new Block(block2.Header.GetHash(), 3, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block3!"
            };

            store.Append(genesis);
            store.Append(block1);
            store.Append(block2);
            store.Append(block3);

            Assert.IsTrue(store.EnumerateFile().Any());
            Assert.IsTrue(store.EnumerateFile().Count() == 4);

            var retrievedGenesis = store.EnumerateFile().First();

            Assert.AreEqual(retrievedGenesis.Header.IdHash, genesis.Header.GetHash());
            Assert.AreEqual(retrievedGenesis.Header.Size, genesis.Size());

            Assert.AreEqual(retrievedGenesis.Block.Header.GetHash(), genesis.Header.GetHash());
            Assert.AreEqual(retrievedGenesis.Block.Header.DataHash, genesis.Header.DataHash);
            Assert.AreEqual(retrievedGenesis.Block.Header.PreviousHash, genesis.Header.PreviousHash);

            Assert.AreEqual(retrievedGenesis.Block.Data, genesis.Data);

            StorageItem previousStorageItem = null;
            foreach (var storageItem in store.EnumerateFile())
            {
                if (storageItem.Block.Header.PreviousHash == Hasher.ZeroSha256)
                {
                    Assert.AreEqual(storageItem.Header.IdHash, genesis.Header.GetHash());
                    Assert.AreEqual(storageItem.Block.Header.GetHash(), genesis.Header.GetHash());
                    Assert.AreEqual(storageItem.Block.Header.Index, 0U);
                }
                else
                {
                    Assert.AreEqual(storageItem.Block.Header.PreviousHash, previousStorageItem.Header.IdHash);
                    Assert.AreEqual(storageItem.Block.Header.PreviousHash, previousStorageItem.Block.Header.GetHash());
                    Assert.AreEqual(storageItem.Block.Header.Index, previousStorageItem.Block.Header.Index + 1);
                }

                Assert.AreEqual(storageItem.Header.Size, storageItem.Block.Size());
                Assert.AreEqual(storageItem.Block.Header.GetHash(), storageItem.Header.IdHash);

                previousStorageItem = storageItem;
            }

            store.RemoveBackingFile();
        }

        [TestMethod]
        public void ItemsFindByIdStoreTest()
        {
            var store = new Store("test");

            Assert.IsFalse(store.EnumerateFile().Any());

            var genesis = Block.GenerateGenesis();
            var block1 = new Block(genesis.Header.GetHash(), 1, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block1!"
            };
            var block2 = new Block(block1.Header.GetHash(), 2, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block2!"
            };
            var block3 = new Block(block2.Header.GetHash(), 3, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block3!"
            };

            store.Append(genesis);
            store.Append(block1);
            store.Append(block2);
            store.Append(block3);

            Assert.IsTrue(store.EnumerateFile().Any());
            Assert.IsTrue(store.EnumerateFile().Count() == 4);

            var foundStorageItem = store.FindBlockById(block2.Header.GetHash());

            Assert.AreEqual(foundStorageItem.Header.IdHash, block2.Header.GetHash());
            Assert.AreEqual(foundStorageItem.Header.Size, block2.Size());

            Assert.AreEqual(foundStorageItem.Block.Header.GetHash(), block2.Header.GetHash());
            Assert.AreEqual(foundStorageItem.Block.Header.DataHash, block2.Header.DataHash);
            Assert.AreEqual(foundStorageItem.Block.Header.PreviousHash, block2.Header.PreviousHash);

            Assert.AreEqual(foundStorageItem.Block.Data, block2.Data);

            store.RemoveBackingFile();
        }

        [TestMethod]
        public void ItemsFindByPositionStoreTest()
        {
            var store = new Store("test");

            Assert.IsFalse(store.EnumerateFile().Any());

            var genesis = Block.GenerateGenesis();
            var block1 = new Block(genesis.Header.GetHash(), 1, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block1!"
            };
            var block2 = new Block(block1.Header.GetHash(), 2, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block2!"
            };
            var block3 = new Block(block2.Header.GetHash(), 3, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = "This is some test data for block3!"
            };

            store.Append(genesis);
            store.Append(block1);
            var position = store.Append(block2);
            store.Append(block3);

            Assert.IsTrue(store.EnumerateFile().Any());
            Assert.IsTrue(store.EnumerateFile().Count() == 4);

            var foundStorageItem = store.FindBlockByPosition(position);

            Assert.AreEqual(foundStorageItem.Header.IdHash, block2.Header.GetHash());
            Assert.AreEqual(foundStorageItem.Header.Size, block2.Size());

            Assert.AreEqual(foundStorageItem.Block.Header.GetHash(), block2.Header.GetHash());
            Assert.AreEqual(foundStorageItem.Block.Header.DataHash, block2.Header.DataHash);
            Assert.AreEqual(foundStorageItem.Block.Header.PreviousHash, block2.Header.PreviousHash);

            Assert.AreEqual(foundStorageItem.Block.Data, block2.Data);

            store.RemoveBackingFile();
        }
    }
}
