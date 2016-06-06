using System;
using System.IO;

namespace Monitron.Plugins.LocalMonitorPlugin
{
    public static class StreamExtension
    {
        public static void CopyTo(this Stream src, Stream dst, int size)
        {
            byte[] buffer = new byte[0x10000];
            int bytesLeft = size;
            while (bytesLeft > 0)
            {
                int n = src.Read(buffer, 0, Math.Min(buffer.Length, bytesLeft));
                dst.Write(buffer, 0, n);
                bytesLeft -= n;
            }
        }
    }
}

