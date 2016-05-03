using System;
using SME;
using System.Threading.Tasks;

namespace BPUImplementation
{
	public class MemoryReader : SimpleProcess
	{
		public interface IInput : IBus
		{
			[InitialValue(false)]
			bool Valid { get; set; }
			MemoryAddress StartAddr { get; set; }
			RegisterAddress StartReg { get; set; }
			Counter RepeatCount { get; set; }
			OffsetCounter AddrStride { get; set; }
			OffsetCounter RegStride { get; set; }
		}

		public interface IOutput : IBus
		{
			[InitialValue(false)]
			bool Ready { get; set; }
			[InitialValue(false)]
			bool Valid { get; set; }
			RegisterAddress Register { get; set; }
			RegisterData Data { get; set; }
		}

		[InitializedBus]
		public interface IInternal : IBus
		{
			Counter InputCount { get; set; }
			MemoryAddress InputAddress { get; set; }
			byte InputDelayCounter { get; set; }

			Counter OutputCount { get; set; }
			RegisterAddress OutputAddress { get; set; }
			byte OutputDelayCounter { get; set; }

			OffsetCounter AddrStride { get; set; }
			OffsetCounter RegStride { get; set; }
		}

		[InputBus]
		IInput In;
		[OutputBus]
		IOutput Out;

		[InputBus]
		MockMemoryItem.IReadOut MemReadIn;
		[OutputBus]
		MockMemoryItem.IReadIn MemReadOut;

		[InternalBus]
		IInternal Internal;

		public MemoryReader() : base()
		{
			if (READ_LATENCY > READ_DELAY + 1)
				throw new Exception("Invalid configuration, latency must not be larger than read delay");
			if (READ_LATENCY > 1)
				throw new Exception("Latency must be at most 1 clock");
		}

		// Time between each read can be issued
		public static readonly byte READ_DELAY = 0;
		// Time after a read until the data is ready
		public static readonly byte READ_LATENCY = 1;

		protected override void OnTick()
		{
			DebugOutput = true;
			MemReadOut.Address = 0;

			Out.Valid = false;
			Out.Ready = false;
			Out.Register = 0;
			Out.Data = 0;

			//Countdown
			if (Internal.OutputDelayCounter != 0)
				Internal.OutputDelayCounter--;

			// If we hit 1, then the data is ready
			if (READ_LATENCY != 0 && Internal.OutputDelayCounter == 1 && Internal.OutputCount != 0)
			{
				Out.Valid = true;
				Out.Register = Internal.OutputAddress;
				Out.Data = MemReadIn.Data;
				Internal.OutputCount--;
				Internal.OutputAddress = (RegisterAddress)(Internal.OutputAddress + (int)Internal.AddrStride);
			}

			// Countdown
			if (Internal.InputDelayCounter != 0)
				Internal.InputDelayCounter--;

			// If we have more data
			else if (Internal.InputCount != 0)
			{
				MemReadOut.Address = Internal.InputAddress;
				Internal.InputDelayCounter = READ_DELAY;

				Internal.InputAddress += Internal.AddrStride;
				Internal.InputCount--;

				// For fast memory, bypass the counter
				if (READ_LATENCY == 0)
				{
					Out.Valid = true;
					Out.Register = Internal.OutputAddress;
					Out.Data = MemReadIn.Data;
				}
				else
				{
					Internal.OutputCount--;
					Internal.OutputAddress = (RegisterAddress)(Internal.OutputAddress + (int)Internal.RegStride);
					Internal.OutputDelayCounter = READ_LATENCY;
				}

				Out.Ready = Internal.InputCount == 1;
			}

			// If we accept input
			if (Internal.InputCount == 0 && In.Valid)
			{
				if (Internal.InputDelayCounter == 0)
				{
					if (In.RepeatCount > 0)
					{
						MemReadOut.Address = In.StartAddr;
						Internal.InputDelayCounter = READ_DELAY;

						Internal.InputCount = In.RepeatCount - 1;
						Internal.InputAddress = In.StartAddr + In.AddrStride;
						Internal.AddrStride = In.AddrStride;
						Internal.RegStride = In.RegStride;

						if (READ_LATENCY == 0)
						{
							Out.Valid = true;
							Out.Register = In.StartReg;
							Out.Data = MemReadIn.Data;
							Internal.OutputDelayCounter = 0;
							Internal.OutputCount = In.RepeatCount - 1;
							Internal.OutputAddress = (RegisterAddress)(In.StartReg + In.RegStride);
						}
						else
						{
							Internal.OutputDelayCounter = READ_LATENCY;
							Internal.OutputCount = In.RepeatCount;
							Internal.OutputAddress = In.StartReg;

						}
					}

					Out.Ready = In.RepeatCount == 0 || In.RepeatCount == 1;
				}
				else
				{
					Internal.InputCount = In.RepeatCount;
					Internal.InputAddress = In.StartAddr;
					Internal.AddrStride = In.AddrStride;
					Internal.RegStride = In.RegStride;

					Out.Ready = In.RepeatCount == 0;
				}
			}


		}
	}
}

