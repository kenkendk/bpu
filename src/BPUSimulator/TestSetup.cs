using System;
using BPUImplementation;

namespace BPUSimulator
{
	public abstract class TestSetup : ITestSetup
	{
		#region ITestSetup implementation
		public virtual void PreExecute()
		{
		}
		public virtual void PostExecute()
		{
		}
		public virtual void PreValidate()
		{
		}
		public virtual void PostValidate()
		{
		}
		public virtual int MemorySize
		{
			get
			{
				return 1024;
			}
		}
		public virtual RegisterData[] PreMemory
		{
			get
			{
				return null;
			}
		}
		public abstract RegisterData[] PostMemory { get; }
		public abstract Microcode[] Program { get; }
		#endregion
	}
}

