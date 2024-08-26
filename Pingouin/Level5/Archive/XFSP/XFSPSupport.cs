using System;
using System.Runtime.InteropServices;

namespace Pingouin.Level5.Archive.XFSP
{
    public class XFSPSupport
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Header
        {
            public uint Magic;
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
            public short Unk;
            public ushort NameOffset;
            public ushort tmp;
            public ushort tmp2;
            public byte tmpZ;
            public byte tmp2Z;

            public uint FileOffset => (((uint)tmpZ << 16) | tmp) << 2;
            public uint FileSize => ((uint)tmp2Z << 16) | tmp2;
        }

        public static (int highByte, int lowByte) FileCountToHex(int fileCount)
        {
            int var2 = 0;

            for (int i = 0; i < 12; i++)
            {
                var2 = (int)Math.Pow(2, i);
                if (var2 > fileCount)
                {
                    var2 = i;
                    break;
                }
            }

            int result = ((var2 << 12) | fileCount) & 0xFFFF;

            int highByte = (result >> 8) & 0xFF;
            int lowByte = result & 0xFF;

            return (highByte, lowByte);
        }
    }
}
