using System;
using System.Runtime.InteropServices;

using bh_pointer_t = System.UInt64;
using bh_index_t = System.Int64;
using bh_intp_t = System.UInt64;

using bh_bool_t = System.Boolean;
using bh_int8_t = System.SByte;
using bh_int16_t = System.Int16;
using bh_int32_t = System.Int32;
using bh_int64_t = System.Int64;
using bh_uint8_t = System.Byte;
using bh_uint16_t = System.UInt16;
using bh_uint32_t = System.UInt32;
using bh_uint64_t = System.UInt64;
using bh_float32_t = System.Single;
using bh_float64_t = System.Double;
using bh_complex128_t = System.Numerics.Complex;
using bh_r123_t = bpusmecompiler.bh_r123;

namespace bpusmecompiler
{
	public struct Base
	{
		public bh_pointer_t Data;
		public bh_type Type;
		public bh_index_t NElement;
	}

	public struct View
	{
		public Base? Base;
		public ulong BasePointer;
		public bh_index_t Start;
		public bh_intp_t ndim;
		public bh_index_t[] Shape;
		public bh_index_t[] Stride;
	}

	public struct bh_r123 
	{ 
		public bh_uint64_t start;
		public bh_uint64_t key; 
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ConstantValue
	{
		[FieldOffset(0)]
		public bh_bool_t       bool8;
		[FieldOffset(0)]
		public bh_int8_t       int8;
		[FieldOffset(0)]
		public bh_int16_t      int16;
		[FieldOffset(0)]
		public bh_int32_t      int32;
		[FieldOffset(0)]
		public bh_int64_t      int64;
		[FieldOffset(0)]
		public bh_uint8_t      uint8;
		[FieldOffset(0)]
		public bh_uint16_t     uint16;
		[FieldOffset(0)]
		public bh_uint32_t     uint32;
		[FieldOffset(0)]
		public bh_uint64_t     uint64;
		[FieldOffset(0)]
		public bh_float32_t    float32;
		[FieldOffset(0)]
		public bh_float64_t    float64;
		//[FieldOffset(0)]
		//public bh_complex64_t  complex64;
		[FieldOffset(0)]
		public bh_complex128_t complex128;
		[FieldOffset(0)]
		public bh_r123_t       r123;
	}

	public enum bh_type
	{
		BH_BOOL,
		BH_INT8,
		BH_INT16,
		BH_INT32,
		BH_INT64,
		BH_UINT8,
		BH_UINT16,
		BH_UINT32,
		BH_UINT64,
		BH_FLOAT32,
		BH_FLOAT64,
		BH_COMPLEX64,
		BH_COMPLEX128,
		BH_R123,
		BH_UNKNOWN
	};

	public struct Constant
	{
		public ConstantValue Value;
		public bh_type Type;
	}

	public struct Instruction
	{
		public bh_opcode Opcode;
		public View[] Operands;
		public Constant Constant;
	}

	public enum bh_opcode
	{
		BH_ADD = 1,		// Add arguments element-wise.
		BH_SUBTRACT = 2,		// Subtract arguments, element-wise.
		BH_MULTIPLY = 3,		// Multiply arguments element-wise.
		BH_DIVIDE = 4,		// Divide arguments element-wise.
		BH_POWER = 5,		// First array elements raised to powers from second array, element-wise.
		BH_ABSOLUTE = 6,		// Calculate the absolute value element-wise.
		BH_GREATER = 7,		// Return the truth value of (x1 > x2) element-wise.
		BH_GREATER_EQUAL = 8,		// Return the truth value of (x1 >= x2) element-wise.
		BH_LESS = 9,		// Return the truth value of (x1 < x2) element-wise.
		BH_LESS_EQUAL = 10,		// Return the truth value of (x1 =< x2) element-wise.
		BH_EQUAL = 11,		// Return (x1 == x2) element-wise.
		BH_NOT_EQUAL = 12,		// Return (x1 != x2) element-wise.
		BH_LOGICAL_AND = 13,		// Compute the truth value of x1 AND x2 elementwise.
		BH_LOGICAL_OR = 14,		// Compute the truth value of x1 OR x2 elementwise.
		BH_LOGICAL_XOR = 15,		// Compute the truth value of x1 XOR x2, element-wise.
		BH_LOGICAL_NOT = 16,		// Compute the truth value of NOT x elementwise.
		BH_MAXIMUM = 17,		// Element-wise maximum of array elements.
		BH_MINIMUM = 18,		// Element-wise minimum of array elements.
		BH_BITWISE_AND = 19,		// Compute the bit-wise AND of two arrays element-wise.
		BH_BITWISE_OR = 20,		// Compute the bit-wise OR of two arrays element-wise.
		BH_BITWISE_XOR = 21,		// Compute the bit-wise XOR of two arrays element-wise.
		BH_INVERT = 22,		// Compute bit-wise inversion, or bit-wise NOT, element-wise.
		BH_LEFT_SHIFT = 23,		// Shift the bits of an integer to the left.
		BH_RIGHT_SHIFT = 24,		// Shift the bits of an integer to the right.
		BH_COS = 25,		// Cosine elementwise.
		BH_SIN = 26,		// Trigonometric sine, element-wise.
		BH_TAN = 27,		// Compute tangent element-wise.
		BH_COSH = 28,		// Hyperbolic cosine, element-wise.
		BH_SINH = 29,		// Hyperbolic sine, element-wise.
		BH_TANH = 30,		// Compute hyperbolic tangent element-wise.
		BH_ARCSIN = 31,		// Inverse sine, element-wise.
		BH_ARCCOS = 32,		// Trigonometric inverse cosine, element-wise.
		BH_ARCTAN = 33,		// Trigonometric inverse tangent, element-wise.
		BH_ARCSINH = 34,		// Inverse hyperbolic sine elementwise.
		BH_ARCCOSH = 35,		// Inverse hyperbolic cosine, elementwise.
		BH_ARCTANH = 36,		// Inverse hyperbolic tangent elementwise.
		BH_ARCTAN2 = 37,		// Element-wise arc tangent of ``x1/x2`` choosing the quadrant correctly.
		BH_EXP = 38,		// Calculate the exponential of all elements in the input array.
		BH_EXP2 = 39,		// Calculate `2**p` for all `p` in the input array.
		BH_EXPM1 = 40,		// Calculate ``exp(x) - 1`` for all elements in the array.
		BH_LOG = 41,		// Natural logarithm, element-wise.
		BH_LOG2 = 42,		// Base-2 logarithm of `x`.
		BH_LOG10 = 43,		// Return the base 10 logarithm of the input array, element-wise.
		BH_LOG1P = 44,		// Return the natural logarithm of one plus the input array, element-wise.
		BH_SQRT = 45,		// Return the positive square-root of an array, element-wise.
		BH_CEIL = 46,		// Return the ceiling of the input, element-wise.
		BH_TRUNC = 47,		// Return the truncated value of the input, element-wise.
		BH_FLOOR = 48,		// Return the floor of the input, element-wise.
		BH_RINT = 49,		// Round elements of the array to the nearest integer.
		BH_MOD = 50,		// Return the element-wise remainder of division.
		BH_ISNAN = 51,		// Test for NaN values.
		BH_ISINF = 52,		// Test for infinity values.
		BH_IDENTITY = 53,		// The identity function that returns the input value converted to the output data type.
		BH_DISCARD = 54,		// System instruction that informs the child component to forget the array and release any metadata allocated.
		BH_FREE = 55,		// System instruction that informs the child component to deallocate the data storage associated with the array.
		BH_SYNC = 56,		// System instruction that informs the child component to make data synchronized and available.
		BH_NONE = 57,		// A opcode that should be ignored.
		BH_TALLY = 58,		// System instruction that informs the child component to tally operations.
		BH_ADD_REDUCE = 59,		// Sums all elements in the specified dimension.
		BH_MULTIPLY_REDUCE = 60,		// Multiplies all elements in the specified dimension.
		BH_MINIMUM_REDUCE = 61,		// Finds the smallest elements in the specified dimension.
		BH_MAXIMUM_REDUCE = 62,		// Finds the largest elements in the specified dimension.
		BH_LOGICAL_AND_REDUCE = 63,		// Logical AND of all elements in the specified dimension.
		BH_BITWISE_AND_REDUCE = 64,		// Bitwise AND of all elements in the specified dimension.
		BH_LOGICAL_OR_REDUCE = 65,		// Logical OR of all elements in the specified dimension.
		BH_BITWISE_OR_REDUCE = 66,		// Bitwise OR of all elements in the specified dimension.
		BH_LOGICAL_XOR_REDUCE = 67,		// Logical XOR of all elements in the specified dimension.
		BH_BITWISE_XOR_REDUCE = 68,		// Bitwise XOR of all elements in the specified dimension.
		BH_RANDOM = 69,		// Random123: The returned result is a deterministic function of the key and counter, i.e. a unique (seed, indexes) tuple will always produce the same result. The result is highly sensitive to small changes in the inputs, so that the sequence of values produced by simply incrementing the counter (or key) is effectively indistinguishable from a sequence of samples of a uniformly distributed random variable.
		BH_RANGE = 70,		// Returns a 1-dim base-array filled with integer range starting a zero
		BH_REAL = 71,		// Return the real part of the elements of the array.
		BH_IMAG = 72,		// Return the imaginary part of the elements of the array.
		BH_ADD_ACCUMULATE = 73,		// Computes the prefix sum.
		BH_MULTIPLY_ACCUMULATE = 74,		// Computes the prefix product.
		BH_SIGN = 75,		// Computes the SIGN of elements. -1 = negative, 1=positive. 0 = 0.
		BH_MATMUL = 76,		// Matrix multiplication C = A x B
		BH_GATHER = 77,		// Gather elements from IN selected by INDEX into OUT. IN.shape == INDEX.shape.
		BH_SCATTER = 78,		// Scatter all elements of IN into OUT selected by INDEX. OUT.shape == INDEX.shape.

		BH_NO_OPCODES = 78, // The amount of opcodes
		BH_MAX_OPCODE_ID = 78   // The extension method offset
	};
}

