using System;
using System.IO;

namespace Xevle.IO.Streams
{
	/// <summary>
	/// Buffered binary reader.
	/// </summary>
	public class BufferedBinaryReader : IDisposable
	{
		Stream m_stream;
		byte[] m_buffer;
		int posInBuffer;
		int capInBuffer;
		byte[] i_buffer;
		int i_bufferSize;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> class.
		/// </summary>
		/// <param name="filename">Filename.</param>
		public BufferedBinaryReader(string filename) : this(new FileStream(filename, FileMode.Open, FileAccess.Read), 0x10000)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> class.
		/// </summary>
		/// <param name="input">Input.</param>
		public BufferedBinaryReader(Stream input) : this(input, 0x10000)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> class.
		/// </summary>
		/// <param name="input">Input.</param>
		/// <param name="bufferSize">Buffer size.</param>
		public BufferedBinaryReader(Stream input, int bufferSize)
		{
			if (input == null) throw new ArgumentNullException("input");
			if (!input.CanRead) throw new ArgumentException("Stream not readable.");
			if (bufferSize <= 0) throw new ArgumentOutOfRangeException("bufferSize", "Must be greater than zero.");

			i_buffer = new byte[8];

			m_stream = input;

			i_bufferSize = bufferSize;
			FillBuffer();
		}

		/// <summary>
		/// Gets the length of the stream.
		/// </summary>
		/// <value>The length of the stream.</value>
		public long StreamLength
		{
			get { return m_stream.Length; }
		}

		/// <summary>
		/// Gets the stream position.
		/// </summary>
		/// <value>The stream position.</value>
		public long StreamPosition
		{
			get { return m_stream.Position - (capInBuffer - posInBuffer); }
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public virtual void Close()
		{
			Dispose(true);
		}

		/// <summary>
		/// Dispose the specified disposing.
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && m_stream != null) m_stream.Close();
			m_stream = null;
			m_buffer = null;
			capInBuffer = posInBuffer = 0;
		}

		/// <summary>
		/// Fills the buffer.
		/// </summary>
		protected virtual void FillBuffer()
		{
			if (m_buffer == null || m_buffer.Length < i_bufferSize) m_buffer = new byte[i_bufferSize];

			posInBuffer = 0;
			capInBuffer = m_stream.Read(m_buffer, 0, i_bufferSize);
		}

		/// <summary>
		/// Read the specified buffer, index and count.
		/// </summary>
		/// <param name="buffer">Buffer.</param>
		/// <param name="index">Index.</param>
		/// <param name="count">Count.</param>
		public virtual int Read(byte[] buffer, int index, int count)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			if (index < 0) throw new ArgumentOutOfRangeException("index");
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if ((buffer.Length - index) < count) throw new ArgumentException();
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");

			int posInTarget = 0;
			do
			{
				if (capInBuffer <= posInBuffer) break;

				int len = Math.Min(capInBuffer - posInBuffer, count);
				Buffer.BlockCopy(m_buffer, posInBuffer, buffer, posInTarget, len);
				posInBuffer += len;
				posInTarget += len;
				count -= len;
				if (posInBuffer == capInBuffer) FillBuffer();
			}
			while(count > 0);

			return posInTarget;
		}

		/// <summary>
		/// Reads the boolean.
		/// </summary>
		/// <returns><c>true</c>, if boolean was  read, <c>false</c> otherwise.</returns>
		public virtual bool ReadBoolean()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			byte val = m_buffer[posInBuffer];
			posInBuffer++;
			if (posInBuffer == capInBuffer) FillBuffer();

			return val != 0;
		}

		/// <summary>
		/// Reads the byte.
		/// </summary>
		/// <returns>The byte.</returns>
		public virtual byte ReadByte()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			byte val = m_buffer[posInBuffer];
			posInBuffer++;
			if (posInBuffer == capInBuffer) FillBuffer();

			return val;
		}

		/// <summary>
		/// Reads the bytes.
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="count">Count.</param>
		public virtual byte[] ReadBytes(int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException("count");
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			byte[] buffer = new byte[count];

			int offset = 0;
			do
			{
				int num2 = Read(buffer, offset, count);
				if (num2 == 0) break;
				offset += num2;
				count -= num2;
			}
			while(count > 0);

			if (offset != buffer.Length)
			{
				byte[] dst = new byte[offset];
				Buffer.BlockCopy(buffer, 0, dst, 0, offset);
				buffer = dst;
			}
			return buffer;
		}

		/// <summary>
		/// Reads the double.
		/// </summary>
		/// <returns>The double.</returns>
		public virtual unsafe double ReadDouble()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 8;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			fixed(byte* ret=i_buffer) return *(double*)ret;
		}

		/// <summary>
		/// Reads the int16.
		/// </summary>
		/// <returns>The int16.</returns>
		public virtual unsafe short ReadInt16()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 2;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			//return (short)(i_buffer[0]|(i_buffer[1]<<8));
			fixed(byte* ret=i_buffer) return *(short*)ret;
		}

		/// <summary>
		/// Reads the int32.
		/// </summary>
		/// <returns>The int32.</returns>
		public virtual unsafe int ReadInt32()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 4;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			//return (((i_buffer[0]|(i_buffer[1]<<8))|(i_buffer[2]<<0x10))|(i_buffer[3]<<0x18));
			fixed(byte* ret=i_buffer) return *(int*)ret;
		}

		/// <summary>
		/// Reads the int24.
		/// </summary>
		/// <returns>The int24.</returns>
		public virtual int ReadInt24()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 3;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			return (int)((i_buffer[0] | (i_buffer[1] << 8)) | (i_buffer[2] << 0x10));
		}

		/// <summary>
		/// Reads the int64.
		/// </summary>
		/// <returns>The int64.</returns>
		public virtual unsafe long ReadInt64()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 8;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			fixed(byte* ret=i_buffer) return *(long*)ret;
		}

		/// <summary>
		/// Reads the S byte.
		/// </summary>
		/// <returns>The S byte.</returns>
		public virtual sbyte ReadSByte()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			byte val = m_buffer[posInBuffer];
			posInBuffer++;
			if (posInBuffer == capInBuffer) FillBuffer();

			return (sbyte)val;
		}

		/// <summary>
		/// Reads the single.
		/// </summary>
		/// <returns>The single.</returns>
		public virtual unsafe float ReadSingle()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 4;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			fixed(byte* ret=i_buffer) return *(float*)ret;
		}

		/// <summary>
		/// Reads the U int16.
		/// </summary>
		/// <returns>The U int16.</returns>
		public virtual unsafe ushort ReadUInt16()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 2;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			fixed(byte* ret=i_buffer) return *(ushort*)ret;
		}

		/// <summary>
		/// Reads the Uint24.
		/// </summary>
		/// <returns>The U int24.</returns>
		public virtual uint ReadUInt24()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 3;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			return (uint)((i_buffer[0] | (i_buffer[1] << 8)) | (i_buffer[2] << 0x10));
		}

		/// <summary>
		/// Reads the U int32.
		/// </summary>
		/// <returns>The U int32.</returns>
		public virtual unsafe uint ReadUInt32()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 4;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			//return (uint)(((i_buffer[0]|(i_buffer[1]<<8))|(i_buffer[2]<<0x10))|(i_buffer[3]<<0x18));
			fixed(byte* ret=i_buffer) return *(uint*)ret;
		}

		/// <summary>
		/// Reads the U int64.
		/// </summary>
		/// <returns>The U int64.</returns>
		public virtual unsafe ulong ReadUInt64()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			int fillsize = 8;

			int len = Math.Min(capInBuffer - posInBuffer, fillsize);
			Buffer.BlockCopy(m_buffer, posInBuffer, i_buffer, 0, len);
			posInBuffer += len;

			if (posInBuffer == capInBuffer) FillBuffer();
			if (len < fillsize) Buffer.BlockCopy(m_buffer, 0, i_buffer, len, posInBuffer = fillsize - len);

			//uint num=(uint)(((i_buffer[0]|(i_buffer[1]<<8))|(i_buffer[2]<<0x10))|(i_buffer[3]<<0x18));
			//uint num2=(uint)(((i_buffer[4]|(i_buffer[5]<<8))|(i_buffer[6]<<0x10))|(i_buffer[7]<<0x18));
			//return (((ulong)num2<<0x20)|num);
			fixed(byte* ret=i_buffer) return *(ulong*)ret;
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Xevle.IO.Streams.BufferedBinaryReader"/>.
		/// The <see cref="Dispose"/> method leaves the <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> in an unusable
		/// state. After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> so the garbage collector can reclaim the memory that the
		/// <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> was occupying.</remarks>
		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Gets a value indicating whether this instance can seek.
		/// </summary>
		/// <value><c>true</c> if this instance can seek; otherwise, <c>false</c>.</value>
		public bool CanSeek
		{
			get { return m_stream == null ? false : m_stream.CanSeek; }
		}

		/// <summary>
		/// Seek the specified offset and origin.
		/// </summary>
		/// <param name="offset">Offset.</param>
		/// <param name="origin">Origin.</param>
		public long Seek(long offset, SeekOrigin origin)
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (!m_stream.CanSeek) throw new NotSupportedException("Stream not seekable.");

			long currentpos = m_stream.Position - (capInBuffer - posInBuffer);
			long pos = 0;

			switch (origin)
			{
				case SeekOrigin.Begin:
					{
						break;
					}
				case SeekOrigin.Current:
					{
						pos = currentpos;
						break;
					}
				case SeekOrigin.End:
					{
						pos = m_stream.Length;
						break;
					}
			}

			m_stream.Seek(pos + offset, SeekOrigin.Begin);
			FillBuffer();

			return currentpos;
		}

		/// <summary>
		/// Skip the specified offset.
		/// </summary>
		/// <param name="offset">Offset.</param>
		public void Skip(long offset)
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (capInBuffer <= posInBuffer) throw new EndOfStreamException();

			if (offset > capInBuffer - posInBuffer)
			{
				if (!m_stream.CanSeek)
				{
					offset -= capInBuffer - posInBuffer;

					if (m_buffer == null || m_buffer.Length < i_bufferSize) m_buffer = new byte[i_bufferSize];

					do
					{
						int len = (int)Math.Min(offset, i_bufferSize);
						int read = m_stream.Read(m_buffer, 0, len);
						if (read == 0) throw new EndOfStreamException();

						offset -= read;
					}
					while(offset > 0);

					FillBuffer();
				}
				else Seek(offset, SeekOrigin.Current);
			}
			else
			{
				posInBuffer += (int)offset;

				if (posInBuffer == capInBuffer) FillBuffer();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Xevle.IO.Streams.BufferedBinaryReader"/> is EO.
		/// </summary>
		/// <value><c>true</c> if EO; otherwise, <c>false</c>.</value>
		public bool EOF
		{
			get { return capInBuffer <= posInBuffer; }
		}

		/// <summary>
		/// Releases the inner stream.
		/// </summary>
		/// <returns>The inner stream.</returns>
		public Stream ReleaseInnerStream()
		{
			if (m_stream == null) throw new ObjectDisposedException(null, "File closed");
			if (!m_stream.CanSeek) throw new NotSupportedException("Stream not seekable.");

			m_stream.Seek(m_stream.Position - (capInBuffer - posInBuffer), SeekOrigin.Begin);
			Stream ret = m_stream;
			m_stream = null;

			Dispose(true);

			return ret;
		}
	}
}
