using System;
using SME;
using SME.VHDLComponents;
using SME.Render.VHDL;

namespace BPUImplementation
{
	public abstract class RegisterBank : SimpleDualPortMemory<RegisterAddress, RegisterData>
	{
		public RegisterBank()
		{
			m_memory = new RegisterData[(64 * 1024) / RegisterData.DATA_LENGTH];
		}

		protected override int ConvertAddress(RegisterAddress adr)
		{
			return (int)(uint)adr;
		}

		protected override int AddressWidth { get { return 10; } }
		protected override int DataWidth { get { return RegisterData.DATA_LENGTH * 8; } }
	}

	[VHDLComponent("reg_bank")]
	public class RegisterBank0 : RegisterBank 
	{
		new public interface IReadIn : RegisterBank.IReadIn {};
		new public interface IWriteIn : RegisterBank.IWriteIn {};

		new public interface IReadOut : RegisterBank.IReadOut {};

		protected override void Setup(Clock clock)
		{
			SetBusses<IReadIn, IReadOut, IWriteIn>(clock);
		}
	}

	[VHDLComponent("reg_bank")]
	public class RegisterBank1 : RegisterBank 
	{
		new public interface IReadIn : RegisterBank.IReadIn {};
		new public interface IWriteIn : RegisterBank.IWriteIn {};

		new public interface IReadOut : RegisterBank.IReadOut {};

		protected override void Setup(Clock clock)
		{
			SetBusses<IReadIn, IReadOut, IWriteIn>(clock);
		}
	}

	[VHDLComponent("reg_bank")]
	public class RegisterBank2 : RegisterBank 
	{
		new public interface IReadIn : RegisterBank.IReadIn {};
		new public interface IWriteIn : RegisterBank.IWriteIn {};

		new public interface IReadOut : RegisterBank.IReadOut {};

		protected override void Setup(Clock clock)
		{
			SetBusses<IReadIn, IReadOut, IWriteIn>(clock);
		}
	}

	[VHDLComponent("reg_bank")]
	public class RegisterBank3 : RegisterBank 
	{
		new public interface IReadIn : RegisterBank.IReadIn {};
		new public interface IWriteIn : RegisterBank.IWriteIn {};

		new public interface IReadOut : RegisterBank.IReadOut {};

		protected override void Setup(Clock clock)
		{
			SetBusses<IReadIn, IReadOut, IWriteIn>(clock);
		}
	}

	[VHDLComponent("reg_bank")]
	public class RegisterBank4 : RegisterBank 
	{
		new public interface IReadIn : RegisterBank.IReadIn {};
		new public interface IWriteIn : RegisterBank.IWriteIn {};

		new public interface IReadOut : RegisterBank.IReadOut {};

		protected override void Setup(Clock clock)
		{
			SetBusses<IReadIn, IReadOut, IWriteIn>(clock);
		}
	}

}

