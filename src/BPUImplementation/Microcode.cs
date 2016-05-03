using System;
using SME;
using SME.Render.VHDL;

namespace BPUImplementation
{
	public struct Microcode
	{
		public static readonly ushort MAX_INSTRUCTION_COUNT = 99;

		public bool rd_wait;
		public bool wr_wait;
		public bool ex_wait;

		public bool wr_mem_valid;
		public Counter wr_mem_cnt;
		public MemoryAddress wr_mem_adr;
		public RegisterAddress wr_mem_reg;
		public OffsetCounter wr_mem_adr_stride;
		public OffsetCounter wr_mem_reg_stride;

		public bool rd_mem_valid;
		public Counter rd_mem_cnt;
		public MemoryAddress rd_mem_adr;
		public RegisterAddress rd_mem_reg;
		public OffsetCounter rd_mem_adr_stride;
		public OffsetCounter rd_mem_reg_stride;

		public bool ex_valid;
		public Opcode ex_opcode;
		public RegisterAddress ex_src_reg1;
		public RegisterAddress ex_src_reg2;
		public RegisterAddress ex_dst_reg;
		public OffsetCounter ex_src_reg1_stride;
		public OffsetCounter ex_src_reg2_stride;
		public OffsetCounter ex_dst_reg_stride;
		public Counter ex_cnt;
		public bool ex_src1_mem;
		public bool ex_src2_mem;

		public ushort nextpc;

		public Microcode(
			bool rd_wait, bool wr_wait, bool ex_wait,
			bool wr_mem_valid, Counter wr_mem_cnt, MemoryAddress wr_mem_adr, OffsetCounter wr_mem_adr_stride, RegisterAddress wr_mem_reg, OffsetCounter wr_mem_reg_stride,
			bool rd_mem_valid, Counter rd_mem_cnt, MemoryAddress rd_mem_adr, OffsetCounter rd_mem_adr_stride, RegisterAddress rd_mem_reg, OffsetCounter rd_mem_reg_stride,
			bool ex_valid, Opcode ex_opcode, RegisterAddress ex_src_reg1, RegisterAddress ex_src_reg2, RegisterAddress ex_dst_reg, Counter ex_cnt, OffsetCounter ex_src_reg1_stride, OffsetCounter ex_src_reg2_stride, OffsetCounter ex_dst_reg_stride, bool ex_src1_mem, bool ex_src2_mem,
			ushort next)
		{
			this.rd_wait = rd_wait;
			this.wr_wait = wr_wait;
			this.ex_wait = ex_wait;

			this.wr_mem_valid = wr_mem_valid;
			this.wr_mem_cnt = wr_mem_cnt;
			this.wr_mem_adr = wr_mem_adr;
			this.wr_mem_adr_stride = wr_mem_adr_stride;
			this.wr_mem_reg = wr_mem_reg;
			this.wr_mem_reg_stride = wr_mem_reg_stride;

			this.rd_mem_valid = rd_mem_valid;
			this.rd_mem_cnt = rd_mem_cnt;
			this.rd_mem_adr = rd_mem_adr;
			this.rd_mem_adr_stride = rd_mem_adr_stride;
			this.rd_mem_reg = rd_mem_reg;
			this.rd_mem_reg_stride = rd_mem_reg_stride;

			this.ex_valid = ex_valid;
			this.ex_opcode = ex_opcode;
			this.ex_src_reg1 = ex_src_reg1;
			this.ex_src_reg2 = ex_src_reg2;
			this.ex_dst_reg = ex_dst_reg;
			this.ex_cnt = ex_cnt;
			this.ex_src_reg1_stride = ex_src_reg1_stride;
			this.ex_src_reg2_stride = ex_src_reg2_stride;
			this.ex_dst_reg_stride = ex_dst_reg_stride;
			this.ex_src1_mem = ex_src1_mem;
			this.ex_src2_mem = ex_src2_mem;

			this.nextpc = next;
		}

		public Microcode(
			int rd_wait, int wr_wait, int ex_wait,
			int wr_mem_valid, Counter wr_mem_cnt, MemoryAddress wr_mem_adr, OffsetCounter wr_mem_adr_stride, RegisterAddress wr_mem_reg, OffsetCounter wr_mem_reg_stride,
			int rd_mem_valid, Counter rd_mem_cnt, MemoryAddress rd_mem_adr, OffsetCounter rd_mem_adr_stride, RegisterAddress rd_mem_reg, OffsetCounter rd_mem_reg_stride,
			int ex_valid, Opcode ex_opcode, RegisterAddress ex_src_reg1, RegisterAddress ex_src_reg2, RegisterAddress ex_dst_reg, Counter ex_cnt, OffsetCounter ex_src_reg1_stride, OffsetCounter ex_src_reg2_stride, OffsetCounter ex_dst_reg_stride, int ex_src1_mem, int ex_src2_mem,
			ushort next)
		{
			this.rd_wait = rd_wait != 0;
			this.wr_wait = wr_wait != 0;
			this.ex_wait = ex_wait != 0;

			this.wr_mem_valid = wr_mem_valid != 0;
			this.wr_mem_cnt = wr_mem_cnt;
			this.wr_mem_adr = wr_mem_adr;
			this.wr_mem_adr_stride = wr_mem_adr_stride;
			this.wr_mem_reg = wr_mem_reg;
			this.wr_mem_reg_stride = wr_mem_reg_stride;

			this.rd_mem_valid = rd_mem_valid != 0;
			this.rd_mem_cnt = rd_mem_cnt;
			this.rd_mem_adr = rd_mem_adr;
			this.rd_mem_adr_stride = rd_mem_adr_stride;
			this.rd_mem_reg = rd_mem_reg;
			this.rd_mem_reg_stride = rd_mem_reg_stride;

			this.ex_valid = ex_valid != 0;
			this.ex_opcode = ex_opcode;
			this.ex_src_reg1 = ex_src_reg1;
			this.ex_src_reg2 = ex_src_reg2;
			this.ex_dst_reg = ex_dst_reg;
			this.ex_cnt = ex_cnt;
			this.ex_src_reg1_stride = ex_src_reg1_stride;
			this.ex_src_reg2_stride = ex_src_reg2_stride;
			this.ex_dst_reg_stride = ex_dst_reg_stride;
			this.ex_src1_mem = ex_src1_mem != 0;
			this.ex_src2_mem = ex_src2_mem != 0;

			this.nextpc = next;
		}

	}


}

