using System;
using SME;
using System.Threading.Tasks;

namespace BPUImplementation
{
	public class MemoryWriteFeeder : SimpleProcess
	{
		public interface IInput: IBus
		{
			[InitialValue(false)]
			bool Valid { get; set; }
			MemoryAddress StartAddr { get; set; }
			RegisterAddress StartReg { get; set; }
			Counter RepeatCount { get; set; }
			bool Blocked { get; set; }
			OffsetCounter AddrStride { get; set; }
			OffsetCounter RegStride { get; set; }
		}

		public interface IOutput : IBus
		{
			[InitialValue(false)]
			bool Valid { get; set; }
			RegisterAddress Register { get; set; }
			MemoryAddress Address { get; set; }
			[InitialValue(false)]
			bool Ready { get; set; }
		}

		[InitializedBus]
		public interface IInternal : IBus
		{
			bool Ready { get; set; }
			Counter Count { get; set; }
			RegisterAddress Register { get; set; }
			MemoryAddress Address { get; set; }
			OffsetCounter AddrStride { get; set; }
			OffsetCounter RegStride { get; set; }

		}

		[InputBus]
		IInput In;
		[OutputBus]
		IOutput Out;
		[InternalBus]
		IInternal Internal;

		protected override void OnTick()
		{
			Out.Valid = false;
			Out.Register = 0;
			Out.Address = 0;
			Out.Ready = false;
			Internal.Ready = false;

			if (In.Valid && Internal.Count == 0 && !In.Blocked)
			{
				if (In.RepeatCount == 0)
				{
					Out.Ready = true;
				}
				else
				{
					Internal.Count = In.RepeatCount - 1;
					Internal.Register = (RegisterAddress)(In.StartReg + In.RegStride);
					Internal.Address = In.StartAddr + In.AddrStride;
					Internal.RegStride = In.RegStride;
					Internal.AddrStride = In.AddrStride;

					Out.Register = In.StartReg;
					Out.Address = In.StartAddr;
					Out.Valid = true;

					if (In.RepeatCount == 1)
						Out.Ready = true;
				}
			}
			else
			{
				if (In.Valid && (Internal.Count == 1 || (Internal.Count == 0 && In.Blocked)))
				{
					Internal.Count = In.RepeatCount;
					Internal.Register = In.StartReg;
					Internal.Address = In.StartAddr;
					Internal.RegStride = In.RegStride;
					Internal.AddrStride = In.AddrStride;

					if (In.RepeatCount == 0)
						Internal.Ready = true;
				}

				if ((Internal.Count == 1 && !In.Blocked) || Internal.Ready)
					Out.Ready = true;

				if (Internal.Count != 0 && In.Blocked == false)
				{
					Out.Register = Internal.Register;
					Out.Address = Internal.Address;
					Out.Valid = true;

					Internal.Count--;
					Internal.Register = (RegisterAddress)(Internal.Register + (int)Internal.RegStride);
					Internal.Address += Internal.AddrStride;
				}
			}
		}
	}
}

