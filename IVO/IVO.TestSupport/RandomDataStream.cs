using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace IVO.TestSupport
{
    public sealed class RandomDataStream : Stream
    {
        private long length;
        private RandomNumberGenerator rng;
        private long position;

        public RandomDataStream(long length)
        {
            this.length = length;
            this.rng = RandomNumberGenerator.Create();
            this.position = 0;
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return this.length; }
        }

        public override long Position
        {
            get
            {
                return this.position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (position + count >= length) count = (int)(length - position);
            if (count <= 0) return 0;

            byte[] tmp = new byte[count];
            rng.GetBytes(tmp);
            
            Array.Copy(tmp, 0, buffer, offset, count);
            tmp = null;

            position += count;

            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
