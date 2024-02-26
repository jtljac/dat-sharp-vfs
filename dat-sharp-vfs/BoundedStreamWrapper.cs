namespace dat_sharp_vfs;

/// <summary>
/// A wrapper for streams that sets bounds on how much of the file can be accessed
/// </summary>
public class BoundedStreamWrapper : Stream {
    /// <summary>The amount of bytes of the wrapped stream to make available</summary>
    private readonly int _length;

    /// <summary>The lowerBound of the original stream in bytes (inclusive)</summary>
    private readonly int _lowerBound;

    /// <summary>The upper bound of the wrapped stream in bytes (Exclusive)</summary>
    private readonly int _upperBound;

    /// <summary>The original stream being wrapped</summary>
    private readonly Stream _wrappedStream;

    public BoundedStreamWrapper(Stream wrappedStream, int lowerBound, int length) {
        _wrappedStream = wrappedStream;
        _lowerBound = lowerBound;
        _length = length;
        _upperBound = lowerBound + length;

        _wrappedStream.Position = lowerBound;
    }

    /// <summary>
    /// Disabled
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown always</exception>
    public override void Flush() {
        throw new NotSupportedException("You bounded streams are read-only");
    }

    /// <summary>
    /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
    /// </summary>/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
    /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
    /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
    /// <returns>The number of bytes read.</returns>
    public override int Read(byte[] buffer, int offset, int count) {
        if (Position + count >= _length) {
            count = (int) (_length - Position);
        }

        return _wrappedStream.Read(buffer, offset, count);
    }

    /// <inheritdoc cref="Stream.Seek"/>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when trying to seek outside of the bounds of the file</exception>
    public override long Seek(long offset, SeekOrigin origin) {
        var newOffset = origin switch {
            SeekOrigin.Begin => offset + _lowerBound,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => offset + _upperBound,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };

        if (_lowerBound > newOffset || newOffset > _upperBound) {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "You cannot seek outside the bounds");
        }

        return _wrappedStream.Seek(newOffset, SeekOrigin.Begin) - _lowerBound;
    }

    /// <summary>
    /// Disabled
    /// </summary>
    /// <exception cref="NotSupportedException">Always</exception>
    public override void SetLength(long value) {
        throw new NotSupportedException("You bounded streams are read-only");
    }

    /// <summary>
    /// Disabled
    /// </summary>
    /// <exception cref="NotSupportedException">Always</exception>
    public override void Write(byte[] buffer, int offset, int count) {
        throw new NotSupportedException("You bounded streams are read-only");
    }

    public override bool CanRead => _wrappedStream.CanRead;

    public override bool CanSeek => _wrappedStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => _length;

    public override long Position {
        get => _wrappedStream.Position - _lowerBound;
        set {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), value, "Position must not be negative");
            if (value > _upperBound) throw new EndOfStreamException("Attempted to seek passed upper bound");
            _wrappedStream.Position = value + _lowerBound;
        }
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);

        // Dispose of wrapped stream
        _wrappedStream.Dispose();
    }


}