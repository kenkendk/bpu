using System;

namespace bpusmecompiler
{
	public class TestRunner : BPUSimulator.TestSetup
	{
		private BPUImplementation.RegisterData[] m_premem;
		private BPUImplementation.Microcode[] m_program;

		public TestRunner(BPUImplementation.RegisterData[] memory, BPUImplementation.Microcode[] program)
		{
			m_premem = memory;
			m_program = program;
		}

		public override int MemorySize
		{
			get
			{
				return m_premem.Length * BPUImplementation.RegisterData.DATA_LENGTH;
			}
		}

		public override BPUImplementation.RegisterData[] PreMemory
		{
			get
			{
				return m_premem;
			}
		}

		public override BPUImplementation.RegisterData[] PostMemory
		{
			get
			{
				return null;
			}
		}

		public override BPUImplementation.Microcode[] Program
		{
			get
			{
				return m_program;
			}
		}

	}
}

