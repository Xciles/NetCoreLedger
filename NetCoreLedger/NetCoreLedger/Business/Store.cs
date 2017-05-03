using System.Collections.Generic;
using System.IO;
using NetCoreLedger.Domain;
using NetCoreLedger.Utils;

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
}