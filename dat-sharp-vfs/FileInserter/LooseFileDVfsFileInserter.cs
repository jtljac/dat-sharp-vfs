using dat_sharp_vfs.VfsFile;

namespace dat_sharp_vfs.FileInserter;

/// <summary>
/// An implementation of IVfsFileInserter that inserts every file in a directory
/// </summary>
/// <param name="dirPath">The path to the folder containing the loose files to add</param>
public class LooseFileDVfsFileInserter(string dirPath, bool recursive) : IVfsFileInserter{
    private IEnumerable<Tuple<string, DVfsFile>> GetFiles(string path) {
        // Bare with, this one's cheeky
        // Iterate through all the folders in this file
        foreach (var file in Directory.GetFiles(path)) {
            yield return new Tuple<string, DVfsFile>(Path.GetRelativePath(dirPath, file), new LooseDVfsFile(file));
        }

        if (!recursive) yield break;

        // Iterate through all the directories, then yield on each yield from recursion
        foreach (var directory in Directory.GetDirectories(dirPath)) {
            foreach (var tuple in GetFiles(directory)) {
                yield return tuple;
            }
        }
    }

    public IEnumerable<Tuple<string, DVfsFile>> GetFiles() {
        return GetFiles(dirPath);
    }

    public void HandleInsertFailure(string path, DVfsFile file, InsertionFailureReason failureReason) {
        // We don't care to retry for any reason
        // DVfsFile should just get picked up by garbage collector
    }
}