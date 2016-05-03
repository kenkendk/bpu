using System;
using SME;

namespace BPUImplementation
{
	public class MemoryReadDistributor : SimpleProcess
	{
		[InputBus]
		MemoryReader.IOutput Input;

		[OutputBus]
		RegisterBank0.IWriteIn Out0;
		[OutputBus]
		RegisterBank1.IWriteIn Out1;

		protected override void OnTick()
		{
			Out0.Enabled = Out1.Enabled = Input.Valid;
			Out0.Address = Out1.Address = Input.Register;
			Out0.Data = Out1.Data = Input.Data;
		}
	}
}

