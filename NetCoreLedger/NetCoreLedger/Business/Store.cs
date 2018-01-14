using System;
using System.Collections.Generic;
using System.IO;
using NetCoreLedger.Domain;
using NetCoreLedger.Utils;

namespace NetCoreLedger.Business
{
    public class Store
    {
        private readonly Dictionary<string, uint> _idHashPositionLookup = new Dictionary<string, uint>();
        private const string FileName = "ridb.rdb";
        private readonly string _path;

        private uint _lastPosition = 0;
        public const int BufferSize = 1024 * 4;

        public Store(string folder)
        {
            // Setup
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            _path = Path.Combine(folder, FileName);

            if (!File.Exists(_path))
            {
                var file = File.Create(_path);
                file.Close();
            }
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

        public uint Append(Block block)
        {
            var position = SeekEnd();
            var stored = CreateStorageItem(block.Header.GetHash(), position, block);
            Write(stored);
            _lastPosition = position + stored.Size();

            _idHashPositionLookup.Add(block.Header.GetHash(), position);

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
            foreach (var storageItem in EnumerateFile())
            {
                // validate block 
                storageItem.Block.ValidateDataIntegrity();
                if (storageItem.Block.Header.PreviousHash != Hasher.ZeroSha256 && !chain.Last.BlockHash.Equals(storageItem.Block.Header.PreviousHash))
                {
                    throw new DataValidityException();
                }

                // add to chain
                chain.AddLast(storageItem.Block.Header);

                _idHashPositionLookup.Add(storageItem.Block.Header.GetHash(), storageItem.Position);
            }
        }

        public void RemoveBackingFile()
        {
            try
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        public StorageItem FindBlockById(string idHash, uint startPosition = 0, uint endPosition = uint.MaxValue)
        {
            if (_idHashPositionLookup.TryGetValue(idHash, out var position))
            {
                return FindBlockByPosition(position, endPosition);
            }

            return null;
        }

        public StorageItem FindBlockByPosition(uint startPosition = 0, uint endPosition = uint.MaxValue)
        {
            using (var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize))
            {
                fs.Position = startPosition;

                if (fs.Position > endPosition)
                {
                    return null;
                }

                return ReadStorageItemForPosition(fs, startPosition);
            }
        }

        private StorageItem ReadStorageItemForPosition(Stream stream, uint position)
        {
            var item = new StorageItem(position);
            item.ReadFromStream(stream, false);
            return item;
        }

        public void Validate()
        {
            if (!File.Exists(_path)) throw new StoreInvalidException();

            StorageItem previous = null;
            foreach (var storageItem in EnumerateFile())
            {
                if (storageItem.Header.IdHash != storageItem.Block.Header.GetHash()) throw new StoreInvalidException();
                storageItem.Block.ValidateDataIntegrity();

                if (previous != null && storageItem.Block.Header.PreviousHash != previous.Block.Header.GetHash()) throw new StoreInvalidException();

                previous = storageItem;
            }
        }
    }
}