using System;

namespace Pingouin.Level5.Archive.XFSA
{
    public static class XFSASupport
    {
        public struct Header
        {
            public UInt32 Magic;
            public int DirectoryEntriesOffset;
            public int DirectoryHashOffset;
            public int FileEntriesOffset;
            public int NameOffset;
            public int DataOffset;
            public short DirectoryEntriesCount;
            public short DirectoryHashCount;
            public int FileEntriesCount;
            public int TableChunkSize;
        }

        public struct DirectoryEntry
        {
            public uint Crc32;
            public uint Tmp1;
            public ushort FirstFileIndex;
            public ushort FirstDirectoryIndex;
            public uint Tmp2;

            public long FileNameStartOffset
            {
                get => Tmp1 >> 14;
                set => Tmp1 = (uint)((Tmp1 & 0x3FFF) | (value << 14));
            }

            public long DirectoryNameStartOffset
            {
                get => Tmp2 >> 14;
                set => Tmp2 = (uint)((Tmp2 & 0x3FFF) | (value << 14));
            }

            public int FileCount
            {
                get => (int)(Tmp1 & 0x3FFF);
                set => Tmp1 = (uint)((Tmp1 & ~0x3FFFu) | (value & 0x3FFFu));
            }

            public int DirectoryCount
            {
                get => (int)(Tmp2 & 0x3FFF);
                set => Tmp2 = (uint)((Tmp2 & ~0x3FFFu) | (value & 0x3FFFu));
            }
        }

        public struct FileEntry
        {
            public uint Crc32;
            public uint Tmp1;
            public uint Tmp2;

            public long FileOffset
            {
                get => (Tmp1 & 0x03FFFFFF) << 4;
                set => Tmp1 = (uint)((Tmp1 & ~0x03FFFFFF) | ((value >> 4) & 0x03FFFFFF));
            }

            public long FileSize
            {
                get => Tmp2 & 0x007FFFFF;
                set => Tmp2 = (uint)((Tmp2 & ~0x007FFFFF) | (value & 0x007FFFFF));
            }

            public long NameOffsetInFolder
            {
                get => (Tmp1 >> 26 << 9) | (Tmp2 >> 23);
                set
                {
                    Tmp1 = (uint)((Tmp1 & 0x03FFFFFF) | (value >> 9 << 26));
                    Tmp2 = (uint)((Tmp2 & 0x007FFFFF) | (value << 23));
                }
            }
        }
    }
}
