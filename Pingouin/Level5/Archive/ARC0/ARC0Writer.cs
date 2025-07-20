using System.IO;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using Pingouin.Tools;
using Pingouin.Level5.Compression;
using Pingouin.Level5.Compression.NoCompression;

namespace Pingouin.Level5.Archive.ARC0
{
    public class ARC0Writer
    {
        private readonly VirtualDirectory _directory;
        private readonly ARC0Support.Header _header;

        public ARC0Writer(VirtualDirectory directory, ARC0Support.Header header)
        {
            _directory = directory;
            _header = header;
        }

        public void Save(string fileName, ProgressBar progressBar = null)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                WriteToStream(stream, progressBar);
            }
        }

        public byte[] Save(ProgressBar progressBar = null)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                WriteToStream(memoryStream, progressBar);
                return memoryStream.ToArray();
            }
        }

        private void WriteToStream(Stream stream, ProgressBar progressBar = null)
        {
            BinaryDataWriter writer = new BinaryDataWriter(stream);

            int tableNameOffset = 0;
            int firstFileIndex = 0;
            uint fileOffset = 0;

            List<byte[]> tableName = new List<byte[]>();
            var directoryEntries = new List<ARC0Support.DirectoryEntry>();
            var fileEntries = new List<ARC0Support.FileEntry>();
            Dictionary<ARC0Support.FileEntry, SubMemoryStream> files = new Dictionary<ARC0Support.FileEntry, SubMemoryStream>();
            Dictionary<string, VirtualDirectory> folders = _directory.GetAllFoldersAsDictionnary();

            foreach (var folder in folders)
            {
                // Remove first slash from directory name
                string directoryName = folder.Key.Substring(1, folder.Key.Length - 1);
                byte[] directoryNameByte = Encoding.GetEncoding("Shift-JIS").GetBytes(directoryName + '\0');
                tableName.Add(directoryNameByte);

                var directoryEntry = new ARC0Support.DirectoryEntry();

                directoryEntry.Crc32 = System.BitConverter.ToUInt32(new Crc32().ComputeHash(Encoding.UTF8.GetBytes(directoryName)).Reverse().ToArray(), 0);
                if (directoryEntry.Crc32 == 0)
                {
                    directoryEntry.Crc32 = 0xFFFFFFFF;
                }

                directoryEntry.DirectoryCount = (short)folder.Value.Folders.Count;
                directoryEntry.FirstFileIndex = (ushort)firstFileIndex;
                directoryEntry.FileCount = (short)folder.Value.Files.Count();
                directoryEntry.DirectoryNameStartOffset = tableNameOffset;
                directoryEntry.FileNameStartOffset = tableNameOffset + directoryNameByte.Length;
                directoryEntries.Add(directoryEntry);

                tableNameOffset += directoryNameByte.Length;
                firstFileIndex += folder.Value.Files.Count();
                int nameOffsetInFolder = 0;

                // File Information
                var fileEntryFromFolder = new List<ARC0Support.FileEntry>();
                Dictionary<string, SubMemoryStream> filesInFolder = folder.Value.Files.OrderBy(file => file.Key).ToDictionary(file => file.Key, file => file.Value);
                foreach (var file in filesInFolder)
                {
                    byte[] fileNameByte = Encoding.GetEncoding("Shift-JIS").GetBytes(file.Key + '\0');
                    tableName.Add(fileNameByte);

                    var entryFile = new ARC0Support.FileEntry();
                    entryFile.Crc32 = System.BitConverter.ToUInt32(new Crc32().ComputeHash(Encoding.GetEncoding("Shift-JIS").GetBytes(file.Key)).Reverse().ToArray(), 0);
                    entryFile.NameOffsetInFolder = (uint)nameOffsetInFolder;
                    entryFile.FileOffset = fileOffset;

                    if (file.Value.ByteContent == null)
                    {
                        entryFile.FileSize = (uint)file.Value.Size;
                    }
                    else
                    {
                        entryFile.FileSize = (uint)file.Value.ByteContent.Length;
                    }

                    fileEntryFromFolder.Add(entryFile);
                    files.Add(entryFile, file.Value);

                    tableNameOffset += fileNameByte.Length;
                    nameOffsetInFolder += fileNameByte.Length;
                    fileOffset = (uint)((fileOffset + entryFile.FileSize + 3) & ~3);
                }

                fileEntries.AddRange(fileEntryFromFolder.OrderBy(x => x.Crc32));
            }

            // Order directory entries by hash and set directoryIndex accordingly
            var directoryIndex = 0;
            directoryEntries = directoryEntries.OrderBy(x => x.Crc32).Select(x =>
            {
                x.FirstDirectoryIndex = (ushort)directoryIndex;
                directoryIndex += x.DirectoryCount;
                return x;
            }).ToList();

            // Calculate directory hashes
            var directoryHashes = directoryEntries.Select(x => x.Crc32).ToList();

            long totalBytes = files.Sum(file => file.Value.Size);
            long bytesWritten = 0;

            // Write the directory entries
            writer.Seek(0x48);
            long directoryEntriesOffset = 0x48;
            writer.Write(CompressBlockTo<ARC0Support.DirectoryEntry>(directoryEntries.ToArray(), new NoCompression()));
            writer.WriteAlignment(4);

            // Write the directory hashes
            long directoryHashOffset = stream.Position;
            writer.Write(CompressBlockTo<uint>(directoryHashes.ToArray(), new NoCompression()));
            writer.WriteAlignment(4);

            // Write the file entries
            long fileEntriesOffset = stream.Position;
            writer.Write(CompressBlockTo<ARC0Support.FileEntry>(fileEntries.ToArray(), new NoCompression()));
            writer.WriteAlignment(4);

            // Write name table
            long fileNameTableOffset = stream.Position;
            byte[] tableNameArray = tableName.SelectMany(bytes => bytes).ToArray();
            writer.Write(CompressBlockTo<byte>(tableNameArray, new NoCompression()));
            writer.WriteAlignment(4);

            // Write the file data
            long dataOffset = stream.Position;
            foreach (ARC0Support.FileEntry file in fileEntries)
            {
                writer.BaseStream.Position = dataOffset + file.FileOffset;
                files[file].CopyTo(stream);
                bytesWritten += file.FileSize;

                // Update progress bar if provided
                if (progressBar != null && totalBytes > 0)
                {
                    progressBar.Value = (int)((double)bytesWritten / totalBytes * 100);
                }
            }

            // Update the header
            var header = _header;
            header.Magic = 0x30435241;
            header.DirectoryEntriesOffset = (int)directoryEntriesOffset;
            header.DirectoryHashOffset = (int)directoryHashOffset;
            header.FileEntriesOffset = (int)fileEntriesOffset;
            header.NameOffset = (int)fileNameTableOffset;
            header.DataOffset = (int)dataOffset;
            header.DirectoryEntriesCount = (short)directoryEntries.Count;
            header.DirectoryHashCount = (short)directoryHashes.Count;
            header.FileEntriesCount = fileEntries.Count;
            header.DirectoryCount = directoryEntries.Count;
            header.FileCount = fileEntries.Count;
            header.TableChunkSize = (int)(directoryEntries.Count * 20 +
                                directoryHashes.Count * 4 +
                                fileEntries.Count * 16 +
                                tableNameArray.Length + 0x20 + 3) & ~3;
            writer.Seek(0);
            writer.WriteStruct(header);
        }

        private byte[] CompressBlockTo<T>(T[] data, ICompression compression)
        {
            byte[] serializedData = SerializeData<T>(data);
            return compression.Compress(serializedData);
        }

        private byte[] SerializeData<T>(T[] data)
        {
            MemoryStream stream = new MemoryStream();

            BinaryDataWriter writer = new BinaryDataWriter(stream);
            writer.WriteMultipleStruct<T>(data);
            writer.Dispose();

            return stream.ToArray();
        }
    }
}