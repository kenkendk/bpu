using System;
using System.Collections.Generic;
using System.Linq;

using TDATA = System.Int16;

namespace bpusmecompiler
{
	public static class Compiler
	{
		public const uint REGISTER_SIZE = (1024 * 64) / BPUImplementation.RegisterData.DATA_LENGTH;
		public const int DATA_EL_SIZE = BPUImplementation.RegisterData.DATA_LENGTH; // In bytes

		public class CompilerState
		{
			public List<KeyValuePair<ulong, ulong>> pendingread = new List<KeyValuePair<ulong, ulong>>();
			public List<KeyValuePair<ulong, ulong>> pendingwrite = new List<KeyValuePair<ulong, ulong>>();
			public List<Tuple<ulong, ulong, ulong>> registerfilemap = new List<Tuple<ulong, ulong, ulong>>();
			public List<BPUImplementation.Microcode> code = new List<BPUImplementation.Microcode>();
			public List<TDATA> constantmap = new List<TDATA>();

			public StorageManager regspace = new StorageManager(REGISTER_SIZE);
			public bool needswrwait = false;
			public bool needsexwait = false;
			public bool needsrdwait { get { return pendingread.Count > 0; } }
			public Dictionary<ulong, ulong> map = new Dictionary<ulong, ulong>();

			public void HasWaitedForWrites()
			{
				if (!needswrwait)
					return;

				foreach (var op in pendingwrite)
					regspace.Free(op.Key);

				pendingwrite.Clear();
			}
		}

		private static TDATA GetConstantValue(Instruction src)
		{
			TDATA val;

			switch (src.Constant.Type)
			{
				case bh_type.BH_FLOAT32:
					val = (TDATA)src.Constant.Value.float32;
					break;
				case bh_type.BH_FLOAT64:
					val = (TDATA)src.Constant.Value.float64;
					break;
				case bh_type.BH_UINT8:
					val = (TDATA)src.Constant.Value.uint8;
					break;
				case bh_type.BH_UINT16:
					val = (TDATA)src.Constant.Value.uint16;
					break;
				case bh_type.BH_UINT32:
					val = (TDATA)src.Constant.Value.uint32;
					break;
				case bh_type.BH_UINT64:
					val = (TDATA)src.Constant.Value.uint64;
					break;
				default:
					throw new Exception(string.Format("Unsupported input type: {0}", src.Constant.Type));
			}

			return val;
		}
		
		public static CompilerState Compile(Instruction[] instructions, Dictionary<ulong, ulong> map)
		{
			var cs = new CompilerState() { map = map };

			// Pre-build constant map
			foreach (var src in instructions)
				foreach (var op in src.Operands)
					if (op.Base == null)
						cs.constantmap.Add(GetConstantValue(src));
				
			cs.constantmap = cs.constantmap.Distinct().ToList();
			// If we have constants/literals, make sure we have reserved space for it in the register file
			if (cs.constantmap.Count != 0)
				cs.regspace.Allocate((ulong)cs.constantmap.Count);

			foreach (var src in instructions)
			{
				if (src.Opcode == bh_opcode.BH_FREE)
					continue;

				// Simple first version: Load from memory into registers
				// Then execute
				// Then load from register to memory
				// Rinse and repeat

				var sourceRegs = new List<ulong>();

				// Copy to registers
				foreach (var op in src.Operands.Skip(1))
					sourceRegs.Add(AllocateRegisterForOperand(cs, src, op, false));

				//Execute
				var srcA = (ushort)(sourceRegs.Count > 0 ? sourceRegs.First() : 0);
				var srcB = (ushort)(sourceRegs.Count > 1 ? sourceRegs.Skip(1).First() : 0);

				BPUImplementation.Opcode opcode;
				switch (src.Opcode)
				{
					case bh_opcode.BH_IDENTITY:
						opcode = BPUImplementation.Opcode.BitwiseOr;
						srcB = srcA;
						break;
					case bh_opcode.BH_ADD:
						opcode = BPUImplementation.Opcode.Add;
						break;
					case bh_opcode.BH_MULTIPLY:
						opcode = BPUImplementation.Opcode.Mul;
						break;
					case bh_opcode.BH_GREATER_EQUAL:
						opcode = BPUImplementation.Opcode.GreaterThanEqual;
						break;
					case bh_opcode.BH_LESS_EQUAL:
						opcode = BPUImplementation.Opcode.LessThanEqual;
						break;
					case bh_opcode.BH_BITWISE_AND:
						opcode = BPUImplementation.Opcode.BitwiseAnd;
						break;
					case bh_opcode.BH_BITWISE_OR:
						opcode = BPUImplementation.Opcode.BitwiseOr;
						break;
					case bh_opcode.BH_BITWISE_XOR:
						opcode = BPUImplementation.Opcode.BitwiseXor;
						break;
					case bh_opcode.BH_EQUAL:
						opcode = BPUImplementation.Opcode.Equal;
						break;
					case bh_opcode.BH_SYNC:
					case bh_opcode.BH_DISCARD:
						opcode = BPUImplementation.Opcode.Noop;
						break;
					default:
						throw new Exception(string.Format("Unsupported opcode: {0}", src.Opcode));
				}

				var dstop = src.Operands.First();
				var dst = (ushort)AllocateRegisterForOperand(cs, src, dstop, src.Opcode != bh_opcode.BH_SYNC);
				var count = dstop.Shape.Take((int)dstop.ndim).Aggregate(1L, (x, y) => x * y);

				if (src.Opcode == bh_opcode.BH_SYNC)
				{
					if (dstop.Base == null)
						throw new Exception("Base not allocated?");

					if (dstop.Base.Value.Data == 0)
						throw new Exception("Target memory not allocated?");

					var size = (uint)dstop.Base.Value.NElement;
					var memdst = (uint)cs.map[dstop.BasePointer];

					// Write back to memory
					EmitInstruction(cs,
						new BPUImplementation.Microcode(
							false, cs.needswrwait, cs.needsexwait,
							true, size, memdst, 1, dst, 1,
							false, 0, 0, 0, 0, 0,
							false, BPUImplementation.Opcode.Noop, 0, 0, 0, 0, 0, 0, 0, false, false, (ushort)(cs.code.Count + 1)
						)
					);

					// Free up unused space
					cs.HasWaitedForWrites();

					cs.needsexwait = false;
					cs.needswrwait = true;
					cs.pendingwrite.Add(new KeyValuePair<ulong, ulong>());
				}
				else if (src.Opcode == bh_opcode.BH_DISCARD)
				{
					cs.regspace.Free(dst);
				}
				else
				{
					var stridea = sourceRegs.Count > 0 && src.Operands[1].Base == null ? 0 : 1;
					var strideb = sourceRegs.Count > 1 && src.Operands[2].Base == null ? 0 : 1;

					// TODO: Need to keep track of memory, to see if it is in the memory block, or the register block

					// Execute
					EmitInstruction(cs,
						new BPUImplementation.Microcode(
							cs.needsrdwait, false, cs.needsexwait,
							false, 0, 0, 0, 0, 0,
							false, 0, 0, 0, 0, 0,
							true, opcode, srcA, srcB, dst, (uint)count, stridea, strideb, 1, true, true, (ushort)(cs.code.Count + 1)
						)
					);

					// We have awaited reads and writes, so clean up
					cs.needsexwait = true;
					cs.pendingread.Clear();

				}


			}

			// Tag on the exit code
			EmitInstruction(cs,
				new BPUImplementation.Microcode(
					cs.needsrdwait, cs.needswrwait, cs.needsexwait,
					false, 0, 0, 0, 0, 0,
					false, 0, 0, 0, 0, 0,
					true, BPUImplementation.Opcode.Noop, 0, 0, 0, 0, 0, 0, 0, false, false, (ushort)(BPUImplementation.Microcode.MAX_INSTRUCTION_COUNT - 1)
				)
			);

			return cs;
		}

		private static void EmitInstruction(CompilerState cs, BPUImplementation.Microcode instr)
		{
			// TODO: Each call to cs.code.Add should check if the previous instruction can be merged with the new
			// to have as few instructions as possible

			cs.code.Add(instr);
		}

		private static ulong AllocateRegisterForOperand(CompilerState cs, Instruction instr, View op, bool isTarget)
		{
			if (op.BasePointer == 0)
				return (ulong)cs.constantmap.IndexOf(GetConstantValue(instr));

			var prealloc = cs.registerfilemap.Where(x => x.Item1 == op.BasePointer).FirstOrDefault();
			if (prealloc != null)
				return prealloc.Item2;

			var bs = op.Base.Value;

			// TODO: use only the subset accessed by the joint span of the view!
			var memsize = (ulong)bs.NElement;

			if (bs.Data == 0)
			{
				if (!isTarget)
					throw new Exception("Storage is not allocated");

				// Allocate data in register file only, no pending read

				// Grab space in register file
				var regp = cs.regspace.Allocate(memsize);

				cs.registerfilemap.Add(new Tuple<ulong, ulong, ulong>(op.BasePointer, regp, memsize));
				return regp;
			}


			var mappedaddr = cs.map[op.BasePointer];

			// Grab space in register file
			var regoffset = cs.regspace.Allocate(memsize);

			// Record that we have memory in register
			cs.registerfilemap.Add(new Tuple<ulong, ulong, ulong>(op.BasePointer, regoffset, memsize));
			cs.pendingread.Add(new KeyValuePair<ulong, ulong>(op.BasePointer, regoffset));

			// TODO: Wait logic should handle cases where the read and write are independent

			// Emit copy
			EmitInstruction(cs,
				new BPUImplementation.Microcode(
					false, cs.needswrwait, false, 
					false, 0, 0, 0, 0, 0,
					true, (uint)memsize, (uint)mappedaddr, 1, (ushort)regoffset, 1,
					false, BPUImplementation.Opcode.Noop, 0, 0, 0, 0, 0, 0, 0, false, false, (ushort)(cs.code.Count + 1)
				)
			);

			cs.needswrwait = false;

			return regoffset;
		}
	}
}

