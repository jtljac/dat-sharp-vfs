namespace dat_sharp_vfs.Util;

/// <summary>
/// An error for operations that require an empty directory are performed on a directory that isn't empty
/// </summary>
public class DirectoryNotEmptyException : Exception {
    public DirectoryNotEmptyException() { }
    public DirectoryNotEmptyException(string? message) : base(message) { }
    public DirectoryNotEmptyException(string? message, Exception? innerException) : base(message, innerException) { }
}