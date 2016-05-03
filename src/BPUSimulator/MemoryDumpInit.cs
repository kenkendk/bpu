using System;
using BPUImplementation;

namespace BPUSimulator
{
	internal static class ConvertHelper
	{
		public static byte[] ConvertToBytes(RegisterData[] data)
		{
			var memory = new byte[data.Length * 2];
			for (var i = 0; i < data.Length; i++)
			{
				memory[i * 2] = (byte)((data[i] >> 8) & 0xf);
				memory[(i * 2) + 1] = (byte)((data[i]) & 0xf);
			}

			return memory;
		}
	}

	public partial class MemoryDump
	{
		private byte[] memory;
		public MemoryDump(RegisterData[] memory)
		{
			this.memory = ConvertHelper.ConvertToBytes(memory);
		}
	}

	public partial class MemoryDumpCOE
	{
		private byte[] memory;
		public MemoryDumpCOE(RegisterData[] memory)
		{
			this.memory = ConvertHelper.ConvertToBytes(memory);
		}
	}

	public partial class MemoryDumpMIF
	{
		private byte[] memory;
		public MemoryDumpMIF(RegisterData[] memory)
		{
			this.memory = ConvertHelper.ConvertToBytes(memory);
		}
	}

}

