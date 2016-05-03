using SME;
using System;
using System.Threading.Tasks;

namespace BPUImplementation
{
	[ClockedProcess]
	public class PipelineStage3 : SimpleProcess
	{
		[InputBus]
		private PipelineStage2.ILatch LatchIn;
		[InputBus]
		private RegisterOutputMultiplexer.IOutput RegisterOut;
		[InputBus]
		private RegisterBank2.IReadOut Bank2In;

		[OutputBus]
		private MemoryWriter.IInput WriteOut;
		[OutputBus]
		private ALU.IInput AluOut;

		protected override void OnTick()
		{
			// Defaults
			WriteOut.Address = 0;
			WriteOut.Data = 0;
			AluOut.Op1 = 0;
			AluOut.Op2 = 0;

			// Port map
			WriteOut.Valid = LatchIn.WrMemValid;
			if (LatchIn.WrMemValid)
			{
				WriteOut.Address = LatchIn.WrMemAddr;
				WriteOut.Data = Bank2In.Data;
			}

			AluOut.Opcode = LatchIn.Opcode;
			if (LatchIn.DstValid)
			{
				AluOut.Dst = LatchIn.DstReg;
				AluOut.Op1 = RegisterOut.ExData1;
				AluOut.Op2 = RegisterOut.ExData2;
				AluOut.Valid = LatchIn.DstValid;
			}
		}
	}
}

