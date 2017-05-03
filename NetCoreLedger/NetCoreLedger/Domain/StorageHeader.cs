using System.IO;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Domain
{
    public class StorageHeader
    {
        private string _idHash = "";
        private uint _size;

        public string IdHash
        {
            get { return _idHash; }
        }

        public uint Size
        {
            get { return _size; }
        }

        public StorageHeader() { }

        public StorageHeader(string idHash, uint size)
        {
            _idHash = idHash;
            _size = size;
        }

        public void WriteToStream(Stream ms)
        {
            ms.WriteX2String(IdHash);
            ms.WriteInt(Size);
        }

        public void ReadFromStream(Stream stream)
        {
            stream.ReadX2String(ref _idHash, 32);
            stream.ReadUInt(ref _size);
        }
    }
}