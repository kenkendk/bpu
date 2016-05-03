using System;
using SME;
using System.Threading.Tasks;
using SME.Render.VHDL;

namespace BPUImplementation
{
	public class MemoryWriter : SimpleProcess
	{
		public interface IOutput : IBus
		{
			[InitialValue(false)]
			bool Blocked { get; set; }
			[InitialValue(0)]
			byte QueueSize { get; set; }
		}

		public interface IInput : IBus
		{
			[InitialValue(false)]
			bool Valid { get; set; }
			MemoryAddress Address { get; set; }
			RegisterData Data { get; set; }
		}

		[InitializedBus]
		public interface IInternal : IBus
		{
			byte BufferFill { get; set; }
			byte BufferTail { get; set; }
			byte Countdown { get; set;}
		}

		[InputBus]
		IInput In;
		[OutputBus]
		IOutput Out;

		[OutputBus]
		MockMemoryItem.IWriteIn MemoryOut;

		[InternalBus]
		IInternal Internal;

		public const byte BUFFER_SIZE = 8;
		public const byte MAX_BUFFER_FILL = (byte)(BUFFER_SIZE - 2);
		// The minimum number of clock cycles between each write
		public static readonly byte WRITE_DELAY = 0;

		[VHDLSignal]
		private RegisterData[] m_bufferData = new RegisterData[BUFFER_SIZE];
		[VHDLSignal]
		private MemoryAddress[] m_bufferAdr = new MemoryAddress[BUFFER_SIZE];

		protected override void OnTick()
		{
			Out.Blocked = Internal.BufferFill >= MAX_BUFFER_FILL;
			Out.QueueSize = Internal.BufferFill;

			MemoryOut.Enabled = false;
			MemoryOut.Address = 0;
			MemoryOut.Data = 0;

			var anytopop = Internal.Countdown == 0 && Internal.BufferFill != 0;

			if (anytopop)
			{
				MemoryOut.Enabled = true;
				MemoryOut.Address = m_bufferAdr[Internal.BufferTail];
				MemoryOut.Data = m_bufferData[Internal.BufferTail];

				Internal.Countdown = WRITE_DELAY;
				Internal.BufferFill--;
				Internal.BufferTail = (byte)((Internal.BufferTail + 1) % BUFFER_SIZE);
				Out.Blocked = Internal.BufferFill - 1 >= MAX_BUFFER_FILL;
				Out.QueueSize = (byte)(Internal.BufferFill - 1);
			}

			if (In.Valid && Internal.BufferFill < BUFFER_SIZE)
			{
				// Bypass the buffers and go straight out
				if (Internal.BufferFill == 0 && Internal.Countdown == 0)
				{
					MemoryOut.Enabled = true;
					MemoryOut.Address = In.Address;
					MemoryOut.Data = In.Data;
					Internal.Countdown = WRITE_DELAY;
				}
				// Buffer the request
				else
				{
					m_bufferData[(Internal.BufferTail + Internal.BufferFill) % BUFFER_SIZE] = In.Data;
					m_bufferAdr[(Internal.BufferTail + Internal.BufferFill) % BUFFER_SIZE] = In.Address;

					// If we have already taken one off the stack,
					// adjust the counters
					if (anytopop)
					{
						Internal.BufferFill = Internal.BufferFill;
						Out.Blocked = Internal.BufferFill >= MAX_BUFFER_FILL;
						Out.QueueSize = Internal.BufferFill;
					}
					// Otherwise increase the fill
					else
					{
						Internal.BufferFill++;
						Out.Blocked = Internal.BufferFill + 1 >= MAX_BUFFER_FILL;
						Out.QueueSize = (byte)(Internal.BufferFill + 1);
					}
				}
			}
		}
	}
}

