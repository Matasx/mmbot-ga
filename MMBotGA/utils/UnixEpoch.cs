using System;

namespace MMBotGA.utils
{
    internal static class UnixEpoch
    { 
        public static DateTime GetDateTimeMs(long epoch) => DateTime.UnixEpoch.AddMilliseconds(epoch);
        public static long GetEpochMs(DateTime dateTime) => (long)(dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalMilliseconds;
    }
}
