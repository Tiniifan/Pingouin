using System;
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
    public class ARC0WriterOptimized
    {
        private readonly VirtualDirectory _directory;
        private readonly ARC0Support.Header _header;
        private static readonly Encoding ShiftJISEncoding = Encoding.GetEncoding("Shift-JIS");
        private static readonly Encoding UTF8Encoding = Encoding.UTF8;

        public ARC0WriterOptimized(VirtualDirectory directory, ARC0Support.Header header)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            _header = header;
        }

        public void Save(string fileName, ProgressBar progressBar = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("Le nom de fichier ne peut pas être vide", nameof(fileName));

            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 8192))
            {
                WriteToStream(stream, progressBar);
            }
        }

        public byte[] Save(ProgressBar progressBar = null)
        {
            using (var memoryStream = new MemoryStream())
            {
                WriteToStream(memoryStream, progressBar);
                return memoryStream.ToArray();
            }
        }

        private void WriteToStream(Stream stream, ProgressBar progressBar = null)
        {
            var writer = new BinaryDataWriter(stream);

            int tableNameOffset = 0;
            int firstFileIndex = 0;
            uint fileOffset = 0;

            var tableName = new List<byte[]>();
            var folders = _directory.GetAllFoldersAsDictionnary();

            var directoryEntries = new List<ARC0Support.DirectoryEntry>(folders.Count);
            var fileEntries = new List<ARC0Support.FileEntry>(folders.Sum(f => f.Value.Files.Count()));
            var files = new Dictionary<ARC0Support.FileEntry, SubMemoryStream>(fileEntries.Capacity);

            var crc32 = new Crc32();

            foreach (var folder in folders)
            {
                string directoryName = folder.Key.Length > 1 ? folder.Key.Substring(1) : string.Empty;
                byte[] directoryNameByte = GetShiftJISBytesWithNull(directoryName);
                tableName.Add(directoryNameByte);

                var directoryEntry = new ARC0Support.DirectoryEntry
                {
                    Crc32 = CalculateCrc32(crc32, UTF8Encoding.GetBytes(directoryName)),
                    DirectoryCount = (short)folder.Value.Folders.Count,
                    FirstFileIndex = (ushort)firstFileIndex,
                    FileCount = (short)folder.Value.Files.Count(),
                    DirectoryNameStartOffset = tableNameOffset,
                    FileNameStartOffset = tableNameOffset + directoryNameByte.Length
                };

                if (directoryEntry.Crc32 == 0)
                    directoryEntry.Crc32 = 0xFFFFFFFF;

                directoryEntries.Add(directoryEntry);

                tableNameOffset += directoryNameByte.Length;
                firstFileIndex += folder.Value.Files.Count();
                int nameOffsetInFolder = 0;

                var fileEntryFromFolder = new List<ARC0Support.FileEntry>();
                var filesInFolder = folder.Value.Files.OrderBy(file => file.Key).ToList();

                foreach (var file in filesInFolder)
                {
                    byte[] fileNameByte = GetShiftJISBytesWithNull(file.Key);
                    tableName.Add(fileNameByte);

                    var entryFile = new ARC0Support.FileEntry
                    {
                        Crc32 = CalculateCrc32(crc32, ShiftJISEncoding.GetBytes(file.Key)),
                        NameOffsetInFolder = (uint)nameOffsetInFolder,
                        FileOffset = fileOffset,
                        FileSize = (uint)(file.Value.ByteContent?.Length ?? file.Value.Size)
                    };

                    fileEntryFromFolder.Add(entryFile);
                    files.Add(entryFile, file.Value);

                    tableNameOffset += fileNameByte.Length;
                    nameOffsetInFolder += fileNameByte.Length;
                    fileOffset = (uint)((fileOffset + entryFile.FileSize + 3) & ~3);
                }

                fileEntryFromFolder.Sort((x, y) => x.Crc32.CompareTo(y.Crc32));
                fileEntries.AddRange(fileEntryFromFolder);
            }

            var directoryIndex = 0;
            directoryEntries.Sort((x, y) => x.Crc32.CompareTo(y.Crc32));

            for (int i = 0; i < directoryEntries.Count; i++)
            {
                var tempEntry = directoryEntries[i];
                tempEntry.FirstDirectoryIndex = (ushort)directoryIndex;
                directoryIndex += tempEntry.DirectoryCount;
                directoryEntries[i] = tempEntry; // ⚠️ structure copiée, puis réaffectée
            }

            var directoryHashes = directoryEntries.Select(e => e.Crc32).ToArray();

            long totalBytes = files.Sum(file => file.Value.Size);
            long bytesWritten = 0;

            writer.Seek(0x48);
            long directoryEntriesOffset = 0x48;
            writer.Write(CompressBlockTo(directoryEntries.ToArray(), new NoCompression()));
            writer.WriteAlignment(4);

            long directoryHashOffset = stream.Position;
            writer.Write(CompressBlockTo(directoryHashes, new NoCompression()));
            writer.WriteAlignment(4);

            long fileEntriesOffset = stream.Position;
            writer.Write(CompressBlockTo(fileEntries.ToArray(), new NoCompression()));
            writer.WriteAlignment(4);

            long fileNameTableOffset = stream.Position;
            byte[] tableNameArray = ConcatenateByteArrays(tableName);
            writer.Write(CompressBlockTo(tableNameArray, new NoCompression()));
            writer.WriteAlignment(4);

            long dataOffset = stream.Position;

            foreach (var file in fileEntries)
            {
                writer.BaseStream.Position = dataOffset + file.FileOffset;
                files[file].CopyTo(stream);
                bytesWritten += file.FileSize;

                if (progressBar != null && totalBytes > 0)
                {
                    int newValue = (int)((double)bytesWritten / totalBytes * 100);
                    if (newValue != progressBar.Value)
                        progressBar.Value = newValue;
                }
            }

            var header = _header;
            header.Magic = 0x30435241;
            header.DirectoryEntriesOffset = (int)directoryEntriesOffset;
            header.DirectoryHashOffset = (int)directoryHashOffset;
            header.FileEntriesOffset = (int)fileEntriesOffset;
            header.NameOffset = (int)fileNameTableOffset;
            header.DataOffset = (int)dataOffset;
            header.DirectoryEntriesCount = (short)directoryEntries.Count;
            header.DirectoryHashCount = (short)directoryHashes.Length;
            header.FileEntriesCount = fileEntries.Count;
            header.DirectoryCount = directoryEntries.Count;
            header.FileCount = fileEntries.Count;
            header.TableChunkSize = ((directoryEntries.Count * 20 +
                                      directoryHashes.Length * 4 +
                                      fileEntries.Count * 16 +
                                      tableNameArray.Length + 0x20 + 3) & ~3);

            writer.Seek(0);
            writer.WriteStruct(header);
        }

        private byte[] CompressBlockTo<T>(T[] data, ICompression compression)
        {
            byte[] serializedData = SerializeData(data);
            return compression.Compress(serializedData);
        }

        private byte[] SerializeData<T>(T[] data)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryDataWriter(stream))
                {
                    writer.WriteMultipleStruct<T>(data);
                }
                return stream.ToArray();
            }
        }

        private byte[] GetShiftJISBytesWithNull(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new byte[] { 0 };

            var bytes = ShiftJISEncoding.GetBytes(text);
            var result = new byte[bytes.Length + 1];
            Array.Copy(bytes, result, bytes.Length);
            result[bytes.Length] = 0;
            return result;
        }

        private uint CalculateCrc32(Crc32 crc32, byte[] data)
        {
            var hash = crc32.ComputeHash(data);
            return BitConverter.ToUInt32(hash.Reverse().ToArray(), 0);
        }

        private byte[] ConcatenateByteArrays(List<byte[]> arrays)
        {
            int totalLength = arrays.Sum(arr => arr.Length);
            var result = new byte[totalLength];
            int offset = 0;

            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }
    }
}
