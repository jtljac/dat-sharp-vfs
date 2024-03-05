/*
 * Implementation notes:
 * Every operation with a path has a public method where the path is a string, which calls a protected method, where the
 * Path is a Span<string>. The public method is for ease of use. The protected method breaks the path up by it's path
 * separators ('/') using a span. For each level the method recurses, it passes a slice of the span that uses the same
 * memory buffer, but starting from the 2nd (index 1) element. This prevents repeated allocating and parsing of strings
 * were we to only use a string argument, and the constant copying of memory if we were to use an Array or List
 */

using dat_sharp_vfs.Util;

namespace dat_sharp_vfs;

/// <summary>
/// A class representing a folder in a Virtual File System.
/// </summary>
public class DatSharpVfs {
    private static readonly string[] SpecialDirs = ["", ".", ".."];

    /// <summary>The directories in this directory</summary>
    private readonly Dictionary<string, DatSharpVfs> _directories = new();
    /// <summary>The files in this directory</summary>
    private readonly Dictionary<string, DVfsFile> _files = new();

    /// <summary>The parent of this directory</summary>
    private readonly DatSharpVfs _parent;

    public DatSharpVfs() {
        // Empty string for when a path contains consecutive '/'
        _directories[""] = this;
        _directories["."] = this;
        _directories[".."] = _parent = this;
    }

    /// <param name="parent">The parent of this directory</param>
    private DatSharpVfs(DatSharpVfs parent) {
        // Empty string for when a path contains consecutive '/'
        _directories[""] = this;
        _directories["."] = this;
        _directories[".."] = _parent = parent;
    }

    /* --------------------------------------- */
    /* File/Directory Modify                   */
    /* --------------------------------------- */

    /// <summary>
    /// Create a new directory in the Vfs
    /// <para/>
    /// If a directory already exists at the given path then it will be returned and no new directory will be created
    /// </summary>
    /// <param name="path">The path of the new directory</param>
    /// <param name="recursive">If true, recursively create directories in the path that don't exist</param>
    /// <returns>
    ///     The newly created directory, or the existing directory if one already exists at the path
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the path is empty</exception>
    /// <exception cref="DirectoryNotFoundException">
    ///     Thrown when recursive is false and the path contains a directory that doesn't exist
    /// </exception>
    public DatSharpVfs CreateDirectory(string path, bool recursive = false) {
        // Trim end to avoid folder with empty name
        return CreateDirectory(StringToPath(path.TrimEnd('/')), recursive);
    }

    /// <inheritdoc cref="CreateDirectory"/>
    private DatSharpVfs CreateDirectory(Span<string> path, bool recursive = false) {
        if (path.IsEmpty) throw new ArgumentException("Path cannot be empty", nameof(path));
        if (path.Length == 1) {
            // If already exists then return the existing directory.
            // This mimics the behaviour of C#'s Directory#CreateDirectory
            if (_directories.TryGetValue(path[0], out var existingDir)) return existingDir;
            return _directories[path[0]] = new DatSharpVfs(this);
        }

        if (_directories.TryGetValue(path[0], out var nextDir)) return nextDir.CreateDirectory(path[1..], recursive);

        if (!recursive) throw new DirectoryNotFoundException("Failed to find Directory");
        return (_directories[path[0]] = new DatSharpVfs(this))
            .CreateDirectory(path[1..], recursive);
    }

    /// <summary>
    /// Mount a file in the Vfs
    /// </summary>
    /// <param name="path">The path to mount the file to</param>
    /// <param name="file">The DVfsFile to mount</param>
    /// <param name="overwrite">If true then if there is already a file at the path, overwrite it</param>
    /// <param name="createFolders">If true, recursively create directories in the path that don't exist</param>
    /// <exception cref="ArgumentException">Thrown when the filename or path is empty</exception>
    /// <exception cref="FileExistsException">
    /// Thrown if there is already a file mounted at the path and <paramref name="overwrite"/> is false
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when createFolders is false and the path contains a directory that doesn't exist
    /// </exception>
    public void MountFile(string path, DVfsFile file, bool overwrite = true, bool createFolders = false) {
        if (path.EndsWith('/')) throw new ArgumentException("Filename cannot be empty", nameof(path));
        MountFile(StringToPath(path), file, createFolders);
    }

    /// <inheritdoc cref="MountFile"/>
    private void MountFile(Span<string> path, DVfsFile file, bool overwrite = true, bool createFolders = false) {
        if (path.IsEmpty) throw new ArgumentException("Path cannot be empty", nameof(path));
        
        if (path.Length == 1) {
            if (_files.ContainsKey(path[0])) {
                if (overwrite) UnmountFile(path[0]);
                else throw new FileExistsException();
            }

            _files[path[0]] = file;
            file.IncrementReferences();
            return;
        }

        if (_directories.TryGetValue(path[0], out var nextDir)) {
            nextDir.MountFile(path[1..], file, createFolders);
            return;
        }

        if (!createFolders) throw new DirectoryNotFoundException("Failed to find Directory");
        (_directories[path[0]] = new DatSharpVfs(this))
            .MountFile(path[1..], file, createFolders);
    }

    /// <summary>
    /// Bulk mount files using an inserter
    /// </summary>
    /// <param name="basePath">A path to the base directory to start mounting the files at</param>
    /// <param name="inserter">An inserter that defines which files to insert</param>
    /// <param name="overwrite">If true then if a file's path points an existing file, it will overwrite it</param>
    /// <param name="createFolders">
    /// If true, recursively create directories in the path that don't exist. Applies to both the base path and paths
    /// from the inserter
    /// </param>
    /// <returns>The number of files that successfully mounted</returns>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the basePath contains a directory that doesn't exist
    /// </exception>
    public int MountFiles(string basePath, IVfsFileInserter inserter, bool overwrite = false, bool createFolders = true) {
        return MountFiles(StringToPath(basePath), inserter, overwrite, createFolders);
    }

    /// <inheritdoc cref="MountFiles"/>
    private int MountFiles(Span<string> basePath, IVfsFileInserter inserter, bool overwrite = false, bool createFolders = true) {
        if (!basePath.IsEmpty) {
            if (_directories.TryGetValue(basePath[0], out var nextDir)) return nextDir.MountFiles(basePath[1..], inserter, createFolders);

            if (!createFolders) throw new DirectoryNotFoundException("Failed to find Directory");
            _directories[basePath[0]] = new DatSharpVfs(this);
            return _directories[basePath[0]].MountFiles(basePath[1..], inserter, createFolders);
        }

        var counter = 0;

        foreach (var (path, file) in inserter.GetFiles()) {
            try {
                MountFile(StringToPath(path), file, overwrite, createFolders);
                ++counter;
            }
            catch (ArgumentException) {
                inserter.HandleInsertFailure(path, file, InsertionFailureReason.BadPath);
            }
            catch (FileExistsException) {
                inserter.HandleInsertFailure(path, file, InsertionFailureReason.FileExists);
            }
            catch (DirectoryNotFoundException) {
                inserter.HandleInsertFailure(path, file, InsertionFailureReason.CouldNotFindDirectory);
            }
        }

        return counter;
    }

    /// <summary>
    /// Unmount a file from the Vfs
    /// </summary>
    /// <param name="path">The path to the file to unmount</param>
    /// <returns>The file that was unmounted, or null if there is no file at the path</returns>
    public DVfsFile? UnmountFile(string path) {
        return UnmountFile(StringToPath(path));
    }

    /// <inheritdoc cref="UnmountFile"/>
    private DVfsFile? UnmountFile(Span<string> path) {
        switch (path.Length) {
            case 0:
                return null;
            case 1:
                if (!_files.Remove(path[0], out var file)) return null;

                file.DecrementReferences();
                return file;

            default:
                _directories.TryGetValue(path[0], out var directory);
                return directory?.UnmountFile(path[1..]);
        }
    }

    /// <summary>
    /// Remove a directory from the Vfs
    /// <para/>
    /// A directory is not aware of its own name, therefore it cannot remove itself from it's parent.
    /// </summary>
    /// <param name="path">The path to the directory to remove</param>
    /// <param name="recursive">If true, recursively remove files and subdirectories</param>
    /// <exception cref="ArgumentException">Thrown when path is empty, or the final directory is the path is '.' or ".."</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if path refers to a directory that doesn't exist</exception>
    /// <exception cref="DirectoryNotEmptyException">
    /// Thrown when recursive is false and the directory to remove isn't empty
    /// </exception>
    public void RemoveDirectory(string path, bool recursive = false) {
        // Trim end to avoid folder with empty name
        RemoveDirectory(StringToPath(path.TrimEnd('/')), recursive);
    }

    /// <inheritdoc cref="RemoveDirectory"/>
    private void RemoveDirectory(Span<string> path, bool recursive = false) {
        switch (path.Length) {
            case 0:
                throw new ArgumentException("Path cannot be empty", nameof(path));
            case 1:
                if (SpecialDirs.Contains(path[0])) {
                    throw new ArgumentException("A folder cannot remove itself or its parent. You must remove a folder from its parent", nameof(path));
                }

                if (!_directories.TryGetValue(path[0], out var dir)) throw new DirectoryNotFoundException("Failed to find directory");

                if (!dir.IsEmpty) {
                    if (!recursive) throw new DirectoryNotEmptyException("That directory is not empty");
                    dir.CleanDirectory();
                }

                _directories.Remove(path[0]);
                break;

            default:
                if (!_directories.TryGetValue(path[0], out var directory))
                    throw new DirectoryNotFoundException("Failed to find directory");;
                directory?.RemoveDirectory(path[1..], recursive);
                break;
        }
    }

    /// <summary>
    /// Unmount all files and remove all directories recursively
    /// </summary>
    public void CleanDirectory() {
        foreach (var fileName in _files.Keys.ToList()) {
            UnmountFile(fileName);
        }

        foreach (var (dirName, dir) in _directories.ToList().Where((pair) => !SpecialDirs.Contains(pair.Key))) {
            dir.CleanDirectory();
            _directories.Remove(dirName);
        }
    }

    /* --------------------------------------- */
    /* File/Directory Access                   */
    /* --------------------------------------- */

    /// <summary>
    /// Get a directory from the Vfs
    /// </summary>
    /// <param name="path">The path to the directory in the Vfs</param>
    /// <returns>The directory at the path, or null if one doesn't exist</returns>
    public DatSharpVfs? GetDirectory(string path) {
        return GetDirectory(StringToPath(path));
    }

    /// <inheritdoc cref="GetDirectory"/>
    private DatSharpVfs? GetDirectory(Span<string> path) {
        if (path.Length == 0) return this;

        _directories.TryGetValue(path[0], out var directory);
        return directory?.GetDirectory(path[1..]);
    }

    /// <summary>
    /// Get a file from the Vfs
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <returns>
    /// The file at the path, or null if the file doesn't exist or any directories in the path don't exist
    /// </returns>
    public DVfsFile? GetFile(string path) {
        return GetFile(StringToPath(path));
    }

    /// <inheritdoc cref="GetFile"/>
    private DVfsFile? GetFile(Span<string> path) {
        switch (path.Length) {
            case 0:
                return null;
            case 1:
                _files.TryGetValue(path[0], out var file);
                return file;
            default:
                _directories.TryGetValue(path[0], out var directory);
                return directory?.GetFile(path[1..]);
        }
    }

    /* --------------------------------------- */
    /* File Util                               */
    /* --------------------------------------- */

    /// <summary>
    /// Get if this directory is the root of the directory
    /// </summary>
    public bool IsRoot => _parent == this;

    /// <summary>
    /// Get how deep relative to the root this directory is
    /// </summary>
    public int Depth => IsRoot ? 0 : 1 + _parent.Depth;

    /// <summary>
    /// Get if this directory is empty, I.E. contains no files or folders
    /// </summary>
    public bool IsEmpty => _directories.Count <= 3 && _files.Count == 0;

    /// <summary>
    /// Get if a file exists
    /// </summary>
    /// <param name="path">The path to the file</param>
    /// <returns>True if there is a file at the given path</returns>
    public bool FileExists(string path) {
        return FileExists(StringToPath(path));
    }

    /// <inheritdoc cref="FileExists"/>
    private bool FileExists(Span<string> path) {
        switch (path.Length) {
            case 0:
                return false;
            case 1:
                return _files.ContainsKey(path[0]);
            default:
                _directories.TryGetValue(path[0], out var directory);
                return directory?.FileExists(path[1..]) ?? false;
        }
    }

    /// <summary>
    /// Get if a directory exists
    /// </summary>
    /// <param name="path">The path to the directory</param>
    /// <returns>True if there is a directory at the given path</returns>
    public bool DirectoryExists(string path) {
        return DirectoryExists(StringToPath(path));
    }

    /// <inheritdoc cref="DirectoryExists"/>
    private bool DirectoryExists(Span<string> path) {
        if (path.Length == 0) return true;

        _directories.TryGetValue(path[0], out var directory);
        return directory?.DirectoryExists(path[1..]) ?? false;
    }

    /// <summary>
    /// List the files in the directory at the given path
    /// </summary>
    /// <param name="path">
    /// The path to the directory to list the files of
    /// <para/>
    /// You can use "." or "" (an empty string) to refer to the current directory
    /// </param>
    /// <returns>An array of filenames</returns>
    public string[] ListFiles(string path) {
        return ListFiles(StringToPath(path));
    }

    /// <inheritdoc cref="ListFiles"/>
    private string[] ListFiles(Span<string> path) {
        if (path.IsEmpty) return _files.Keys.ToArray();

        _directories.TryGetValue(path[0], out var directory);
        return directory?.ListFiles(path[1..]) ?? Array.Empty<string>();
    }


    /// <summary>
    /// List the directories in the directory at the given path
    /// </summary>
    /// <param name="path">
    /// The path to the directory to list the directories of
    /// <para/>
    /// You can use "." or "" (an empty string) to refer to the current directory
    /// </param>
    /// <param name="includeAll">Whether to include hidden files (Files that start with '.'</param>
    /// <returns>An array of directory names</returns>
    public string[] ListDirectories(string path, bool includeAll = false) {
        return ListDirectories(StringToPath(path), includeAll);
    }

    /// <inheritdoc cref="ListDirectories"/>
    private string[] ListDirectories(Span<string> path, bool includeAll = false) {
        if (path.IsEmpty) return _directories.Keys
            .Where(key => key.Length != 0 && (includeAll || !key.StartsWith('.'))).ToArray();

        _directories.TryGetValue(path[0], out var directory);
        return directory?.ListDirectories(path[1..]) ?? Array.Empty<string>();
    }

    /* --------------------------------------- */
    /* Util                                    */
    /* --------------------------------------- */

    /// <summary>
    /// Convert a string path into a span, where each element is a directory name (the final element may be a filename
    /// or a directory name). The path is split by '/' (Forward slash).
    /// <para/>
    /// Paths beginning with a '/' (forward slash) will be modified to reroute to the root of the vfs
    /// </summary>
    /// <param name="path">The path to convert</param>
    /// <returns>A span representing the path</returns>
    private Span<string> StringToPath(string path) {
        if (path.Length == 0) return new Span<string>();
        // Handle special characters
        // If starts with /, modify string to navigate to root
        if (path[0] == '/') path = string.Concat(Enumerable.Repeat("../", Depth)) + path[1..];
        return path.Split("/").AsSpan();
    }
}