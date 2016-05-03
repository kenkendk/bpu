using System;
using BPUImplementation;
using SME;

namespace BPUSimulator
{
	public class SimpleAddMultiply : TestSetup
	{
		public override RegisterData[] PreMemory
		{
			get
			{
				return new RegisterData[] {0, 1, 2, 3, 4};
			}
		}

		public override RegisterData[] PostMemory
		{
			get
			{
				return new RegisterData[] {0, 4, 16, 36, 64};
			}
		}

		public override Microcode[] Program
		{
			get
			{
				return new Microcode[] {
					new Microcode(0,0,0,   0,0,0,1,0,1, 1,5,0,1,0,1, 0, Opcode.Noop,  0,0,0,0,0,0,0,0,0,    1), // Read 5 elements from adr=0 to reg=0
					new Microcode(1,0,0,   0,0,0,0,0,0, 0,0,0,0,0,0, 1, Opcode.Add,   0,0,0,5,1,1,1,1,1,    2), // Add from mem:0-5 into self
					new Microcode(0,0,1,   0,0,0,0,0,0, 0,0,0,0,0,0, 1, Opcode.Mul,   0,0,0,5,1,1,1,0,0,    3), // Mul from reg:0-5 into self
					new Microcode(0,0,1,   1,5,0,1,0,1, 0,0,0,0,0,0, 0, Opcode.Noop,  0,0,0,0,0,0,0,0,0,    4), // Write 5 elements from reg=0 to adr=0
					new Microcode(0,1,0,   0,0,0,0,0,0, 0,0,0,0,0,0, 0, Opcode.Noop,  0,0,0,0,0,0,0,0,0,   (ushort)(Microcode.MAX_INSTRUCTION_COUNT - 1)), // NOOP, wait for mem write
				};
			}
		}
	}
}

