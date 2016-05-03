using System;
using SME;

namespace BPUImplementation
{
	public class EmulatedMemoryStorage
	{
		public static byte[] m_data = new byte[1024*1024*10];

		public static RegisterData Read(MemoryAddress address, byte[] storage = null)
		{
			uint v = 0;
			for (var i = 0; i < RegisterData.DATA_LENGTH; i++)
				if (BitConverter.IsLittleEndian)
					v = v | (uint)((storage ?? m_data)[(address + RegisterData.DATA_LENGTH) - (i + 1)] << (i * 8));
				else
					v = v | (uint)((storage ?? m_data)[address + i] << (i * 8));

			//Console.WriteLine("Read {0} from {1}*2", v, address / 2);
			return v;
		}

		public static void Write(MemoryAddress address, RegisterData data, byte[] storage = null)
		{
			//Console.WriteLine("Writing {0} to {1}*2", data, address / 2);
			var d = BitConverter.GetBytes(data);
			for (var i = 0; i < RegisterData.DATA_LENGTH; i++)
				if (BitConverter.IsLittleEndian)
					(storage ?? m_data)[(address + RegisterData.DATA_LENGTH) - (i + 1)] = d[i];
				else
					(storage ?? m_data)[address + i] = d[i];
		}
	}
}

