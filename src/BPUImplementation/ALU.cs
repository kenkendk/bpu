using System;
using SME;
using System.Threading.Tasks;

namespace BPUImplementation
{
	public class ALU : SimpleProcess
	{
		public interface IInput : IBus
		{
			[InitialValue(AluOpcode.Noop)]
			AluOpcode Opcode { get; set; }
			[InitialValue]
			bool Valid { get; set; }
			RegisterData Op1 { get; set; }
			RegisterData Op2 { get; set; }
			[InitialValue]
			RegisterAddress Dst { get; set; }
		}

		public interface IOutput : IBus
		{
			[InitialValue(false)]
			bool Valid { get; set; }
			RegisterData Result { get; set; }
			RegisterAddress DstReg { get; set; }
		}
			
		[InputBus]
		IInput In;

		[OutputBus]
		IOutput Out;

		public static readonly RegisterData TRUE = 1;
		public static readonly RegisterData FALSE = 0;

		private RegisterData ShiftLeft(RegisterData value, RegisterData shiftvalue)
		{
			return (uint)value << (int)(uint)shiftvalue;
		}

		private RegisterData ShiftRight(RegisterData value, RegisterData shiftvalue)
		{
			return (uint)value >> (int)(uint)shiftvalue;
		}

		private RegisterData ArithmeticShiftLeft(RegisterData value, RegisterData shiftvalue)
		{
			/*var mask = (uint)(1 << ((RegisterData.DATA_LENGTH * 8) - 1));
			var topbit = value & mask;
			return  topbit | ((value << (int)(uint)shiftvalue) & (mask - 1));*/
			return (uint)value << (int)(uint)shiftvalue;
		}

		private RegisterData ArithmeticShiftRight(RegisterData value, RegisterData shiftvalue)
		{
			var mask = (uint)(1 << ((RegisterData.DATA_LENGTH * 8) - 1));
			var topbit = value & mask;
			mask = (uint) ~(0xFFFF >> (int)(uint)shiftvalue);

			if (topbit == 0)
				return (uint)value >> (int)(uint)shiftvalue;
			else
				return mask | (((uint)value >> (int)(uint)shiftvalue) & ~mask);
		}

		private RegisterData RotateLeft(RegisterData value, RegisterData shiftvalue)
		{
			return ((uint)value << (int)(uint)shiftvalue) | ((uint)value >> (32 - (int)(uint)shiftvalue));
		}

		private RegisterData RotateRight(RegisterData value, RegisterData shiftvalue)
		{
			return ((uint)value >> (int)(uint)shiftvalue) | ((uint)value << (32 - (int)(uint)shiftvalue));
		}


		protected override void OnTick()
		{
			//DebugOutput = true;
			Out.Valid = In.Valid;
			Out.DstReg = In.Dst;

			var op1 = In.Op1;
			var op2 = In.Op2;

			RegisterData res;

			var logicalOp1 = op1 != 0;
			var logicalOp2 = op2 != 0;

			switch(In.Opcode)
			{
				case AluOpcode.Add:
					res = op1 + op2;
					break;
				case AluOpcode.Sub:
					res = op1 - op2;
					break;
				case AluOpcode.Mul:
					res = op1 * op2;
					break;
				/*case AluOpcode.Div:
					res = op1 / op2;
					break;*/
				case AluOpcode.Min:
					res = op1 < op2 ? op1 : op2;
					break;
				case AluOpcode.Max:
					res = op1 >= op2 ? op1 : op2;
					break;
				case AluOpcode.CopyA:
					res = op1;
					break;
				case AluOpcode.CopyB:
					res = op2;
					break;
				case AluOpcode.IncrA:
					res = (RegisterData)(op1 + 1);
					break;
				case AluOpcode.DecrA:
					res = (RegisterData)(op1 - 1);
					break;
				case AluOpcode.One:
					res = 1;
					break;
				case AluOpcode.Zero:
					res = 0;
					break;
				case AluOpcode.ShiftLeft:
					res = ShiftLeft(op1, op2);
					break;
				case AluOpcode.ShiftRight:
					res = ShiftRight(op1, op2);
					break;
				case AluOpcode.ArithmeticShiftLeft:
					res = ArithmeticShiftLeft(op1, op2);
					break;
				case AluOpcode.ArithmeticShiftRight:
					res = ArithmeticShiftRight(op1, op2);
					break;
				case AluOpcode.RotateLeft:
					res = RotateLeft(op1, op2);
					break;
				case AluOpcode.RotateRight:
					res = RotateRight(op1, op2);
					break;
				case AluOpcode.BitwiseOr:
					res = op1 | op2;
					break;
				case AluOpcode.BitwiseAnd:
					res = op1 & op2;
					break;
				case AluOpcode.BitwiseXor:
					res = op1 ^ op2;
					break;
				case AluOpcode.BitwiseNot:
					res = ~op1;
					break;
				case AluOpcode.LogicalOr:
					res = logicalOp1 || logicalOp2 ? TRUE : FALSE;
					break;
				case AluOpcode.LogicalAnd:
					res = logicalOp1 && logicalOp2 ? TRUE : FALSE;
					break;
				case AluOpcode.LogicalXor:
					res = logicalOp1 ^ logicalOp2 ? TRUE : FALSE;
					break;
				case AluOpcode.LogicalNot:
					res = logicalOp1 ? FALSE : TRUE;
					break;
				case AluOpcode.GreaterThan:
					res = op1 > op2 ? TRUE : FALSE;
					break;
				case AluOpcode.GreaterThanEqual:
					res = op1 >= op2 ? TRUE : FALSE;
					break;
				case AluOpcode.LessThan:
					res = op1 < op2 ? TRUE : FALSE;
					break;
				case AluOpcode.LessThanEqual:
					res = op1 <= op2 ? TRUE : FALSE;
					break;
				case AluOpcode.Equal:
					res = op1 == op2 ? TRUE : FALSE;
					break;
				case AluOpcode.NotEqual:
					res = op1 != op2 ? TRUE : FALSE;
					break;
				default:
					res = 0;
					Out.Valid = false;
					Out.DstReg = 0;
					break;
			}

			Out.Result = res;

			if (In.Valid)
				PrintDebug("ALU execute: {0}({1} + {2}) = {3}", In.Opcode, (uint)op1, (uint)op2, res);
			
		}
	}
}

