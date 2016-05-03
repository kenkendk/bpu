using System;
using SME;
using SME.VHDLComponents;
using SME.Render.VHDL;

namespace BPUImplementation
{
	[VHDLComponent("mock_mem")]
	public class MockMemoryItem : SimpleDualPortMemory<MemoryAddress, RegisterData>
	{
		public new interface IReadIn : SimpleDualPortMemory<MemoryAddress, RegisterData>.IReadIn { }
		public new interface IReadOut : SimpleDualPortMemory<MemoryAddress, RegisterData>.IReadOut { }
		public new interface IWriteIn : SimpleDualPortMemory<MemoryAddress, RegisterData>.IWriteIn { }

		protected override int AddressWidth { get { return 10; } }
		protected override int DataWidth { get { return RegisterData.DATA_LENGTH * 8; } }

		public MockMemoryItem()
		{
		}

		public MockMemoryItem(Clock clock)
			: base(clock)
		{
			//this.DebugOutput = true;
			m_memory = new RegisterData[EmulatedMemoryStorage.m_data.Length / RegisterData.DATA_LENGTH];
			for (var i = 0; i < m_memory.Length; i++)
				m_memory[i] = EmulatedMemoryStorage.Read((uint)i * RegisterData.DATA_LENGTH);
		}

		protected override void OnTick()
		{
			if (WriteIn.Enabled)
				PrintDebug("Writing value {0} to memory address {1}", WriteIn.Data, WriteIn.Address);

			if (ConvertAddress(ReadIn.Address) != 0)
				PrintDebug("Read memory {0} with value {1}", ReadIn.Address, m_memory[ConvertAddress(ReadIn.Address)]);
			
			base.OnTick();
		}

		public void WriteMemoryBack()
		{
			for (var i = 0; i < m_memory.Length; i++)
				EmulatedMemoryStorage.Write((uint)i * RegisterData.DATA_LENGTH, m_memory[i]);
		}

		protected override void Setup(Clock clock)
		{
			SetBusses<IReadIn, IReadOut, IWriteIn>(clock);
		}

		protected override int ConvertAddress(MemoryAddress adr)
		{
			return (int)(uint)adr;
		}
	}
}

