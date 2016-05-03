using System;
using SME;
using SME.Render.VHDL;

namespace BPUImplementation
{
	public class RegisterOutputMultiplexer : SimpleProcess
	{
		public interface IInput : IBus
		{
			UInt2 Source1 { get; set; }
			UInt2 Source2 { get; set; }
		}
		
		public interface IOutput : IBus
		{
			RegisterData ExData1 { get; set; }
			RegisterData ExData2 { get; set; }
		}
			

		[InputBus]
		private RegisterBank0.IReadOut Bank0In;
		[InputBus]
		private RegisterBank1.IReadOut Bank1In;
		[InputBus]
		private RegisterBank3.IReadOut Bank3In;
		[InputBus]
		private RegisterBank4.IReadOut Bank4In;
		[InputBus]
		private IInput Input;
		[InputBus]
		private ALU.IOutput AluRes;

		[OutputBus]
		private IOutput Output;

		public static readonly UInt2 SOURCE_REG = 0; 
		public static readonly UInt2 SOURCE_MEM = 1; 
		public static readonly UInt2 SOURCE_ALU = 2;


		protected override void OnTick()
		{
			if (Input.Source1 == SOURCE_MEM)
				Output.ExData1 = Bank0In.Data;
			else if (Input.Source1 == SOURCE_ALU)
				Output.ExData1 = AluRes.Result;
			else
				Output.ExData1 = Bank3In.Data;

			if (Input.Source2 == SOURCE_MEM)
				Output.ExData2 = Bank1In.Data;
			else if (Input.Source2 == SOURCE_ALU)
				Output.ExData2 = AluRes.Result;
			else
				Output.ExData2 = Bank4In.Data;
		}
	}
}

