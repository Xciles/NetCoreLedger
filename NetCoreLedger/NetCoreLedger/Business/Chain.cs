using System.Collections.Concurrent;
using System.Collections.Generic;
using NetCoreLedger.Domain;

namespace NetCoreLedger.Business
{
    public class Chain
    {
        private ConcurrentDictionary<string, ChainBlock> _chainByHash = new ConcurrentDictionary<string, ChainBlock>();
        private ConcurrentDictionary<uint, ChainBlock> _chainByIndex = new ConcurrentDictionary<uint, ChainBlock>();
        private ChainBlock _last = null;

        public ChainBlock Genesis { get { return GetChainBlock(0); } }
        public ChainBlock Last { get { return _last; } }
        public uint Count { get { return Last?.Index + 1 ?? 0; } }


        public Chain() { }

        public Chain(BlockHeader genesis)
        {
            AddLast(genesis);
        }

        private ChainBlock GetChainBlock(uint index)
        {
            _chainByIndex.TryGetValue(index, out var result);
            return result;
        }

        private void SetChainBlock(ChainBlock block)
        {
            _chainByIndex.AddOrUpdate(block.Index, block, (u, chainBlock) => chainBlock);
            _chainByHash.AddOrUpdate(block.BlockHash, block, (s, chainBlock) => chainBlock);
        }

        public void AddLast(BlockHeader blockHeader)
        {
            var chainBlock = new ChainBlock(blockHeader, Last);

            _last = chainBlock;

            SetChainBlock(_last);
        }

        public IEnumerable<BlockHeader> Enumerate()
        {
            var current = Last;
            while (current != null)
            {
                yield return current.Header;

                current = current.Previous;
            }
        }

        public ChainBlock this[uint i] => GetChainBlock(i);
    }
}