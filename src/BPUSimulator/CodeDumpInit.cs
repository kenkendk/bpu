using System;
using BPUImplementation;

namespace BPUSimulator
{
	public partial class CodeDump
	{
		private Microcode[] microcode;
		public CodeDump(Microcode[] code) { microcode = code; }
	}
}

