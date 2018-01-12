using System.IO;
using NetCoreLedger.Business;

namespace NetCoreLedger.Domain
{
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

        public void ReadFromStreamForId(Stream stream, string idHash)
        {
            Header.ReadFromStream(stream);

            if (Header.IdHash == idHash)
            {
                Block.ReadFromStream(stream, Header.Size);
            }
            else
            {
                var remaining = (int)(Header.Size);
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