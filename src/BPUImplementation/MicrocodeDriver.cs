using SME;
using System;
using System.Linq;
using SME.Render.VHDL;

namespace BPUImplementation
{
	public class MicrocodeDriver : SimpleProcess
	{
		[InitializedBus]
		public interface IInput : IBus
		{
			bool ReadReady { get; set; }
			bool WriteReady { get; set; }
			bool ExecuteReady { get; set; }
			byte MemoryQueueSize { get; set; }
		}

		public interface IOutput : IBus
		{
			[InitialValue(false)]
			bool MemReadValid { get; set; }
			MemoryAddress MemReadStartAddr { get; set; }
			RegisterAddress MemReadStartReg { get; set; }
			Counter MemReadRepeatCount { get; set; }
			OffsetCounter MemReadRegStride { get; set; }
			OffsetCounter MemReadAdrStride { get; set; }

			[InitialValue(false)]
			bool MemWriteValid { get; set; }
			MemoryAddress MemWriteStartAddr { get; set; }
			RegisterAddress MemWriteStartReg { get; set; }
			Counter MemWriteRepeatCount { get; set; }
			OffsetCounter MemWriteRegStride { get; set; }
			OffsetCounter MemWriteAdrStride { get; set; }

			[InitialValue(false)]
			bool ExValid { get; set; }
			Opcode ExOpcode { get; set; }
			RegisterAddress ExStartSrcReg1 { get; set; }
			RegisterAddress ExStartSrcReg2 { get; set; }
			RegisterAddress ExStartDstReg { get; set; }
			Counter ExRepeatCount { get; set; }

			OffsetCounter ExStartSrcReg1Stride { get; set; }
			OffsetCounter ExStartSrcReg2Stride { get; set; }
			OffsetCounter ExStartDstRegStride { get; set; }

			[InitialValue]
			bool ExReg1MemSource { get; set; }
			[InitialValue]
			bool ExReg2MemSource { get; set; }

			[InitialValue(false)]
			bool Completed { get; set; }

		}

		[InitializedBus]
		public interface IInternal : IBus
		{
			[InitialValue(0)]
			ushort PC { get; set; }
			bool Need_RD_Wait { get; set; }
			bool Need_WR_Wait { get; set; }
			bool Need_EX_Wait { get; set; }
			bool CompletePhase { get; set; }
			[InitialValue(0)]
			byte CompleteCountdown { get; set; }
		}

		[InputBus]
		private IInput ReadySignals;
		[InputBus]
		private MemoryWriter.IOutput MemorySignal;

		[OutputBus]
		private IOutput Command;

		[InternalBus]
		private IInternal Internal;

		[VHDLIgnore]
		private readonly Microcode[] Program = ProgramMemory.Program;

		protected override void OnTick()
		{
			//DebugOutput = true;
			Command.MemReadValid = false;
			Command.MemWriteValid = false;
			Command.ExValid = false;

			Command.MemReadStartAddr = 0;
			Command.MemReadStartReg = 0;
			Command.MemReadRepeatCount = 0;
			Command.MemReadAdrStride = 0;
			Command.MemReadRegStride = 0;

			Command.MemWriteStartAddr = 0;
			Command.MemWriteStartReg = 0;
			Command.MemWriteRepeatCount = 0;
			Command.MemWriteAdrStride = 0;
			Command.MemWriteRegStride = 0;

			Command.ExOpcode = Opcode.Noop;
			Command.ExStartSrcReg1 = 0;
			Command.ExStartSrcReg2 = 0;
			Command.ExStartDstReg = 0;
			Command.ExRepeatCount = 0;
			Command.ExStartSrcReg1Stride = 0;
			Command.ExStartSrcReg2Stride = 0;
			Command.ExStartDstRegStride = 0;


			Command.ExReg1MemSource = false;
			Command.ExReg2MemSource = false;

			Command.Completed = false;


			if (Internal.CompletePhase)
			{
				if (Internal.Need_RD_Wait && MemorySignal.QueueSize > 0)
					Internal.CompleteCountdown = 2;
				else if (Internal.CompleteCountdown == 0)
					Command.Completed = MemorySignal.QueueSize == 0;
				else
					Internal.CompleteCountdown--;
			}
			else
			{
				if (Internal.Need_RD_Wait && ReadySignals.ReadReady)					
					Internal.Need_RD_Wait = false;
				if (Internal.Need_WR_Wait && ReadySignals.WriteReady)
					Internal.Need_WR_Wait = false;
				if (Internal.Need_EX_Wait && ReadySignals.ExecuteReady)
					Internal.Need_EX_Wait = false;

				if ((!Internal.Need_RD_Wait || ReadySignals.ReadReady) && (!Internal.Need_WR_Wait || ReadySignals.WriteReady) && (!Internal.Need_EX_Wait || ReadySignals.ExecuteReady))
				{
					var instr = Program[Internal.PC];

					PrintDebug("Issuing instruction at {0}", Internal.PC);

					Command.ExReg1MemSource = instr.ex_src1_mem;
					Command.ExReg2MemSource = instr.ex_src2_mem;

					if (instr.rd_mem_valid)
					{
						PrintDebug("Issuing read from @{0}-@{1} into r{2}-r{3}", instr.rd_mem_adr, instr.rd_mem_adr + instr.rd_mem_cnt, instr.rd_mem_reg, instr.rd_mem_reg + instr.rd_mem_cnt);
						Command.MemReadValid = true;
						Command.MemReadStartAddr = instr.rd_mem_adr;
						Command.MemReadStartReg = instr.rd_mem_reg;
						Command.MemReadRepeatCount = instr.rd_mem_cnt;
						Command.MemReadAdrStride = instr.rd_mem_adr_stride;
						Command.MemReadRegStride = instr.rd_mem_reg_stride;
					}

					if (instr.wr_mem_valid)
					{
						PrintDebug("Issuing write from r{0}-r{1} into @{2}-@{3}", instr.wr_mem_reg, instr.wr_mem_reg + instr.wr_mem_cnt, instr.wr_mem_adr, instr.wr_mem_adr + instr.wr_mem_cnt);
						Command.MemWriteValid = true;
						Command.MemWriteStartAddr = instr.wr_mem_adr;
						Command.MemWriteStartReg = instr.wr_mem_reg;
						Command.MemWriteRepeatCount = instr.wr_mem_cnt;
						Command.MemWriteAdrStride = instr.wr_mem_adr_stride;
						Command.MemWriteRegStride = instr.wr_mem_reg_stride;
					}

					if (instr.ex_valid)
					{
						PrintDebug("Issuing execute {4} from r{0}-r{1} into r{2}-r{3}", instr.ex_src_reg1, instr.ex_src_reg1 + instr.ex_cnt, instr.ex_dst_reg, instr.ex_dst_reg + instr.ex_cnt, instr.ex_opcode);

						Command.ExValid = true;
						Command.ExOpcode = instr.ex_opcode;
						Command.ExStartSrcReg1 = instr.ex_src_reg1;
						Command.ExStartSrcReg2 = instr.ex_src_reg2;
						Command.ExStartDstReg = instr.ex_dst_reg;
						Command.ExRepeatCount = instr.ex_cnt;
						Command.ExStartSrcReg1Stride = instr.ex_src_reg1_stride;
						Command.ExStartSrcReg2Stride = instr.ex_src_reg2_stride;
						Command.ExStartDstRegStride = instr.ex_dst_reg_stride;
					}

					Internal.PC = instr.nextpc;
					if (instr.nextpc == Microcode.MAX_INSTRUCTION_COUNT - 1)
					{
						Internal.CompletePhase = true;
						Internal.Need_RD_Wait = instr.rd_wait;
						Internal.Need_WR_Wait = instr.wr_wait;
						Internal.Need_EX_Wait = instr.ex_wait;
					}
					else
					{
						var nextinstr = Program[instr.nextpc];
						Internal.Need_RD_Wait = nextinstr.rd_wait;
						Internal.Need_WR_Wait = nextinstr.wr_wait;
						Internal.Need_EX_Wait = nextinstr.ex_wait;
					}
				}
			}
		}

	}
}

