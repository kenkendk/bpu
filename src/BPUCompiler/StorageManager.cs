using System;
using System.Collections.Generic;
using System.Linq;

namespace bpusmecompiler
{
	/// <summary>
	/// Simple class that performs storage managemet, similar to malloc/free
	/// </summary>
	public class StorageManager
	{
		/// <summary>
		/// Types of allocation strategies
		/// </summary>
		public enum Strategy
		{
			/// <summary>
			/// The first available fit is chosen
			/// </summary>
			FirstFit,
			/// <summary>
			/// The best available fit is chosen
			/// </summary>
			BestFit,
			/// <summary>
			/// The worst fit is chosen
			/// </summary>
			WorstFit
		}


		/// <summary>
		/// The size of the memory being managed
		/// </summary>
		public readonly ulong Size;

		/// <summary>
		/// The allocation strategy.
		/// </summary>
		public readonly Strategy AllocationStrategy;

		/// <summary>
		/// The total amount of free space
		/// </summary>
		public ulong FreeSpace { get { return m_freemap.Aggregate(0UL, (x,y) => x + y.Value); } }

		/// <summary>
		/// Gets the size of the largest free chunk.
		/// </summary>
		public ulong LargestFreeChunk { get { return m_freemap.Max(x => x.Value); } }

		/// <summary>
		/// Gets the number of allocations.
		/// </summary>
		public int Allocations { get { return m_allocations.Count; } }

		/// <summary>
		/// The internal list of allocations
		/// </summary>
		private Dictionary<ulong, ulong> m_allocations = new Dictionary<ulong, ulong>();
		private Dictionary<ulong, ulong> m_freemap = new Dictionary<ulong, ulong>();

		/// <summary>
		/// Initializes a new instance of the <see cref="bpusmecompiler.StorageManager"/> class.
		/// </summary>
		/// <param name="size">The size of the storage being managed.</param>
		/// <param name="strategy">The allocation strategy to use</param>
		public StorageManager(ulong size, Strategy strategy = Strategy.BestFit)
		{
			Size = size;
			AllocationStrategy = strategy;
			m_freemap.Add(0u, size);
		}


		/// <summary>
		/// Allocates a chunk of the given size
		/// </summary>
		/// <param name="size">The size to allocate.</param>
		/// <returns>The offset of the allocated item</returns>
		public ulong Allocate(ulong size)
		{
			if (size > LargestFreeChunk)
				throw new Exception(string.Format("Attempted to allocate {0} bytes, but there was only {1} bytes free", size, LargestFreeChunk));

			var targets = m_freemap.Where(x => x.Value >= size);
			if (AllocationStrategy == Strategy.BestFit)
				targets = targets.OrderBy(x => x.Value - size);
			else if (AllocationStrategy == Strategy.WorstFit)
				targets = targets.OrderByDescending(x => x.Value - size);
			else //if (AllocationStrategy == Strategy.FirstFit)
				targets = targets.OrderBy(x => x.Key);

			var entry = targets.First();
			m_freemap.Remove(entry.Key);

			if (entry.Value > size)
				m_freemap.Add(entry.Key + size, entry.Value - size);
			else if (m_freemap.Count == 0)
				m_freemap.Add(this.Size, 0);
			
			m_allocations.Add(entry.Key, size);

			return entry.Key;
		}

		/// <summary>
		/// Releases a previously allocated entry
		/// </summary>
		/// <param name="offset">Offset.</param>
		public void Free(ulong offset)
		{
			if (!m_allocations.ContainsKey(offset))
				throw new Exception("No such entry or double free");
						
			var size = m_allocations[offset];
			m_allocations.Remove(offset);

			m_freemap.Add(offset, size);		

			var rerun = true;
			while (rerun && m_freemap.Count > 1)
			{
				rerun = false;

				var prev = m_freemap.OrderBy(x => x.Key).First();
				foreach (var el in m_freemap.OrderBy(x => x.Key).Skip(1))
				{
					if (prev.Key + prev.Value == el.Key)
					{
						m_freemap.Remove(el.Key);
						m_freemap[prev.Key] = prev.Value + el.Value;
						rerun = true;
						break;
					}

					prev = el;
				}
			}

			/*var firstFreeAfter = m_freemap.Where(x => x.Key > offset).OrderBy(x => x.Key);
			if (firstFreeAfter.Any())
			{
				var ffa = firstFreeAfter.First();
				if (ffa.Key - size == offset)
				{
					size += ffa.Value;
					m_freemap.Remove(ffa.Key);
					m_freemap.Add(offset, size);
					return;
				}
			}

			var firstFreeBefore = m_freemap.Where(x => x.Key < offset).OrderByDescending(x => x.Key);
			if (firstFreeBefore.Any())
			{
				var ffb = firstFreeBefore.First();
				if (ffb.Key + ffb.Value == offset)
				{
					size += ffb.Value;
					m_freemap.Remove(offset);
					m_freemap.Remove(ffb.Key);
					m_freemap.Add(ffb.Key, size);
					return;
				}
			}*/

		}


	}
}

