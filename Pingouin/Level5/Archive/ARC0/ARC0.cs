using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Pingouin.Tools;
using Pingouin.Level5.Compression;
using Pingouin.Level5.Compression.NoCompression;

namespace Pingouin.Level5.Archive.ARC0
{
    public class ARC0 : IArchive
    {
        public string Name => "ARC0";
        public VirtualDirectory Directory { get; set; }
        public Stream BaseStream { get; private set; }
        public ARC0Support.Header Header { get; private set; }

        private bool UseOptimized => true;

        public ARC0()
        {
            Directory = new VirtualDirectory("");
        }

        public ARC0(Stream stream)
        {
            BaseStream = stream;
            var reader = new ARC0Reader(stream);
            var result = reader.Read();
            Directory = result.directory;
            Header = result.header;
        }

        public ARC0(byte[] fileByteArray)
        {
            BaseStream = new MemoryStream(fileByteArray);
            var reader = new ARC0Reader(BaseStream);
            var result = reader.Read();
            Directory = result.directory;
            Header = result.header;
        }

        private int CalculateOptimalParallelism()
        {
            // Calcul basé sur le nombre de fichiers et de dossiers
            int totalFiles = Directory.GetAllFiles().Count;
            int totalFolders = Directory.GetAllFoldersAsDictionnary().Count;
            int totalItems = totalFiles + totalFolders;

            // Nombre de processeurs disponibles
            int processorCount = Environment.ProcessorCount;

            // Calcul optimal : 
            // - Pour peu d'éléments (< 100), limiter le parallélisme
            // - Pour beaucoup d'éléments, utiliser plus de threads mais pas trop
            // - Prendre en compte les processeurs disponibles

            if (totalItems < 50)
                return Math.Min(2, processorCount);
            else if (totalItems < 200)
                return Math.Min(processorCount, 4);
            else if (totalItems < 1000)
                return Math.Min(processorCount, 8);
            else
                return Math.Min(processorCount * 2, 16); // Max 16 threads
        }

        public void Save(string fileName, ProgressBar progressBar = null)
        {
            if (UseOptimized)
            {
                var writer = new ARC0WriterOptimized(Directory, Header);
                writer.Save(fileName, progressBar);
            }
            else
            {
                var writer = new ARC0Writer(Directory, Header);
                writer.Save(fileName, progressBar);
            }
        }

        public byte[] Save(ProgressBar progressBar = null)
        {
            if (UseOptimized)
            {
                var writer = new ARC0WriterOptimized(Directory, Header);
                return writer.Save(progressBar);
            }
            else
            {
                var writer = new ARC0Writer(Directory, Header);
                return writer.Save(progressBar);
            }
        }

        public IArchive Close()
        {
            BaseStream?.Dispose();
            BaseStream = null;
            Directory = null;
            return null;
        }
    }
}