using System;
using SME;

namespace BPUImplementation
{
	public class RegisterInputDistributor : SimpleProcess
	{
		[InputBus]
		private ALU.IOutput Input;

		[OutputBus]
		private RegisterBank2.IWriteIn Bank2Out;
		[OutputBus]
		private RegisterBank3.IWriteIn Bank3Out;
		[OutputBus]
		private RegisterBank4.IWriteIn Bank4Out;


		protected override void OnTick()
		{
			Bank2Out.Enabled = Bank3Out.Enabled = Bank4Out.Enabled = Input.Valid;
			Bank2Out.Address = Bank3Out.Address = Bank4Out.Address = Input.Valid ? Input.DstReg : (RegisterAddress)0;
			Bank2Out.Data = Bank3Out.Data = Bank4Out.Data = Input.Valid ? Input.Result : 0;
		}

	}
}

