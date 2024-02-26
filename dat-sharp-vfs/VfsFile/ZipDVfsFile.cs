using System.IO.Compression;
using dat_sharp_vfs.Util;

namespace dat_sharp_vfs.VfsFile;

/// <summary>
/// A DVfsFile implementation that represents a file in a Zip Archive
/// </summary>
public class ZipDVfsFile(ZipArchiveEntry entry) : DVfsFile {
    public override long Size => entry.Length;
    public override bool IsValid {
        // Try opening, I really can't find anything else
        get {
            try {
                using var stream = entry.Open();
                return true;
            }
            catch {
                return false;
            }
        }
    }

    public override int GetFileContent(in byte[] buffer, int offset) {
        if (buffer.Length - offset < Size)
            throw new ArgumentException("Buffer does not have enough space for file contents", nameof(buffer));

        using var stream = GetFileStream();
        return stream.Read(buffer, offset, (int) Size);

    }

    public override Stream GetFileStream() {
        try {
            return entry.Open();
        }
        catch (Exception e) {
            throw new InvalidDVfsFileException("Failed to open Zip File entry", e);
        }
    }
}