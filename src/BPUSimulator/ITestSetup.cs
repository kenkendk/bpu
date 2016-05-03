using System;
using BPUImplementation;

namespace BPUSimulator
{
	public interface ITestSetup
	{
		int MemorySize { get; }
		RegisterData[] PreMemory { get; }
		RegisterData[] PostMemory {get; }

		Microcode[] Program { get; }

		void PreExecute();
		void PostExecute();
		void PreValidate();
		void PostValidate();
	}
}

