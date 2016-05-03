using System;
using SME;

namespace BPUImplementation
{
	public class BankForwarder : SimpleProcess
	{
		[InputBus]
		MemoryWriteFeeder.IOutput WriteIn;
		[InputBus]
		ExecuteFeeder.IOutput ExecuteIn;

		[OutputBus]
		RegisterBank0.IReadIn Out0;
		[OutputBus]
		RegisterBank1.IReadIn Out1;
		[OutputBus]
		RegisterBank2.IReadIn Out2;
		[OutputBus]
		RegisterBank3.IReadIn Out3;
		[OutputBus]
		RegisterBank4.IReadIn Out4;

		protected override void OnTick()
		{
			Out0.Address = Out3.Address = ExecuteIn.DstValid ? ExecuteIn.SrcReg1 : (RegisterAddress)0;
			Out1.Address = Out4.Address = ExecuteIn.DstValid ? ExecuteIn.SrcReg2 : (RegisterAddress)0;
			Out2.Address = WriteIn.Valid ? WriteIn.Register : (RegisterAddress)0;
		}

	}
}

