using System.IO.MemoryMappedFiles;
using dat_sharp_vfs.Util;

namespace dat_sharp_vfs;

/// <summary>
/// An abstract class representing a file in the VFS
/// </summary>
public abstract class DVfsFile {
    /// <summary>The number of references to this file in the VFS</summary>
    /// <remarks>This is used to enable multiple links to a file in the VFS</remarks>
    public byte references { get; private set; }

    /// <summary>
    /// Increment the number of references to this file
    /// <para/>
    /// This should be used whenever a the file is added to the VFS
    /// </summary>
    /// <returns>The new number of references</returns>
    internal byte IncrementReferences() {
        return ++references;
    }

    /// <summary>
    /// Decrement the number of references to this file
    /// <para/>
    /// This should be used whenever a the file is removed from the VFS
    /// </summary>
    /// <returns>The new number of references</returns>
    internal byte DecrementReferences() {
        return --references;
    }

    /// <summary>
    ///  The size of the file.
    /// <para/>
    /// If the file is invalid then this will return 0 (and not throw an error)
    /// <para/>
    /// Note, this is the full uncompressed size
    /// </summary>
    public abstract long Size { get; }

    /// <summary>
    /// Check the file is valid and accessible
    /// </summary>
    public abstract bool IsValid { get; }

    /// <summary>
    /// Get the contents of the file as a byte array
    /// </summary>
    /// <param name="buffer">The byte array to populate with the file</param>
    /// <param name="offset">The offset of the byte buffer from which to start writing to</param>
    /// <returns>The number of bytes written to the buffer</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the <paramref name="buffer"/> length + <paramref name="offset"/> is less than the <see cref="Size"/>
    /// </exception>
    /// <exception cref="InvalidDVfsFileException">Thrown if the file is not valid</exception>
    public abstract int GetFileContent(in byte[] buffer, int offset);

    /// <summary>
    /// Get a handle to the file via a stream
    /// </summary>
    /// <returns>A stream for accessing the file</returns>
    /// <exception cref="InvalidDVfsFileException">Thrown if the file is not valid</exception>
    public abstract Stream GetFileStream();
}