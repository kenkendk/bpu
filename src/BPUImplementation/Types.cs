using System;
using SME;
using SME.Render.VHDL;
using SME.Render;

using MEMADRT = System.UInt32;
using REGADRT = System.UInt16;
using COUNTERT = System.UInt32;
using DATAT = System.UInt32;
using OFFSETT = System.Byte;
using System.Diagnostics;

namespace BPUImplementation
{
	public enum Opcode
	{
		Noop,
		Add,
		Sub,
		Mul,
		//Div,
		Max,
		Min,
		AddReduce,
		SubReduce,
		MulReduce,
		DivReduce,
		MaxReduce,
		MinReduce,
		BitwiseAnd,
		BitwiseOr,
		BitwiseXor,
		LogicalAnd,
		LogicalOr,
		LogicalXor,
		BitwiseAndReduce,
		BitwiseOrReduce,
		BitwiseXorReduce,
		LogicalAndReduce,
		LogicalOrReduce,
		LogicalXorReduce,
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanEqual,
		LessThan,
		LessThanEqual,
		Range
	}

	public enum AluOpcode
	{
		Noop,
		Add,
		Sub,
		Mul,
		//Div,
		Max,
		Min,
		CopyA,
		CopyB,
		IncrA,
		DecrA,
		Zero,
		One,
		ShiftLeft,
		ShiftRight,
		ArithmeticShiftLeft,
		ArithmeticShiftRight,
		RotateLeft,
		RotateRight,
		BitwiseOr,
		BitwiseAnd,
		BitwiseXor,
		BitwiseNot,
		LogicalOr,
		LogicalAnd,
		LogicalXor,
		LogicalNot,
		Equal,
		NotEqual,
		GreaterThan,
		GreaterThanEqual,
		LessThan,
		LessThanEqual,
	}

	[DebuggerDisplay("{Value}")]
	[VHDLType("STD_LOGIC_VECTOR(15 downto 0)", "T_MEMADR")]
	public struct MemoryAddress : ICSVSerializable {
		private readonly MEMADRT Value;

		private const int ADDR_WIDTH = 16;

		public MemoryAddress(MEMADRT v)
		{
			this.Value = v;
		}

		public static implicit operator MemoryAddress(MEMADRT v)
		{
			return new MemoryAddress(v);
		}

		public static implicit operator MEMADRT(MemoryAddress v)
		{
			return v.Value;
		}

		public static MemoryAddress operator++(MemoryAddress v) 
		{
			return new MemoryAddress((MEMADRT)(v + 1));
		}

		public static MemoryAddress operator--(MemoryAddress v) 
		{
			return new MemoryAddress((MEMADRT)(v - 1));
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		string ICSVSerializable.Serialize()
		{
			return Convert.ToString(this.Value, 2).PadLeft(ADDR_WIDTH, '0');
		}
	};

	[DebuggerDisplay("{Value}")]
	[VHDLType("STD_LOGIC_VECTOR(9 downto 0)", "T_REGNO")]
	public struct RegisterAddress : ICSVSerializable {
		private REGADRT Value;

		private const int REGNO_WIDTH = 10;

		public RegisterAddress(REGADRT v)
		{
			this.Value = v;
		}

		public static implicit operator RegisterAddress(REGADRT v)
		{
			return new RegisterAddress(v);
		}

		public static implicit operator REGADRT(RegisterAddress v)
		{
			return v.Value;
		}

		public static RegisterAddress operator++(RegisterAddress v) 
		{
			return new RegisterAddress((REGADRT)(v + 1));
		}

		public static RegisterAddress operator--(RegisterAddress v) 
		{
			return new RegisterAddress((REGADRT)(v - 1));
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		string ICSVSerializable.Serialize()
		{
			return Convert.ToString(this.Value, 2).PadLeft(REGNO_WIDTH, '0');
		}
	};

	[DebuggerDisplay("{Value}")]
	[VHDLType("STD_LOGIC_VECTOR(9 downto 0)", "T_REPCNT")]
	public struct Counter : ICSVSerializable  {
		private COUNTERT Value;

		private const int REGNO_WIDTH = 10;

		public Counter(COUNTERT v)
		{
			this.Value = v;
		}

		public static implicit operator Counter(COUNTERT v)
		{
			return new Counter(v);
		}

		public static implicit operator COUNTERT(Counter v)
		{
			return v.Value;
		}

		public static explicit operator Counter(int v)
		{
			return new Counter((COUNTERT)v);
		}

		public static explicit operator int(Counter v)
		{
			return (int)v.Value;
		}

		public static Counter operator++(Counter v) 
		{
			return new Counter((COUNTERT)(v + 1));
		}

		public static Counter operator--(Counter v) 
		{
			return new Counter((COUNTERT)(v - 1));
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		string ICSVSerializable.Serialize()
		{
			return Convert.ToString(this.Value, 2).PadLeft(REGNO_WIDTH, '0');
		}
	};

	[DebuggerDisplay("{Value}")]
	[VHDLType("STD_LOGIC_VECTOR(3 downto 0)", "T_OFFSETCNT")]
	public struct OffsetCounter : ICSVSerializable  {
		private OFFSETT Value;

		private const int REGNO_WIDTH = 4;

		public OffsetCounter(OFFSETT v)
		{
			this.Value = v;
		}

		public static implicit operator OffsetCounter(OFFSETT v)
		{
			return new OffsetCounter(v);
		}

		public static implicit operator OFFSETT(OffsetCounter v)
		{
			return v.Value;
		}

		public static implicit operator int(OffsetCounter v)
		{
			return v.Value;
		}

		public static implicit operator OffsetCounter(int v)
		{
			return new OffsetCounter((OFFSETT)v);
		}
			
		public static OffsetCounter operator++(OffsetCounter v) 
		{
			return new OffsetCounter((OFFSETT)(v + 1));
		}

		public static OffsetCounter operator--(OffsetCounter v) 
		{
			return new OffsetCounter((OFFSETT)(v - 1));
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		string ICSVSerializable.Serialize()
		{
			return Convert.ToString(this.Value, 2).PadLeft(REGNO_WIDTH, '0');
		}
	};
	[DebuggerDisplay("{Value}")]
	[VHDLType("STD_LOGIC_VECTOR(15 downto 0)", "T_DATA")]
	public struct RegisterData : ICSVSerializable {
		public const int DATA_LENGTH = (DATA_WIDTH + 7) / 8;

		private const int DATA_WIDTH = 16;

		private readonly DATAT Value;

		public RegisterData(DATAT v)
		{
			this.Value = v;
		}

		public static implicit operator RegisterData(DATAT v)
		{
			return new RegisterData(v);
		}

		public static implicit operator DATAT(RegisterData v)
		{
			return v.Value;
		}
			
		public override string ToString()
		{
			return Value.ToString();
		}

		string ICSVSerializable.Serialize()
		{
			return Convert.ToString(this.Value, 2).PadLeft(DATA_WIDTH, '0');
		}
	};
}

