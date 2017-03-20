using System;

namespace NetCoreLedger.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static uint ToUnixTimeSeconds(this DateTime dateTime)
        {
            dateTime = dateTime.ToUniversalTime();

            if (dateTime < Epoch)
                throw new ArgumentOutOfRangeException(nameof(dateTime));
            var result = dateTime.Subtract(Epoch).TotalSeconds;
            if (result > UInt32.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(result));
            return (uint)result;
        }
    }
}