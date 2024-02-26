namespace dat_sharp_vfs.Util;

/// <summary>
/// An exception for when an operation is performed using an invalid DVfsFile
/// </summary>
public class InvalidDVfsFileException : Exception {
    public InvalidDVfsFileException() { }
    public InvalidDVfsFileException(string? message) : base(message) { }
    public InvalidDVfsFileException(string? message, Exception? innerException) : base(message, innerException) { }
}