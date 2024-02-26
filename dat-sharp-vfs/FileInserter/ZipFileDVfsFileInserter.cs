using System.IO.Compression;
using dat_sharp_vfs.VfsFile;

namespace dat_sharp_vfs.FileInserter;

/// <summary>
/// An implementation of IVfsFileInserter that inserts every file in a zip file
/// </summary>
public class ZipFileDVfsFileInserter : IVfsFileInserter {
    private ZipArchive _zipArchive;

    /// <param name="zipFilePath">The path to the zip file to add the entries of</param>
    public ZipFileDVfsFileInserter(string zipFilePath) {
        _zipArchive = ZipFile.OpenRead(zipFilePath);
    }

    /// <param name="zipArchive">The archive to add the entries of</param>
    public ZipFileDVfsFileInserter(ZipArchive zipArchive) {
        _zipArchive = zipArchive;
    }

    public IEnumerable<Tuple<string, DVfsFile>> GetFiles() {
        return _zipArchive.Entries.Select(entry => new Tuple<string, DVfsFile>(entry.Name, new ZipDVfsFile(entry)));
    }

    public void HandleInsertFailure(string path, DVfsFile file, InsertionFailureReason failureReason) {
        // We don't care to retry for any reason
        // DVfsFile should just get picked up by garbage collector
    }
}