namespace dat_sharp_vfs;

/// <summary>
/// An interface for mass inserting files into a DVfs folder
/// </summary>
public interface IVfsFileInserter {
    /// <summary>
    /// Get an Enumerable that contains the files being inserted.
    /// <para/>
    /// Each element in the Enumerable should be a Tuple containing the desired path of the file in the Vfs in the
    /// first element, and the DVfsFile in the second element
    /// </summary>
    /// <returns>An Enumerable containing all the files to insert</returns>
    IEnumerable<Tuple<string, DVfsFile>> GetFiles();

    /// <summary>
    /// Handle when a file has failed to insert
    /// <para/>
    /// The intention of this method is to allow re-queuing or cleanup
    /// </summary>
    /// <param name="path">The path of the file</param>
    /// <param name="file">The file</param>
    /// <param name="failureReason">The reason the file wasn't inserted</param>
    void HandleInsertFailure(string path, DVfsFile file, InsertionFailureReason failureReason);
}

/// <summary>
/// The types of failure that can be thrown when trying to insert a file into the VFS
/// </summary>
public enum InsertionFailureReason {
    /// <summary>A file already exists with the given path</summary>
    FileExists,
    /// <summary>The provided path doesn't exist and creating directories is disabled</summary>
    CouldNotFindDirectory,
    /// <summary>The path provided is invalid</summary>
    BadPath
}