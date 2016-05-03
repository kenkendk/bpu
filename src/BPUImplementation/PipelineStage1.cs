using System;
using SME;
using System.Threading.Tasks;

namespace BPUImplementation
{
	[ClockedProcess]
	public class PipelineStage1 : SimpleProcess
	{
		[InputBus]
		private MicrocodeDriver.IOutput CommandsIn;
		[InputBus]
		private MemoryWriter.IOutput MemoryStatusIn;

		[OutputBus]
		private MemoryReader.IInput MemoryReadOut;
		[OutputBus]
		private MemoryWriteFeeder.IInput MemoryWriteOut;
		[OutputBus]
		private ExecuteFeeder.IInput ExecuteOut;
		[OutputBus]
		private MicrocodeDriver.IInput MemoryStatusOut;


		protected override void OnTick()
		{
			// Defaults
			MemoryReadOut.StartAddr = 0;
			MemoryReadOut.StartReg = 0;
			MemoryReadOut.RepeatCount = 0;

			MemoryWriteOut.StartAddr = 0;
			MemoryWriteOut.StartReg = 0;
			MemoryWriteOut.RepeatCount = 0;

			ExecuteOut.Opcode = Opcode.Noop;
			ExecuteOut.StartSrcReg1 = 0;
			ExecuteOut.StartSrcReg2 = 0;
			ExecuteOut.StartDstReg = 0;
			ExecuteOut.RepeatCount = 0;
			ExecuteOut.Reg1Source = false;
			ExecuteOut.Reg2Source = false;

			// Port map
			MemoryReadOut.Valid = CommandsIn.MemReadValid;
			if (CommandsIn.MemReadValid)
			{
				MemoryReadOut.StartAddr = CommandsIn.MemReadStartAddr;
				MemoryReadOut.StartReg = CommandsIn.MemReadStartReg;
				MemoryReadOut.RepeatCount = CommandsIn.MemReadRepeatCount;
				MemoryReadOut.AddrStride = CommandsIn.MemReadAdrStride;
				MemoryReadOut.RegStride = CommandsIn.MemReadRegStride;
			}

			MemoryWriteOut.Valid = CommandsIn.MemWriteValid;
			if (CommandsIn.MemWriteValid)
			{
				MemoryWriteOut.StartAddr = CommandsIn.MemWriteStartAddr;
				MemoryWriteOut.StartReg = CommandsIn.MemWriteStartReg;
				MemoryWriteOut.RepeatCount = CommandsIn.MemWriteRepeatCount;
				MemoryWriteOut.AddrStride = CommandsIn.MemWriteAdrStride;
				MemoryWriteOut.RegStride = CommandsIn.MemWriteRegStride;
			}

			ExecuteOut.Valid = CommandsIn.ExValid;
			if (CommandsIn.ExValid)
			{
				ExecuteOut.Opcode = CommandsIn.ExOpcode;
				ExecuteOut.StartSrcReg1 = CommandsIn.ExStartSrcReg1;
				ExecuteOut.StartSrcReg2 = CommandsIn.ExStartSrcReg2;
				ExecuteOut.StartDstReg = CommandsIn.ExStartDstReg;
				ExecuteOut.RepeatCount = CommandsIn.ExRepeatCount;
				ExecuteOut.SrcReg1Stride = CommandsIn.ExStartSrcReg1Stride;
				ExecuteOut.SrcReg2Stride = CommandsIn.ExStartSrcReg2Stride;
				ExecuteOut.DstRegStride = CommandsIn.ExStartDstRegStride;
				ExecuteOut.Reg1Source = CommandsIn.ExReg1MemSource;
				ExecuteOut.Reg2Source = CommandsIn.ExReg2MemSource;
			}

			MemoryWriteOut.Blocked = MemoryStatusIn.Blocked;
			MemoryStatusOut.MemoryQueueSize = MemoryStatusIn.QueueSize;
		}
	}
}

