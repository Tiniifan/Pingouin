using System;
using System.Runtime.InteropServices;

namespace Pingouin.Level5.Archive.XPCK
{
    public class XPCKSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public UInt32 Magic;
            public byte fc1;
            public byte fc2;
            public ushort tmp1;
            public ushort tmp2;
            public ushort tmp3;
            public ushort tmp4;
            public ushort tmp5;
            public uint tmp6;

            public ushort FileCount => (ushort)((fc2 & 0xf) << 8 | fc1);
            public ushort FileInfoOffset => (ushort)(tmp1 << 2);
            public ushort FilenameTableOffset => (ushort)(tmp2 << 2);
            public ushort DataOffset => (ushort)(tmp3 << 2);
            public ushort FileInfoSize => (ushort)(tmp4 << 2);
            public ushort FilenameTableSize => (ushort)(tmp5 << 2);
            public uint DataSize => tmp6 << 2;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FileEntry
        {
            public uint Crc32;
            public ushort NameOffset;
            public ushort tmp;
            public ushort tmp2;
            public byte tmpZ;
            public byte tmp2Z;

            public uint FileOffset => (((uint)tmpZ << 16) | tmp) << 2;
            public uint FileSize => ((uint)tmp2Z << 16) | tmp2;
        }

        public static byte[] CalculateF1F2(int fileCount)
        {
            int fc1 = fileCount & 0xFF;
            int fc2 = (fileCount >> 8) & 0xF;
            int f1 = fc1;
            int f2 = (fc2 << 8) | fc1;
            return new byte[] { Convert.ToByte(f1), Convert.ToByte(f2) };
        }
    }
}
