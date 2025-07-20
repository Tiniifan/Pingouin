using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Pingouin.Tools
{
    public class VirtualDirectory
    {
        public string Name;

        public List<VirtualDirectory> Folders;

        public Dictionary<string, SubMemoryStream> Files;

        public Color Color = Color.Black;

        // Cache pour améliorer les performances de recherche
        private Dictionary<string, VirtualDirectory> _folderCache;
        private readonly object _cacheLock = new object();

        public VirtualDirectory()
        {
            Folders = new List<VirtualDirectory>();
            Files = new Dictionary<string, SubMemoryStream>();
            _folderCache = new Dictionary<string, VirtualDirectory>();
        }

        public VirtualDirectory(string name)
        {
            Name = name;
            Folders = new List<VirtualDirectory>();
            Files = new Dictionary<string, SubMemoryStream>();
            _folderCache = new Dictionary<string, VirtualDirectory>();
        }

        public VirtualDirectory GetFolder(string name)
        {
            // Utilisation du cache pour éviter les recherches répétées
            if (_folderCache.TryGetValue(name, out VirtualDirectory cachedFolder))
            {
                return cachedFolder;
            }

            var folder = Folders.FirstOrDefault(f => f.Name == name);

            if (folder != null)
            {
                lock (_cacheLock)
                {
                    _folderCache[name] = folder;
                }
            }

            return folder;
        }

        public VirtualDirectory GetFolderFromFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return this;

            var pathSplit = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var current = this;

            // Get Path
            for (int i = 0; i < pathSplit.Length; i++)
            {
                current = current.GetFolder(pathSplit[i]);

                if (current == null)
                {
                    throw new DirectoryNotFoundException(path + " not exist");
                }
            }

            return current;
        }

        public bool IsFolderExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            var pathSplit = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var current = this;

            // Get Path
            for (int i = 0; i < pathSplit.Length; i++)
            {
                current = current.GetFolder(pathSplit[i]);

                if (current == null)
                {
                    return false;
                }
            }

            return true;
        }

        public List<VirtualDirectory> GetAllFolders()
        {
            // Utilisation de la capacité initiale pour éviter les redimensionnements
            var allFolders = new List<VirtualDirectory>(Folders.Count);
            allFolders.AddRange(Folders);
            return allFolders;
        }

        public Dictionary<string, VirtualDirectory> GetAllFoldersAsDictionnary()
        {
            var directories = new Dictionary<string, VirtualDirectory> { { Name + "/", this } };
            var stack = new Stack<(VirtualDirectory folder, string parentPath)>();

            // Initialiser la pile avec les dossiers de niveau 1
            foreach (var folder in Folders)
            {
                stack.Push((folder, Name + "/"));
            }

            // Parcours itératif au lieu de récursif pour éviter les stack overflow
            while (stack.Count > 0)
            {
                var (currentFolder, parentPath) = stack.Pop();
                var currentPath = parentPath + currentFolder.Name + "/";

                if (!directories.ContainsKey(currentPath))
                {
                    directories.Add(currentPath, currentFolder);
                }

                // Ajouter les sous-dossiers à la pile
                foreach (var subFolder in currentFolder.Folders)
                {
                    stack.Push((subFolder, currentPath));
                }
            }

            return directories;
        }

        public byte[] GetFileFromFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty");

            var pathSplit = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var fileName = pathSplit[pathSplit.Length - 1];
            var current = this;

            // Get Path
            for (int i = 0; i < pathSplit.Length - 1; i++)
            {
                current = current.GetFolder(pathSplit[i]);

                if (current == null)
                {
                    throw new DirectoryNotFoundException(path + " not exist");
                }
            }

            if (current.Files.TryGetValue(fileName, out SubMemoryStream subMemoryStream))
            {
                // Fill subMemoryStream
                if (subMemoryStream.ByteContent == null)
                {
                    subMemoryStream.Read();
                }

                return subMemoryStream.ByteContent;
            }
            else
            {
                throw new FileNotFoundException(fileName + " not exist");
            }
        }

        public Dictionary<string, SubMemoryStream> GetAllFiles()
        {
            var allFiles = new Dictionary<string, SubMemoryStream>();
            var stack = new Stack<(VirtualDirectory folder, string path)>();
            stack.Push((this, Name));

            // Parcours itératif pour éviter les problèmes de récursion
            while (stack.Count > 0)
            {
                var (currentFolder, currentPath) = stack.Pop();

                // Ajouter les fichiers du dossier actuel
                foreach (var file in currentFolder.Files)
                {
                    var filePath = currentPath + "/" + file.Key;
                    if (!allFiles.ContainsKey(filePath))
                    {
                        allFiles.Add(filePath, file.Value);
                    }
                }

                // Ajouter les sous-dossiers à la pile
                foreach (var subFolder in currentFolder.Folders)
                {
                    stack.Push((subFolder, currentPath + "/" + subFolder.Name));
                }
            }

            return allFiles;
        }

        public void AddFile(string name, SubMemoryStream data)
        {
            Files[name] = data; // Utiliser l'indexeur pour éviter les exceptions si la clé existe déjà
        }

        public void AddFolder(string name)
        {
            var newFolder = new VirtualDirectory(name);
            Folders.Add(newFolder);

            // Invalider le cache
            lock (_cacheLock)
            {
                _folderCache.Clear();
            }
        }

        public void AddFolder(VirtualDirectory folder)
        {
            Folders.Add(folder);

            // Invalider le cache
            lock (_cacheLock)
            {
                _folderCache.Clear();
            }
        }

        public long GetSize()
        {
            long size = 0;

            // Utiliser Parallel.ForEach pour les fichiers si la collection est grande
            if (Files.Count > 100)
            {
                var fileSizes = new ConcurrentBag<long>();
                Parallel.ForEach(Files.Values, file =>
                {
                    fileSizes.Add(file.ByteContent?.Length ?? file.Size);
                });
                size = fileSizes.Sum();
            }
            else
            {
                foreach (var file in Files.Values)
                {
                    size += file.ByteContent?.Length ?? file.Size;
                }
            }

            // Calcul parallèle pour les dossiers
            if (Folders.Count > 1)
            {
                var folderSizes = new ConcurrentBag<long>();
                Parallel.ForEach(Folders, folder =>
                {
                    folderSizes.Add(folder.GetSize());
                });
                size += folderSizes.Sum();
            }
            else
            {
                foreach (var folder in Folders)
                {
                    size += folder.GetSize();
                }
            }

            return size;
        }

        public void Reorganize()
        {
            // Retrieve all folders and order them by name
            var folders = GetAllFolders().OrderBy(x => x.Name).ToArray();

            // Iterate through each folder path
            foreach (var folderPath in folders)
            {
                // Split the path into individual folder names
                var pathSplit = folderPath.Name.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var currentPath = "";

                // Iterate through each folder name in the path
                foreach (var folderName in pathSplit)
                {
                    // Build the current path
                    currentPath = Path.Combine(currentPath, folderName) + "\\";

                    // Check if the folder doesn't exist and has a valid depth
                    if (currentPath.Count(c => c == '\\') > 1 && GetFolder(currentPath.Replace("\\", "/")) == null)
                    {
                        // Add the folder to the virtual directory
                        AddFolder(currentPath.Replace("\\", "/"));
                    }
                }
            }

            // Reorganize folders with multiple levels
            folders = GetAllFolders().OrderBy(x => x.Name).ToArray();
            var result = new VirtualDirectory("");
            result.Files = new Dictionary<string, SubMemoryStream>(Files); // Copie optimisée

            // Iterate through each folder in ordered folders
            foreach (var folder in folders.Where(x => !string.IsNullOrEmpty(x.Name)))
            {
                // Split the folder name into individual parts
                var path = folder.Name.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var current = result;

                // Traverse through the folder structure
                for (int i = 0; i < path.Length; i++)
                {
                    // Check if the folder doesn't exist
                    if (current.GetFolder(path[i]) == null)
                    {
                        // Create a new folder and assign files
                        var newFolder = new VirtualDirectory(path[i])
                        {
                            Files = new Dictionary<string, SubMemoryStream>(folder.Files)
                        };
                        current.AddFolder(newFolder);
                    }

                    // Move to the next level in the virtual directory
                    current = current.GetFolder(path[i]);
                }
            }

            // Update the files and folders in the current virtual directory
            Files = result.Files;
            Folders = result.Folders;

            // Invalider le cache après la réorganisation
            lock (_cacheLock)
            {
                _folderCache.Clear();
            }
        }

        public void SortAlphabetically()
        {
            // Tri plus efficace avec StringComparer
            Folders.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

            // Parallélisation pour les gros dossiers
            if (Folders.Count > 10)
            {
                Parallel.ForEach(Folders, folder => folder.SortAlphabetically());
            }
            else
            {
                foreach (var folder in Folders)
                {
                    folder.SortAlphabetically();
                }
            }

            // Tri des fichiers plus efficace
            if (Files.Count > 1)
            {
                var sortedFiles = Files.OrderBy(file => file.Key, StringComparer.OrdinalIgnoreCase)
                                      .ToDictionary(pair => pair.Key, pair => pair.Value);
                Files = sortedFiles;
            }
        }

        public void Print()
        {
            Print(this);
        }

        public void Print(VirtualDirectory directory, int level = 0)
        {
            var indentation = new string('\t', level);
            Console.WriteLine($"{indentation}/{directory.Name}: ");

            foreach (var subDirectory in directory.Folders)
            {
                Print(subDirectory, level + 1);
            }

            foreach (var files in directory.Files)
            {
                var fileIndentation = new string('\t', level + 1);
                Console.WriteLine($"{fileIndentation}{files.Key}");
            }
        }

        public List<VirtualDirectory> SearchDirectories(string directoryName)
        {
            var matchingFolders = new ConcurrentBag<VirtualDirectory>();

            // Search in the current folder
            if (!string.IsNullOrEmpty(Name) && Name.IndexOf(directoryName, StringComparison.OrdinalIgnoreCase) != -1)
            {
                matchingFolders.Add(this);
            }

            // Parallélisation conditionnelle
            if (Folders.Count > 5)
            {
                var folderPartitions = Partitioner.Create(Folders, true);
                Parallel.ForEach(folderPartitions, subFolder =>
                {
                    var subFolderMatches = subFolder.SearchDirectories(directoryName);
                    foreach (var match in subFolderMatches)
                    {
                        matchingFolders.Add(match);
                    }
                });
            }
            else
            {
                foreach (var subFolder in Folders)
                {
                    var subFolderMatches = subFolder.SearchDirectories(directoryName);
                    foreach (var match in subFolderMatches)
                    {
                        matchingFolders.Add(match);
                    }
                }
            }

            return matchingFolders.ToList();
        }

        public List<KeyValuePair<string, SubMemoryStream>> SearchFiles(string fileName)
        {
            var matchingFiles = new ConcurrentBag<KeyValuePair<string, SubMemoryStream>>();

            // Search in the current folder - optimisé avec LINQ
            var currentMatches = Files.Where(x => x.Key.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1);
            foreach (var match in currentMatches)
            {
                matchingFiles.Add(match);
            }

            // Parallélisation conditionnelle
            if (Folders.Count > 5)
            {
                var folderPartitions = Partitioner.Create(Folders, true);
                Parallel.ForEach(folderPartitions, subFolder =>
                {
                    var subFolderMatches = subFolder.SearchFiles(fileName);
                    foreach (var match in subFolderMatches)
                    {
                        matchingFiles.Add(match);
                    }
                });
            }
            else
            {
                foreach (var subFolder in Folders)
                {
                    var subFolderMatches = subFolder.SearchFiles(fileName);
                    foreach (var match in subFolderMatches)
                    {
                        matchingFiles.Add(match);
                    }
                }
            }

            return matchingFiles.ToList();
        }

        public List<KeyValuePair<string, SubMemoryStream>> SearchFiles(VirtualDirectory root, string fileName)
        {
            var matchingFiles = new ConcurrentBag<KeyValuePair<string, SubMemoryStream>>();

            string fullPath = GetFullPath(root);

            // Search in the current folder
            var currentMatches = Files.Where(x => x.Key.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) != -1);
            foreach (var file in currentMatches)
            {
                matchingFiles.Add(new KeyValuePair<string, SubMemoryStream>("/" + fullPath + "/" + file.Key, file.Value));
            }

            // Parallélisation conditionnelle
            if (Folders.Count > 5)
            {
                var folderPartitions = Partitioner.Create(Folders, true);
                Parallel.ForEach(folderPartitions, subFolder =>
                {
                    var subFolderMatches = subFolder.SearchFiles(root, fileName);
                    foreach (var match in subFolderMatches)
                    {
                        matchingFiles.Add(match);
                    }
                });
            }
            else
            {
                foreach (var subFolder in Folders)
                {
                    var subFolderMatches = subFolder.SearchFiles(root, fileName);
                    foreach (var match in subFolderMatches)
                    {
                        matchingFiles.Add(match);
                    }
                }
            }

            return matchingFiles.ToList();
        }

        public string GetFullPath(VirtualDirectory root)
        {
            return GetFullPath(root, this);
        }

        public string GetFullPath(VirtualDirectory currentDirectory, VirtualDirectory searchedDirectory)
        {
            // Utilisation d'une pile pour éviter la récursion profonde
            var stack = new Stack<(VirtualDirectory dir, string path)>();
            stack.Push((currentDirectory, ""));

            while (stack.Count > 0)
            {
                var (current, currentPath) = stack.Pop();

                foreach (var directory in current.Folders)
                {
                    if (directory == searchedDirectory)
                    {
                        return string.IsNullOrEmpty(currentPath) ? directory.Name : currentPath + "/" + directory.Name;
                    }

                    var newPath = string.IsNullOrEmpty(currentPath) ? directory.Name : currentPath + "/" + directory.Name;
                    stack.Push((directory, newPath));
                }
            }

            return string.Empty;
        }

        public void ResetColor()
        {
            Color = Color.Black;

            // Parallélisation pour les gros volumes
            if (Files.Count > 100)
            {
                Parallel.ForEach(Files.Values, file =>
                {
                    file.Color = Color.Black;
                });
            }
            else
            {
                foreach (var file in Files.Values)
                {
                    file.Color = Color.Black;
                }
            }

            if (Folders.Count > 10)
            {
                Parallel.ForEach(Folders, subFolder =>
                {
                    subFolder.ResetColor();
                });
            }
            else
            {
                foreach (var subFolder in Folders)
                {
                    subFolder.ResetColor();
                }
            }
        }
    }
}