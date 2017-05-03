using System;
using System.IO;
using NetCoreLedger.Business;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Domain
{
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
}