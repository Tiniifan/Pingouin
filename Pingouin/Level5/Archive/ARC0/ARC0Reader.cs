using System.IO;
using System.Linq;
using System.Text;
using Pingouin.Tools;
using Pingouin.Level5.Compression;

namespace Pingouin.Level5.Archive.ARC0
{
    public class ARC0Reader
    {
        private readonly Stream _baseStream;

        public ARC0Reader(Stream stream)
        {
            _baseStream = stream;
        }

        public (VirtualDirectory directory, ARC0Support.Header header) Read()
        {
            VirtualDirectory folder = new VirtualDirectory();

            BinaryDataReader data = new BinaryDataReader(_baseStream);
            var header = data.ReadStruct<ARC0Support.Header>();

            // directory entries
            data.Seek((uint)header.DirectoryEntriesOffset);
            byte[] directoryEntriesComp = data.GetSection(header.DirectoryHashOffset - header.DirectoryEntriesOffset);
            var directoryEntries = DecompressBlockTo<ARC0Support.DirectoryEntry>(directoryEntriesComp, header.DirectoryEntriesCount);

            // directory hashes
            data.Seek((uint)header.DirectoryHashOffset);
            byte[] directoryHashesComp = data.GetSection(header.FileEntriesOffset - header.DirectoryHashOffset);
            var directoryHashes = DecompressBlockTo<uint>(directoryHashesComp, header.DirectoryHashCount);

            // File Entry Table
            data.Seek((uint)header.FileEntriesOffset);
            var fileEntriesComp = data.GetSection(header.NameOffset - header.FileEntriesOffset);
            var fileEntries = DecompressBlockTo<ARC0Support.FileEntry>(fileEntriesComp, header.FileEntriesCount);

            // NameTable
            data.Seek((uint)header.NameOffset);
            byte[] fileNamesComp = Compressor.Decompress(data.GetSection(header.DataOffset - header.NameOffset));
            BinaryDataReader fileNames = new BinaryDataReader(fileNamesComp);

            foreach (var directory in directoryEntries)
            {
                fileNames.Seek((uint)directory.DirectoryNameStartOffset);
                string directoryName = fileNames.ReadString(Encoding.GetEncoding("Shift-JIS"));
                VirtualDirectory newFolder = new VirtualDirectory(directoryName);

                var filesInDirectory = fileEntries.Skip(directory.FirstFileIndex).Take(directory.FileCount);

                foreach (var file in filesInDirectory)
                {
                    fileNames.Seek((uint)(directory.FileNameStartOffset + file.NameOffsetInFolder));
                    var fileName = fileNames.ReadString(Encoding.GetEncoding("Shift-JIS"));

                    if (directoryName == "")
                    {
                        folder.AddFile(fileName, new SubMemoryStream(_baseStream, header.DataOffset + file.FileOffset, file.FileSize));
                    }
                    else
                    {
                        newFolder.AddFile(fileName, new SubMemoryStream(_baseStream, header.DataOffset + file.FileOffset, file.FileSize));
                    }
                }

                folder.AddFolder(newFolder);
            }

            folder.Reorganize();
            folder.SortAlphabetically();

            return (folder, header);
        }

        private T[] DecompressBlockTo<T>(byte[] data, int count)
        {
            BinaryDataReader tableDecomp = new BinaryDataReader(Compressor.Decompress(data));
            return tableDecomp.ReadMultipleStruct<T>(count);
        }
    }
}
