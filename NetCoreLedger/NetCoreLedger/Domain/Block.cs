using System;
using System.IO;
using System.Text;
using NetCoreLedger.Business;
using NetCoreLedger.Extensions;
using NetCoreLedger.Utils;

namespace NetCoreLedger.Domain
{
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

        public Block(string previousHash, uint index, uint timeStamp)
        {
            _header.PreviousHash = previousHash;
            _header.DataHash = Hasher.EmtpySha256;
            _header.Index = index;
            _header.BlockTimestamp = timeStamp;
        }

        public Block(uint index, uint timeStamp)
        {
            _header.PreviousHash = Hasher.ZeroSha256;
            _header.DataHash = Hasher.EmtpySha256;
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

        public void ValidateDataIntegrity()
        {
            // Check the data
            var dataHash = Hasher.Sha256(Encoding.UTF8.GetBytes((string) Data));
            if (!dataHash.Equals(Header.DataHash))
            {
                throw new DataValidityException();
            }
        }
    }
}