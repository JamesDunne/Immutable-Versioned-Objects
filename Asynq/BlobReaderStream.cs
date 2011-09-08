using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;

namespace Asynq
{
    /// <summary>
    /// Represents a read-only stream that reads data directly from an open SqlDataReader
    /// for a single column. The SqlCommand that was executed to obtain the SqlDataReader
    /// must be executed with the `CommandBehavior.SequentialAccess` option for best results,
    /// otherwise the entire row will be read in to memory at once thus defeating the
    /// purpose of streaming content directly from the database that may be larger than
    /// available memory.
    /// </summary>
    public sealed class BlobReaderStream : Stream
    {
        private SqlDataReader dr;
        private int ordinal;
        private long? length;
        private long position;

        /// <summary>
        /// Creates a stream that reads over the given SqlDataReader with the current column ordinal.
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="ordinal"></param>
        /// <param name="length">Optional parameter used to supply the Length property with.
        /// If null (the default), the Length property will throw an exception if it is accessed.</param>
        public BlobReaderStream(SqlDataReader dr, int ordinal, long? length = null)
        {
            this.dr = dr;
            this.ordinal = ordinal;
            this.length = length;

            this.position = 0;
        }

        /// <summary>
        /// Gets a value that determines whether or not this stream can be read. Always returns true.
        /// </summary>
        public override bool CanRead { get { return true; } }
        /// <summary>
        /// Gets a value that determines whether or not this stream supports seeking. Always returns false.
        /// </summary>
        public override bool CanSeek { get { return false; } }
        /// <summary>
        /// Gets a value that determines whether or not this stream can be written. Always returns false.
        /// </summary>
        public override bool CanWrite { get { return false; } }

        /// <summary>
        /// Reads the next chunk of data into <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to read data to.</param>
        /// <param name="offset">Where in the buffer to read the data to.</param>
        /// <param name="count">How much data to read.</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // Read the next chunk of data into the given buffer:
            long numRead = this.dr.GetBytes(this.ordinal, dataIndex: this.position, buffer: buffer, bufferIndex: offset, length: count);
            // Advance our tracked position:
            this.position += numRead;

            // Return how many bytes have been read (why this is a `long` escapes me, because the `length` parameter is an `int`):
            return (int)numRead;
        }

        /// <summary>
        /// Gets the blob's length if provided otherwise throws an exception.
        /// </summary>
        public override long Length
        {
            get { if (!length.HasValue) throw new NotSupportedException(); return length.Value; }
        }

        /// <summary>
        /// Gets the current position in the data stream.
        /// </summary>
        public override long Position
        {
            get { return position; }
            set { throw new NotSupportedException(); }
        }

        #region Not supported

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
