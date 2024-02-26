namespace dat_sharp_vfs.Util;

/// <summary>
/// Exception for when attempting to mount a file to a location that already has a file mounted
/// </summary>
public class FileExistsException : Exception {
    public FileExistsException() { }
    public FileExistsException(string? message) : base(message) { }
    public FileExistsException(string? message, Exception? innerException) : base(message, innerException) { }
}