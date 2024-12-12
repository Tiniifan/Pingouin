using System;
using System.IO;
using System.Text;
using System.Linq;
using System.IO.Compression;
using Pingouin.Tools;
using Pingouin.Level5.Archive;
using Pingouin.Level5.Archive.XFSP;
using Pingouin.Level5.Archive.XPCK;
using Pingouin.Level5.Archive.ARC0;

namespace Pingouin.Level5.Archive
{
    public static class Archiver
    {
        public static IArchive GetArchive(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
                return GetArchive(stream);
            }
        }

        public static IArchive GetArchive(Stream stream)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("The provided stream doesn't support reading.");
            }

            byte[] magicBytes = new byte[4];
            int bytesRead = stream.Read(magicBytes, 0, 4);

            if (bytesRead < 4)
            {
                throw new ArgumentException("The provided stream is too short to verify the magic number.");
            }

            string magic = Encoding.UTF8.GetString(magicBytes);
            if (magic == "ARC0")
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new ARC0.ARC0(stream);
            } else if (magic == "XFSA")
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new XFSA.XFSA(stream);
            }
            else if (magic == "XFSP")
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new XFSP.XFSP(stream);
            }
            else if (magic == "XPCK")
            {
                stream.Seek(0, SeekOrigin.Begin);
                return new XPCK.XPCK(stream);
            }
            else if (magic.StartsWith("PK\x03\x04"))
            {
                stream.Seek(0, SeekOrigin.Begin);
                VirtualDirectory newDirectory = ConvertZipToVirtualDirectory(stream);

                if (newDirectory.Folders.Any())
                {
                    ARC0.ARC0 arc0 = new ARC0.ARC0();
                    arc0.Directory = newDirectory;
                    return arc0;
                } else
                {
                    XPCK.XPCK xpck = new XPCK.XPCK();
                    xpck.Directory = newDirectory;
                    return xpck;
                }
            }
            else
            {
                return null;
            }
            
        }

        private static VirtualDirectory ConvertZipToVirtualDirectory(Stream zipStream)
        {
            VirtualDirectory root = new VirtualDirectory("/");

            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Read, leaveOpen: true))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string[] pathParts = entry.FullName.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    VirtualDirectory currentDir = root;

                    for (int i = 0; i < pathParts.Length - 1; i++)
                    {
                        string folderName = pathParts[i];
                        var folder = currentDir.GetFolder(folderName);

                        if (folder == null)
                        {
                            folder = new VirtualDirectory(folderName);
                            currentDir.AddFolder(folder);
                        }

                        currentDir = folder;
                    }

                    if (!entry.FullName.EndsWith("/"))
                    {
                        using (MemoryStream fileStream = new MemoryStream())
                        {
                            entry.Open().CopyTo(fileStream);
                            currentDir.AddFile(pathParts.Last(), new SubMemoryStream(fileStream.ToArray()));
                        }
                    }
                }
            }

            return root;
        }
    }
}
