using SME;
using System;
using System.Threading.Tasks;

namespace BPUImplementation
{
	[ClockedProcess]
	public class PipelineStage2 : SimpleProcess
	{
		[ClockedBus]
		public interface ILatch : IBus
		{
			[InitialValue(false)]
			bool WrMemValid { get; set; }
			MemoryAddress WrMemAddr { get; set; }
			[InitialValue(AluOpcode.Noop)]
			AluOpcode Opcode { get; set; }
			[InitialValue(false)]
			bool DstValid { get; set; }
			RegisterAddress DstReg { get; set; }
		}

		[OutputBus]
		private MicrocodeDriver.IInput ReadyFeedback;
		[OutputBus]
		private ILatch Latch;
		[OutputBus]
		private RegisterOutputMultiplexer.IInput Register;

		[InputBus]
		private ExecuteFeeder.IOutput ExecuteBus;
		[InputBus]
		private MemoryReader.IOutput ReadBus;
		[InputBus]
		private MemoryWriteFeeder.IOutput WriteBus;



		protected override void OnTick()
		{
			// Defaults
			ReadyFeedback.ReadReady = ReadBus.Ready;
			ReadyFeedback.WriteReady = WriteBus.Ready;
			ReadyFeedback.ExecuteReady = ExecuteBus.Ready;

			Latch.WrMemAddr = 0;
			Latch.DstReg = 0;

			Register.Source1 = 0;
			Register.Source2 = 0;

			// Port map
			Latch.WrMemValid = WriteBus.Valid;
			if (WriteBus.Valid)
			{
				Latch.WrMemAddr = WriteBus.Address;
			}

			Latch.DstValid = ExecuteBus.DstValid;
			Latch.Opcode = ExecuteBus.Opcode;
			if (ExecuteBus.DstValid)
			{
				Latch.DstReg = ExecuteBus.DstReg;
				Register.Source1 = ExecuteBus.Reg1Source;
				Register.Source2 = ExecuteBus.Reg2Source;
			}

		}
	}
}

