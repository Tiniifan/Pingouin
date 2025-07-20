using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pingouin.Tools
{
    public class SubMemoryStream
    {
        public long Offset;
        public long Size;
        public byte[] ByteContent;
        public Stream BaseStream;
        public Color Color = Color.Black;

        // Cache pour optimiser les accès répétés
        private bool _isContentLoaded = false;
        private readonly object _readLock = new object();

        // Buffer réutilisable pour éviter les allocations répétées
        private static readonly ThreadLocal<byte[]> _threadLocalBuffer = new ThreadLocal<byte[]>(() => new byte[81920]); // 80KB buffer

        public SubMemoryStream(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Offset = 0;
            Size = data.Length;
            ByteContent = data;
            _isContentLoaded = true;
        }

        public SubMemoryStream(Stream baseStream, long offset, long size)
        {
            if (baseStream == null)
                throw new ArgumentNullException(nameof(baseStream));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative");
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be non-negative");

            Offset = offset;
            Size = size;
            BaseStream = baseStream;
            _isContentLoaded = false;
        }

        public void Read()
        {
            // Double-checked locking pour éviter les lectures multiples
            if (_isContentLoaded)
                return;

            lock (_readLock)
            {
                if (_isContentLoaded)
                    return;

                if (BaseStream != null)
                {
                    // Vérifications de sécurité
                    if (!BaseStream.CanRead)
                        throw new InvalidOperationException("BaseStream is not readable");

                    if (!BaseStream.CanSeek)
                        throw new InvalidOperationException("BaseStream is not seekable");

                    // Vérifier que nous ne dépassons pas la taille du stream
                    if (BaseStream.Length < Offset + Size)
                        throw new InvalidOperationException("Stream is too short for the requested range");

                    ByteContent = new byte[Size];

                    // Optimisation : lecture directe si possible
                    if (Size > 0)
                    {
                        BaseStream.Seek(Offset, SeekOrigin.Begin);
                        int totalBytesRead = 0;

                        // Lecture par chunks pour gérer les gros fichiers
                        while (totalBytesRead < Size)
                        {
                            int bytesToRead = (int)Math.Min(Size - totalBytesRead, 65536); // 64KB chunks
                            int bytesRead = BaseStream.Read(ByteContent, totalBytesRead, bytesToRead);

                            if (bytesRead == 0)
                                throw new EndOfStreamException("Unexpected end of stream");

                            totalBytesRead += bytesRead;
                        }
                    }

                    _isContentLoaded = true;
                }
            }
        }

        public async Task ReadAsync(CancellationToken cancellationToken = default)
        {
            // Version asynchrone pour de meilleures performances I/O
            if (_isContentLoaded)
                return;

            // Utilisation d'un SemaphoreSlim pour l'async
            using (var semaphore = new SemaphoreSlim(1, 1))
            {
                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    if (_isContentLoaded)
                        return;

                    if (BaseStream != null)
                    {
                        if (!BaseStream.CanRead)
                            throw new InvalidOperationException("BaseStream is not readable");

                        if (!BaseStream.CanSeek)
                            throw new InvalidOperationException("BaseStream is not seekable");

                        if (BaseStream.Length < Offset + Size)
                            throw new InvalidOperationException("Stream is too short for the requested range");

                        ByteContent = new byte[Size];

                        if (Size > 0)
                        {
                            BaseStream.Seek(Offset, SeekOrigin.Begin);
                            int totalBytesRead = 0;

                            while (totalBytesRead < Size)
                            {
                                int bytesToRead = (int)Math.Min(Size - totalBytesRead, 65536);
                                int bytesRead = await BaseStream.ReadAsync(ByteContent, totalBytesRead, bytesToRead, cancellationToken);

                                if (bytesRead == 0)
                                    throw new EndOfStreamException("Unexpected end of stream");

                                totalBytesRead += bytesRead;
                            }
                        }

                        _isContentLoaded = true;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        public void Seek()
        {
            if (BaseStream == null)
                throw new InvalidOperationException("BaseStream is null");

            if (!BaseStream.CanSeek)
                throw new InvalidOperationException("BaseStream is not seekable");

            BaseStream.Seek(Offset, SeekOrigin.Begin);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > buffer.Length)
                throw new ArgumentException("Buffer too small");

            if (BaseStream == null)
                return 0;

            if (!BaseStream.CanRead)
                throw new InvalidOperationException("BaseStream is not readable");

            // Optimisation : utiliser ByteContent si disponible
            if (_isContentLoaded && ByteContent != null)
            {
                long currentPosition = BaseStream.Position - Offset;
                if (currentPosition < 0 || currentPosition >= Size)
                    return 0;

                int bytesToRead = (int)Math.Min(count, Size - currentPosition);
                Array.Copy(ByteContent, currentPosition, buffer, offset, bytesToRead);
                return bytesToRead;
            }

            // Calcul optimisé des bytes restants
            long streamPosition = BaseStream.Position;
            long endPosition = Offset + Size;

            if (streamPosition >= endPosition)
                return 0;

            long remainingBytes = endPosition - streamPosition;
            int bytesToReadFromStream = (int)Math.Min(count, remainingBytes);

            // Lecture directe depuis le stream
            return BaseStream.Read(buffer, offset, bytesToReadFromStream);
        }

        public void CopyTo(Stream destination)
        {
            CopyTo(destination, 81920); // 80KB buffer par défaut
        }

        public void CopyTo(Stream destination, int bufferSize)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive");

            if (!destination.CanWrite)
                throw new InvalidOperationException("Destination stream is not writable");

            // Optimisation : utiliser ByteContent si disponible
            if (_isContentLoaded && ByteContent != null)
            {
                destination.Write(ByteContent, 0, ByteContent.Length);
                return;
            }

            // Si pas de BaseStream, essayer de lire d'abord
            if (BaseStream == null)
            {
                if (ByteContent != null)
                {
                    destination.Write(ByteContent, 0, ByteContent.Length);
                }
                return;
            }

            if (!BaseStream.CanRead)
                throw new InvalidOperationException("BaseStream is not readable");

            if (!BaseStream.CanSeek)
                throw new InvalidOperationException("BaseStream is not seekable");

            // Utiliser le buffer thread-local pour éviter les allocations
            byte[] buffer = _threadLocalBuffer.Value;
            if (buffer.Length < bufferSize)
            {
                buffer = new byte[bufferSize];
                _threadLocalBuffer.Value = buffer;
            }

            long currentOffset = Offset;
            long remainingBytes = Size;

            BaseStream.Seek(currentOffset, SeekOrigin.Begin);

            while (remainingBytes > 0)
            {
                int bytesToRead = (int)Math.Min(remainingBytes, buffer.Length);
                int bytesRead = BaseStream.Read(buffer, 0, bytesToRead);

                if (bytesRead == 0)
                    break; // End of stream

                destination.Write(buffer, 0, bytesRead);
                remainingBytes -= bytesRead;
            }
        }

        public async Task CopyToAsync(Stream destination, int bufferSize = 81920, CancellationToken cancellationToken = default)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be positive");

            if (!destination.CanWrite)
                throw new InvalidOperationException("Destination stream is not writable");

            // Optimisation : utiliser ByteContent si disponible
            if (_isContentLoaded && ByteContent != null)
            {
                await destination.WriteAsync(ByteContent, 0, ByteContent.Length, cancellationToken);
                return;
            }

            if (BaseStream == null)
            {
                if (ByteContent != null)
                {
                    await destination.WriteAsync(ByteContent, 0, ByteContent.Length, cancellationToken);
                }
                return;
            }

            if (!BaseStream.CanRead)
                throw new InvalidOperationException("BaseStream is not readable");

            if (!BaseStream.CanSeek)
                throw new InvalidOperationException("BaseStream is not seekable");

            var buffer = new byte[bufferSize];
            long currentOffset = Offset;
            long remainingBytes = Size;

            BaseStream.Seek(currentOffset, SeekOrigin.Begin);

            while (remainingBytes > 0)
            {
                int bytesToRead = (int)Math.Min(remainingBytes, buffer.Length);
                int bytesRead = await BaseStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken);

                if (bytesRead == 0)
                    break;

                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                remainingBytes -= bytesRead;
            }
        }

        // Méthode pour libérer les ressources mémoire si nécessaire
        public void ReleaseMemory()
        {
            lock (_readLock)
            {
                ByteContent = null;
                _isContentLoaded = false;
            }
        }

        // Propriété pour vérifier si le contenu est chargé
        public bool IsContentLoaded => _isContentLoaded;

        // Méthode pour obtenir une portion des données sans charger tout le contenu
        public byte[] GetBytes(int startIndex, int length)
        {
            if (startIndex < 0 || length < 0 || startIndex + length > Size)
                throw new ArgumentOutOfRangeException();

            if (_isContentLoaded && ByteContent != null)
            {
                var result = new byte[length];
                Array.Copy(ByteContent, startIndex, result, 0, length);
                return result;
            }

            if (BaseStream == null)
                throw new InvalidOperationException("No data source available");

            if (!BaseStream.CanRead || !BaseStream.CanSeek)
                throw new InvalidOperationException("BaseStream must be readable and seekable");

            var buffer = new byte[length];
            BaseStream.Seek(Offset + startIndex, SeekOrigin.Begin);
            int bytesRead = BaseStream.Read(buffer, 0, length);

            if (bytesRead < length)
            {
                Array.Resize(ref buffer, bytesRead);
            }

            return buffer;
        }
    }
}