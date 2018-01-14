using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NetCoreLedger.Domain;
using NetCoreLedger.Utils;

namespace NetCoreLedger.Business
{
    public class Chain
    {
        private readonly ConcurrentDictionary<string, ChainBlock> _chainByHash = new ConcurrentDictionary<string, ChainBlock>();
        private readonly ConcurrentDictionary<uint, ChainBlock> _chainByIndex = new ConcurrentDictionary<uint, ChainBlock>();
        private ChainBlock _last = null;

        public ChainBlock Genesis => GetChainBlock(0);
        public ChainBlock Last => _last;
        public uint Count => Last?.Index + 1 ?? 0;


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

        public void Validate()
        {
            var current = Last;
            while (current != null)
            {
                if (current.BlockHash == Genesis.BlockHash)
                {
                    if (current.Previous != null) throw new ChainInvalidException();
                    if (current.Index != 0) throw new ChainInvalidException();
                }
                else
                {
                    if (current.BlockHash != current.Header.GetHash()) throw new ChainInvalidException();
                    if (current.Previous.BlockHash != current.Header.PreviousHash) throw new ChainInvalidException();
                    if (current.Previous.Header.GetHash() != current.Header.PreviousHash) throw new ChainInvalidException();
                    if (current.Previous.Index + 1 != current.Index) throw new ChainInvalidException();
                }

                current = current.Previous;
            }
        }
    }
}