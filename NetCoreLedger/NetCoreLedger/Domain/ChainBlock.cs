using System;
using NetCoreLedger.Business;

namespace NetCoreLedger.Domain
{
    public class ChainBlock
    {
        private string _blockHash;
        private ChainBlock _previous;
        private BlockHeader _header;
        private uint _index;

        public string BlockHash { get { return _blockHash; } }

        public ChainBlock Previous { get { return _previous; } }

        public BlockHeader Header { get { return _header; } }

        public uint Index { get { return _index; } }

        public ChainBlock(BlockHeader header, ChainBlock previous)
        {
            if (previous == null && header.PreviousHash == Hasher.ZeroSha256) 
            {
                // genesis block 
                _index = 0;
            }
            else if (previous == null)
            {
                if (header.PreviousHash != Hasher.EmtpySha256)
                {
                    // Genesis can only be empty
                    throw new ArgumentException(nameof(previous));
                }
            }
            else
            {
                if (header.PreviousHash != previous.BlockHash)
                {
                    // shouldbe the same
                    throw new ArgumentException(nameof(header));
                }
                _index = previous.Index + 1;
            }

            _blockHash = header.GetHash();
            _header = header;
            _previous = previous;
        }
    }
}