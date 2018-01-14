using System;
using System.Collections.Generic;
using NetCoreLedger.Domain;
using NetCoreLedger.Extensions;
using NetCoreLedger.Utils;

namespace NetCoreLedger.Business
{
    public class Ledger
    {
        private Chain _chain;
        private Store _store; 
        
        public Ledger()
        {

        }

        public void Initialize()
        {
            // init ledger
            // Try load from disk and synchronize
            // If we can't load set initial and continue

            _chain = new Chain();
            _store = new Store("data");

            _store.SyncChain(_chain);

            if (_chain.Count == 0)
            {
                var genesis = Block.GenerateGenesis();
                _chain.AddLast(genesis.Header);
                _store.Append(genesis);
            }

            ValidateLedger();
        }

        public void AddBlock(Block block)
        {
            Validate(block);

            _chain.AddLast(block.Header);
            _store.Append(block);
        }

        private void Validate(Block block)
        {
            // Validate chain
            // Add to chain
            // Add to store
            _chain.Validate();
            _store.Validate();

            ValidateLedger();

            // validate block to add
            block.ValidateDataIntegrity();
            if (block.Header.PreviousHash != _chain.Last.Header.GetHash()) throw new BlockInvalidException();
            if (block.Header.Index != _chain.Last.Header.Index + 1) throw new BlockInvalidException();
        }

        public void AddBlockByData(string data)
        {
            // validate block to add
            var block = new Block(_chain.Last.Header.GetHash(), _chain.Count, DateTime.UtcNow.ToUnixTimeSeconds())
            {
                Data = data
            };

            Validate(block);

            _chain.AddLast(block.Header);
            _store.Append(block);
        }

        public IEnumerable<BlockHeader> EnumerateChain()
        {
            return _chain.Enumerate();
        }

        private void ValidateLedger()
        {
            foreach (var blockHeader in _chain.Enumerate())
            {
                var storageItem = _store.FindBlockById(blockHeader.GetHash());
                if (storageItem == null) throw new LedgerInvalidException();

                if (storageItem.Block.Header.GetHash() != blockHeader.GetHash()) throw new LedgerInvalidException();
            }
        }
    }
}
