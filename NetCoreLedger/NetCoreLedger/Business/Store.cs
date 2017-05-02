using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetCoreLedger.Extensions;

namespace NetCoreLedger.Business
{

    public class Store
    {
        private const string FileName = "ridb.rdb";

        private readonly string _folder;
        private string _path;

        private uint _lastPosition = 0;
        public const int BufferSize = 1024 * 4;


        public Store(string folder)
        {
            _folder = folder;

            // Setup
            if (!Directory.Exists(_folder))
            {
                Directory.CreateDirectory(_folder);
            }

            _path = Path.Combine(_folder, FileName);
        }

        private uint SeekEnd()
        {
            if (_lastPosition > 0)
            {
                return _lastPosition;
            }

            if (!File.Exists(_path))
            {
                return 0;
            }

            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _lastPosition = (uint)fs.Length;
            }

            return _lastPosition;
        }

        public uint Append(string idHash, Block block)
        {
            var position = SeekEnd();
            var stored = CreateStorageItem(idHash, position, block);
            Write(stored);
            _lastPosition = position + stored.Size();
            return stored.Position;
        }

        public IEnumerable<StorageItem> EnumerateFile(bool headerOnly = false, uint startPosition = 0, uint endPosition = uint.MaxValue)
        {
            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, BufferSize))
            {
                fs.Position = startPosition;

                foreach (var block in Enumerate(fs, headerOnly, endPosition))
                {
                    yield return block;
                }
            }
        }

        private IEnumerable<StorageItem> Enumerate(Stream stream, bool headerOnly, uint endPosition)
        {
            if (stream.Position > endPosition)
            {
                yield break;
            }

            var len = stream.Length;
            while (stream.Position < len)
            {
                var storedItem = ReadStorageItem(stream, headerOnly, (uint)stream.Position);

                yield return storedItem;
                if (stream.Position >= endPosition)
                    break;
            }
        }

        private StorageItem ReadStorageItem(Stream stream, bool headerOnly, uint position)
        {
            var item = new StorageItem(position);
            item.ReadFromStream(stream, headerOnly);
            return item;
        }

        private StorageItem CreateStorageItem(string idHash, uint position, Block block)
        {
            return new StorageItem(idHash, position, block);
        }

        private void Write(StorageItem item)
        {
            using (var fs = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                fs.Position = item.Position;
                item.WriteToStream(fs);
            }
        }

        public void SyncChain(Chain chain)
        {
            // we build that chain
            foreach (var storageItem in EnumerateFile(true))
            {
                // validate block 
                storageItem.Block.ValidateDataIntegrity();
                if (storageItem.Block.Header.PreviousHash != Hasher.ZeroSha256 && !chain.Last.BlockHash.Equals(storageItem.Block.Header.PreviousHash))
                {
                    throw new DataValidityException();
                }

                // add to chain
                chain.AddLast(storageItem.Block.Header);
            }

            
        }
    }

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

    public class StorageItem
    {
        private static byte[] _readableBuffer = new byte[Store.BufferSize];

        public StorageHeader Header { get; set; }
        public Block Block { get; set; }
        public uint Position { get; set; }

        public StorageItem(uint position)
        {
            Position = position;
            Header = new StorageHeader();
            Block = new Block();
        }

        public StorageItem(string idHash, uint position, Block block)
        {
            Position = position;
            Block = block;
            Header = new StorageHeader(idHash, Block.Size());
        }

        public uint Size()
        {
            var ms = new MemoryStream();
            Header.WriteToStream(ms);
            return Header.Size + (uint)ms.Length;
        }

        public void WriteToStream(Stream ms)
        {
            Header.WriteToStream(ms);
            Block.WriteToStream(ms);
        }

        public void ReadFromStream(Stream stream, bool headerOnly)
        {
            Header.ReadFromStream(stream);
            if (!headerOnly)
            {
                Block.ReadFromStream(stream, Header.Size);
            }
            else
            {
                var beginPosition = stream.Position;
                Block.Header.ReadFromStream(stream);

                var remaining = (int) (Header.Size - (stream.Position - beginPosition));
                if (remaining > Store.BufferSize)
                {
                    // we read in sections of Store.BufferSize, since we don't have that already we can use position to be efficient
                    // FE https://stackoverflow.com/questions/3780704/why-does-filestream-position-increment-in-multiples-of-1024
                    stream.Position += remaining;
                }
                else
                {
                    stream.Read(_readableBuffer, 0, remaining);
                }
            }
        }
    }
}