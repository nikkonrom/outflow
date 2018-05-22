using System;

namespace Outflow
{
    public static class TorrentConverter
    {
        public static string ConvertBytesSpeed(int speed)
        {
            if (speed < 1024)
                return String.Concat(speed, " B/s");
            if (speed < 1048576)
                return String.Concat(Math.Round(speed / 1024.0, 1), " KB/s");
            if (speed < 1073741824)
                return String.Concat(Math.Round(speed / 1048576.0, 1), " MB/s");
            return String.Concat(Math.Round(speed / 1073741824.0, 1), " GB/s");
        }

        public static string ConvertBytesSize(long size)
        {
            if (size < 1024)
                return String.Concat(size, " B");
            if (size < 1048576)
                return String.Concat(Math.Round(size / 1024.0, 1), " KB");
            if (size < 1073741824)
                return String.Concat(Math.Round(size / 1048576.0, 1), " MB");
            return String.Concat(Math.Round(size / 1073741824.0, 1), " GB");
        }
    }
}
