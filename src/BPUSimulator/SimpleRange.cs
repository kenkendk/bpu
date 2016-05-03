using System;
using BPUImplementation;

namespace BPUSimulator
{
	public class SimpleRange : TestSetup
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
				return new RegisterData[] {0, 2, 12, 36, 80, 0, 1, 2, 3, 4 };
			}
		}

		public override Microcode[] Program
		{
			get
			{
				return new Microcode[] {
					new Microcode(0,0,0,   0,0,0,0,0,0, 1,5,0,1,0,1, 0, Opcode.Noop,        0,0,0,0,0,0,0,0,0,    1), // Read 5 elements from adr=0 to reg=0
					new Microcode(1,0,0,   0,0,0,0,0,0, 0,0,0,0,0,0, 1, Opcode.Add,         0,0,0,5,1,1,1,1,1,    2), // Add from mem:0-5 into self
					new Microcode(0,0,1,   0,0,0,0,0,0, 0,0,0,0,0,0, 1, Opcode.AddReduce,   0,0,0,5,1,1,1,0,0,    3), // Reduce from reg:0-5 into self
					new Microcode(0,0,1,   0,0,0,0,0,0, 0,0,0,0,0,0, 1, Opcode.Range,       0,0,5,5,1,1,1,0,0,    4), // Range to reg:5-10
					new Microcode(0,0,1,   1,5,5,1,5,1, 0,0,0,0,0,0, 1, Opcode.Mul,         0,5,0,5,1,1,1,0,0,    5), // Mul reg0:5 with reg5-10 into self
					new Microcode(0,1,1,   1,5,0,1,0,1, 0,0,0,0,0,0, 0, Opcode.Noop,        0,0,0,0,0,0,0,0,0,    6), // Write 5 elements from reg=0 to adr=0
					new Microcode(0,1,0,   0,0,0,0,0,0, 0,0,0,0,0,0, 0, Opcode.Noop,        0,0,0,0,0,0,0,0,0,   (ushort)(Microcode.MAX_INSTRUCTION_COUNT - 1)), // NOOP, wait for mem write
				};
			}
		}
	}
}

