using System;
using SME;
using SME.Render.VHDL;

namespace BPUImplementation
{
	[VHDLIgnore]
	public class CompletionDetector : Process
	{
		[InputBus]
		private MicrocodeDriver.IOutput Signal;

		public async override System.Threading.Tasks.Task Run()
		{
			await WaitUntilAsync(() => Signal.Completed );
		}
	}
}

