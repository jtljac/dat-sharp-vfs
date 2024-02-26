using System.IO.MemoryMappedFiles;
using dat_sharp_vfs.Util;

namespace dat_sharp_vfs.VfsFile;

/// <summary>
/// An implementation of DVfsFile that represents a regular file on the disk
/// </summary>
/// <param name="filePath">The path to the real file on the disk</param>
public class LooseDVfsFile(string filePath) : DVfsFile {
    public override long Size {
        get {
            try {
                if (IsValid) return new FileInfo(filePath).Length;
            }
            catch {
                // ignored
            }

            return 0;
        }
    }

    public override bool IsValid => File.Exists(filePath);

    public override int GetFileContent(in byte[] buffer, int offset) {
        var fileSize = Size;
        if (buffer.Length + offset < fileSize)
            throw new ArgumentException("Buffer does not have enough space for file contents", nameof(buffer));
        try {
            if (IsValid) {
                using var file = File.OpenRead(filePath);
                return file.Read(buffer, offset, (int) fileSize);
            }
        }
        catch {
            // ignored
        }

        throw new InvalidDVfsFileException();
    }

    public override Stream GetFileStream() {
        try {
            if (IsValid) return File.OpenRead(filePath);
        }
        catch {
            // ignored
        }

        throw new InvalidDVfsFileException();
    }

    /// <summary>
    /// Get a handle to the file via a MemoryMappedFile
    /// <para/>
    ///
    /// </summary>
    /// <returns>A Memory Mapped File for accessing the file</returns>
    /// <exception cref="InvalidDVfsFileException">Thrown if the file is not valid</exception>
    public MemoryMappedFile GetMemoryMappedFile() {
        try {
            if (IsValid) return MemoryMappedFile.CreateFromFile(filePath);
        }
        catch {
            // ignored
        }

        throw new InvalidDVfsFileException();
    }
}