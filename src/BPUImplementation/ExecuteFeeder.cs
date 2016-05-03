using SME;
using System;
using System.Threading.Tasks;
using SME.Render.VHDL;

namespace BPUImplementation
{
	public class ExecuteFeeder : SimpleProcess
	{
		public interface IOutput : IBus
		{
			[InitialValue(false)]
			bool DstValid { get; set; }
			RegisterAddress SrcReg1 { get; set; }
			RegisterAddress SrcReg2 { get; set; }
			[InitialValue(AluOpcode.Noop)]
			AluOpcode Opcode { get; set; }
			RegisterAddress DstReg { get; set; }
			[InitialValue(false)]
			bool Ready { get; set; }
			UInt2 Reg1Source { get; set; }
			UInt2 Reg2Source { get; set; }
		}

		public interface IInput: IBus
		{
			[InitialValue(false)]
			bool Valid { get; set; }
			Opcode Opcode { get; set; }
			RegisterAddress StartSrcReg1 { get; set; }
			RegisterAddress StartSrcReg2 { get; set; }
			RegisterAddress StartDstReg { get; set; }
			Counter RepeatCount { get; set; }
			bool Reg1Source { get; set; }
			bool Reg2Source { get; set; }

			OffsetCounter SrcReg1Stride { get; set; }
			OffsetCounter SrcReg2Stride { get; set; }
			OffsetCounter DstRegStride { get; set; }
		}

		[InitializedBus]
		public interface IInternal : IBus
		{
			bool Ready { get; set; }
			Counter Count { get; set; }
			RegisterAddress SrcReg1 { get; set; }
			RegisterAddress SrcReg2 { get; set; }
			RegisterAddress DstReg { get; set; }
			Opcode Opcode { get; set; }
			OffsetCounter SrcReg1Stride { get; set; }
			OffsetCounter SrcReg2Stride { get; set; }
			OffsetCounter DstRegStride { get; set; }
			bool First { get; set; }
			bool SrcReg1Mem { get; set; }
			bool SrcReg2Mem { get; set; }
		}

		[InputBus]
		private IInput In;

		[OutputBus]
		private IOutput Out;

		[InternalBus]
		private IInternal Internal;

		[VHDLCompile]
		private static AluOpcode ConvertOpToAlu(Opcode c)
		{
			switch (c)
			{
				case Opcode.Add:
				case Opcode.AddReduce:
					return AluOpcode.Add;
				case Opcode.Sub:
				case Opcode.SubReduce:
					return AluOpcode.Sub;
				case Opcode.Mul:
				case Opcode.MulReduce:
					return AluOpcode.Mul;
				//case Opcode.Div:
				//case Opcode.DivReduce:
				//	return AluOpcode.Div;
				case Opcode.Min:
				case Opcode.MinReduce:
					return AluOpcode.Min;
				case Opcode.Max:
				case Opcode.MaxReduce:
					return AluOpcode.Max;
				case Opcode.Range:
					return AluOpcode.IncrA;
				case Opcode.Equal:
					return AluOpcode.Equal;
				case Opcode.NotEqual:
					return AluOpcode.NotEqual;
				case Opcode.GreaterThan:
					return AluOpcode.GreaterThan;
				case Opcode.GreaterThanEqual:
					return AluOpcode.GreaterThanEqual;
				case Opcode.LessThan:
					return AluOpcode.LessThan;
				case Opcode.LessThanEqual:
					return AluOpcode.LessThanEqual;
				case Opcode.BitwiseAnd:
				case Opcode.BitwiseAndReduce:
					return AluOpcode.BitwiseAnd;
				case Opcode.BitwiseOr:
				case Opcode.BitwiseOrReduce:
					return AluOpcode.BitwiseOr;
				case Opcode.BitwiseXor:
				case Opcode.BitwiseXorReduce:
					return AluOpcode.BitwiseXor;
				case Opcode.LogicalAnd:
				case Opcode.LogicalAndReduce:
					return AluOpcode.LogicalAnd;
				case Opcode.LogicalOr:
				case Opcode.LogicalOrReduce:
					return AluOpcode.LogicalOr;
				case Opcode.LogicalXor:
				case Opcode.LogicalXorReduce:
					return AluOpcode.LogicalXor;
				default:
					return AluOpcode.Noop;
			}
		}

		[VHDLCompile]
		private static bool IsReduction(Opcode c)
		{
			switch (c)
			{
				case Opcode.AddReduce:
				case Opcode.SubReduce:
				case Opcode.MulReduce:
				case Opcode.DivReduce:
				case Opcode.MinReduce:
				case Opcode.MaxReduce:
				case Opcode.BitwiseOrReduce:
				case Opcode.BitwiseAndReduce:
				case Opcode.BitwiseXorReduce:
				case Opcode.LogicalOrReduce:
				case Opcode.LogicalAndReduce:
				case Opcode.LogicalXorReduce:
					return true;

				default:
					return false;
			}			
		}

		protected override void OnTick()
		{
			Out.DstValid = false;
			Out.SrcReg1 = 0;
			Out.SrcReg2 = 0;
			Out.Opcode = AluOpcode.Noop;
			Out.DstReg = 0;
			Out.Ready = false;
			Out.Reg1Source = 0;
			Out.Reg2Source = 0;

			Internal.Ready = false;

			if (In.Valid && Internal.Count == 0)
			{
				if (In.RepeatCount == 0)
				{
					Out.Ready = true;
				}
				else
				{
					Internal.Count = In.RepeatCount - 1;
					Internal.SrcReg1 = (RegisterAddress)(In.StartSrcReg1 + (int)In.SrcReg1Stride);
					Internal.SrcReg2 = (RegisterAddress)(In.StartSrcReg2 + (int)In.SrcReg2Stride);
					Internal.DstReg = (RegisterAddress)(In.StartDstReg + (int)In.DstRegStride);
					Internal.SrcReg1Mem = In.Reg1Source;
					Internal.SrcReg2Mem = In.Reg2Source;
					Internal.Opcode = In.Opcode;
					Internal.SrcReg1Stride = In.SrcReg1Stride;
					Internal.SrcReg2Stride = In.SrcReg2Stride;
					Internal.DstRegStride = In.DstRegStride;
					Internal.First = false;

					Out.SrcReg1 = In.StartSrcReg1;
					Out.SrcReg2 = In.StartSrcReg2;
					Out.DstReg = In.StartDstReg;
					Out.Opcode = ConvertOpToAlu(In.Opcode);
					Out.DstValid = true;


					Out.Reg1Source = In.Reg1Source ? RegisterOutputMultiplexer.SOURCE_MEM : RegisterOutputMultiplexer.SOURCE_REG;
					Out.Reg2Source = In.Reg2Source ? RegisterOutputMultiplexer.SOURCE_MEM : RegisterOutputMultiplexer.SOURCE_REG;

					if (In.RepeatCount == 1)
						Out.Ready = true;

					// Override if we are doing a reduction
					if (IsReduction(In.Opcode))
					{
						Out.Opcode = AluOpcode.CopyA;
						Internal.SrcReg2 = (RegisterAddress)(In.StartSrcReg1 + In.SrcReg1Stride);
					}
					else if (In.Opcode == Opcode.Range)
					{
						Out.Opcode = AluOpcode.Zero;
					}
				}
			}
			else
			{
				if (In.Valid && Internal.Count == 1)
				{
					Internal.Count = In.RepeatCount;
					Internal.SrcReg1 = In.StartSrcReg1;
					Internal.SrcReg2 = In.StartSrcReg2;
					Internal.DstReg = In.StartDstReg;
					Internal.SrcReg1Mem = In.Reg1Source;
					Internal.SrcReg2Mem = In.Reg2Source;
					Internal.SrcReg1Stride = In.SrcReg1Stride;
					Internal.SrcReg2Stride = In.SrcReg2Stride;
					Internal.DstRegStride = In.DstRegStride;
					Internal.First = true;
					Internal.Opcode = In.Opcode;

					if (In.RepeatCount == 0)
						Internal.Ready = true;
				}

				if (Internal.Count == 1 || Internal.Ready)
					Out.Ready = true;

				if (Internal.Count != 0)
				{
					Out.SrcReg1 = Internal.SrcReg1;
					Out.SrcReg2 = Internal.SrcReg2;
					Out.DstReg = Internal.DstReg;
					Out.Opcode = ConvertOpToAlu(Internal.Opcode);
					Out.DstValid = true;

					Out.Reg1Source = Internal.SrcReg1Mem ? RegisterOutputMultiplexer.SOURCE_MEM : RegisterOutputMultiplexer.SOURCE_REG;
					Out.Reg2Source = Internal.SrcReg2Mem ? RegisterOutputMultiplexer.SOURCE_MEM : RegisterOutputMultiplexer.SOURCE_REG;

					Internal.Count--;
					Internal.SrcReg1 = (RegisterAddress)(Internal.SrcReg1 + (int)Internal.SrcReg1Stride);
					Internal.SrcReg2 = (RegisterAddress)(Internal.SrcReg2 + (int)Internal.SrcReg2Stride);
					Internal.DstReg = (RegisterAddress)(Internal.DstReg + (int)Internal.DstRegStride);

					// Override if we are doing a reduction
					if (IsReduction(Internal.Opcode))
					{
						if (Internal.First)
						{
							Out.Opcode = AluOpcode.CopyA;
							Internal.SrcReg2 = (RegisterAddress)(Internal.SrcReg1 + (int)Internal.SrcReg1Stride);
						}
						else
							Out.Reg1Source = RegisterOutputMultiplexer.SOURCE_ALU;
					}
					else if (Internal.Opcode == Opcode.Range)
					{
						Out.Reg1Source = RegisterOutputMultiplexer.SOURCE_ALU;
						if (Internal.First)
							Out.Opcode = AluOpcode.Zero;
					}

				}
			}
		}
	}
}

