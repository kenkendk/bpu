using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace bpusmecompiler
{
	public static class Deserializer
	{
		private class StreamWrapper : IDisposable
		{ 
			private Stream m_stream;
			private byte[] m_buffer = new byte[32];

			public StreamWrapper(Stream s)
			{
				this.m_stream = s;
			}

			public byte[] Read(int size, byte[] buffer = null)
			{
				m_buffer = m_buffer ?? new byte[size];
				var remain = size;
				int offset = 0;
				while (remain > 0)
				{
					var r = m_stream.Read(m_buffer, offset, remain);
					if (r == 0)
						throw new InvalidDataException(string.Format("File is too short, got {0} bytes while expecting {1}", r, remain));
					remain -= r;
					offset += r;
				}

				return m_buffer;
			}

			public ushort ReadUInt16()
			{
				return BitConverter.ToUInt16(Read(2), 0);
			}

			public uint ReadUInt32()
			{
				return BitConverter.ToUInt32(Read(4), 0);
			}

			public ulong ReadUInt64()
			{
				return BitConverter.ToUInt64(Read(8), 0);
			}

			public short ReadInt16()
			{
				return BitConverter.ToInt16(Read(2), 0);
			}

			public int ReadInt32()
			{
				return BitConverter.ToInt32(Read(4), 0);
			}

			public long ReadInt64()
			{
				return BitConverter.ToInt64(Read(8), 0);
			}

			public ulong ReadIntPtr()
			{
				return ReadUInt64();
			}

			public void Dispose()
			{
				if (m_stream != null)
					m_stream.Dispose();
			}

		}

		public static Dictionary<ulong, ulong> DeserializeMemoryMap(string filename)
		{
			using (var fs = new StreamWrapper(File.OpenRead(filename)))
			{
				var entries = fs.ReadUInt64();
				if (entries > 10000)
					throw new InvalidDataException(string.Format("There are too many entries: {0}", entries));

				var map = new Dictionary<ulong, ulong>();

				for (var i = 0; i < (int)entries; i++)
				{
					var ptr = fs.ReadIntPtr();
					var offset = fs.ReadUInt64();

					map.Add(ptr, offset);
				}

				return map;
			}
		}

		public static Instruction[] DeserializeKernel(string filename)
		{
			using (var fs = new StreamWrapper(File.OpenRead(filename)))
			{
				var instructions = fs.ReadUInt64();
				if (instructions > 10000)
					throw new InvalidDataException(string.Format("There are too many instructions: {0}", instructions));

				var res = new Instruction[instructions];

				for (var i = 0; i < res.Length; i++)
				{
					res[i].Opcode = (bh_opcode)fs.ReadInt64();
					var tmp = fs.Read(16);
					var h = GCHandle.Alloc(tmp, GCHandleType.Pinned);
					try
					{
						res[i].Constant.Value = Marshal.PtrToStructure<ConstantValue>(h.AddrOfPinnedObject());
					}
					finally
					{
						if (h.IsAllocated)
							h.Free();
					}

					res[i].Constant.Type = (bh_type)fs.ReadInt64();

					var operands = fs.ReadUInt32();
					if (operands > 10)
						throw new InvalidDataException(string.Format("There are too many operands: {0}", operands));
					
					res[i].Operands = new View[(int)operands];

					for (var j = 0; j < operands; j++)
					{
						res[i].Operands[j].BasePointer = fs.ReadUInt64();
						if (res[i].Operands[j].BasePointer != 0)
						{
							var b = new Base();
							b.Data = fs.ReadUInt64();
							b.Type = (bh_type)fs.ReadUInt64();
							b.NElement = fs.ReadInt64();

							res[i].Operands[j].Base = b;
						}

						res[i].Operands[j].Start = fs.ReadInt64();
						var dimensions = fs.ReadUInt64();
						if (dimensions > 100)
							throw new InvalidDataException(string.Format("There are too many dimensions: {0}", dimensions));

						res[i].Operands[j].ndim = dimensions;
						res[i].Operands[j].Shape = new long[(int)dimensions];
						res[i].Operands[j].Stride = new long[(int)dimensions];

						for (var k = 0; k < (int)dimensions; k++)
						{
							res[i].Operands[j].Shape[k] = fs.ReadInt64();
							res[i].Operands[j].Stride[k] = fs.ReadInt64();
						}
					}
				}

				return res;
			}

		}
	}
}

