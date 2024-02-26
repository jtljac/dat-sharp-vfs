using System.Collections.ObjectModel;
using System.IO.MemoryMappedFiles;

namespace dat_sharp_vfs.VfsFile;

/// <summary>
/// An implementation of DVfsFile that represents some data stored in memory
/// </summary>
/// <param name="data">The path to the real file on the disk</param>
public class MemoryDVfsFile(byte[] data) : DVfsFile {
    public override long Size => data.Length;
    public override bool IsValid => true;

    public override int GetFileContent(in byte[] buffer, int offset) {
        var fileSize = Size;
        if (buffer.Length - offset < fileSize)
            throw new ArgumentException("Buffer does not have enough space for file contents", nameof(buffer));

        data.CopyTo(buffer, offset);
        return (int) fileSize;
    }

    public override Stream GetFileStream() {
        return new MemoryStream(data, false);
    }
}