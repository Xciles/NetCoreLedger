using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Business
{
    public class Ledger
    {
        private Chain _chain;
        private Store _store; 
        


        public Ledger()
        {
            // init ledger
            // Try load from disk and synchronize
            _chain = new Chain();
            _store = new Store("data");



            _store.SyncChain(_chain);
        }
    }

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
            if (previous == null)
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

    public class Chain
    {
        private ConcurrentDictionary<string, ChainBlock> _chainByHash = new ConcurrentDictionary<string, ChainBlock>();
        private ConcurrentDictionary<uint, ChainBlock> _chainByIndex = new ConcurrentDictionary<uint, ChainBlock>();

        public ChainBlock Genesis { get { return GetChainBlock(0); } }

        public Chain() { }

        public Chain(BlockHeader genesis)
        {
            SetChainBlock(new ChainBlock(genesis, null));
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
            
        }
    }

    public class BlockHeader
    {

        private string _hash = "";
        private string _dataHash = "";
        private string _previousHash = "";
        private uint _index;
        private uint _blockTimestamp;

        public string DataHash
        {
            get { return _dataHash; }
            set { _dataHash = value; }
        }

        public string PreviousHash
        {
            get { return _previousHash; }
            set { _previousHash = value; }
        }

        public uint Index
        {
            get { return _index; }
            set { _index = value; }
        }

        public uint BlockTimestamp
        {
            get { return _blockTimestamp; }
            set { _blockTimestamp = value; }
        }

        public string GetHash()
        {
            if (!String.IsNullOrWhiteSpace(_hash))
            {
                // Has is already calculated
                return _hash;
            }
            var hash = Hasher.Sha256(ToBytes());
            _hash = hash;
            return _hash;
        }

        public void WriteToStream(Stream ms)
        {
            ms.WriteX2String(_previousHash);
            ms.WriteX2String(_dataHash);
            ms.WriteInt(_index);
            ms.WriteInt(_blockTimestamp);
        }

        public uint ReadFromStream(Stream stream)
        {
            uint result = 0;

            result += stream.ReadX2String(ref _previousHash, 32);
            result += stream.ReadX2String(ref _dataHash, 32);
            result += stream.ReadUInt(ref _index);
            result += stream.ReadUInt(ref _blockTimestamp);

            return result;
        }

        private byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                WriteToStream(ms);
                return ms.ToArray();
            }
        }

        public void Init()
        {
            _previousHash = Hasher.EmtpySha256;
            _dataHash = Hasher.EmtpySha256;
            _index = 0;
            _blockTimestamp = 0;
        }
    }

    public class Block
    {
        private BlockHeader _header = new BlockHeader();
        private string _data = "";

        public BlockHeader Header
        {
            get { return _header; }
        }

        public string Data
        {
            get { return _data; }
            set
            {
                // check data when setting directly
                // if hash == default
                //      allow set
                // fail

                if (_header.DataHash != Hasher.EmtpySha256)
                {
                    throw new ArgumentException(nameof(Data));
                }
                var hash = Hasher.Sha256(Encoding.UTF8.GetBytes(value));
                if (_header.DataHash == Hasher.EmtpySha256)
                {
                    _header.DataHash = hash;
                    _data = value;
                }
                else if (_header.DataHash == hash)
                {
                    _data = value;
                }
                else
                {
                    throw new ArgumentException(nameof(hash));
                }
            }
        }

        public Block()
        {
            _header.Init();
        }

        public Block(string previousHash, string dataHash, uint index, uint timeStamp)
        {
            _header.PreviousHash = previousHash;
            _header.DataHash = dataHash;
            _header.Index = index;
            _header.BlockTimestamp = timeStamp;
        }

        public void WriteToStream(Stream ms)
        {
            // write the header
            // write the data
            Header.WriteToStream(ms);
            ms.WriteString(Data);
        }

        public void ReadFromStream(Stream stream, uint size)
        {
            var bytesRead = Header.ReadFromStream(stream);
            stream.ReadString(ref _data, (int) (size - bytesRead));
        }

        private byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                WriteToStream(ms);
                return ms.ToArray();
            }
        }

        public uint Size()
        {
            using (var ms = new MemoryStream())
            {
                WriteToStream(ms);
                return (uint) ms.Length;
            }
        }
    }
}
