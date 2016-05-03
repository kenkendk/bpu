using System;
using SME;
using BPUImplementation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BPUSimulator
{
	public static class TestRunner
	{
		public static readonly int FLUSH_TICKS = Math.Max(MemoryWriter.WRITE_DELAY + 1, 2);

		public static void RunTests(string targetfolder, string csvfolder = null, params ITestSetup[] setups)
		{
			RunTests(targetfolder, (IEnumerable<ITestSetup>)setups, csvfolder);
		}

		public static void RunTests(string targetfolder, IEnumerable<ITestSetup> setups, string csvfolder = null)
		{
			foreach (var t in setups)
			{
				Console.Write("Running test: {0} ....", t.GetType().Name);
				var r = RunTest(targetfolder, t, csvfolder);
				Console.Write(r ? "OK" : "--~~== * ERROR * ==~~--");
				Console.WriteLine();
			}
		}

		public static bool RunTest(string targetfolder, ITestSetup setup, string csvfolder = null)
		{
			var testname = setup.GetType().Name;

			Loader.Reset();

			EmulatedMemoryStorage.m_data = new byte[setup.MemorySize];
			//RegisterFile.RegisterFileSize = setup.RegisterFileSize;
			ProgramMemory.Program = setup.Program;

			File.WriteAllText(Path.Combine(targetfolder, testname + ".microcode.vhdl"), new CodeDump(setup.Program).TransformText());

			if (setup.PreMemory != null)
			{
				File.WriteAllText(Path.Combine(targetfolder, testname + ".memory.vhdl"), new MemoryDump(setup.PreMemory).TransformText());
				File.WriteAllText(Path.Combine(targetfolder, testname + ".memory.coe"), new MemoryDumpCOE(setup.PreMemory).TransformText());
				File.WriteAllText(Path.Combine(targetfolder, testname + ".memory.mif"), new MemoryDumpMIF(setup.PreMemory).TransformText());

				for (var i = 0u; i < setup.PreMemory.Length; i++)
					EmulatedMemoryStorage.Write(i * RegisterData.DATA_LENGTH, setup.PreMemory[i]);
			}

			setup.PreExecute();

			var processes = Loader.LoadAssembly(typeof(BPUImplementation.MicrocodeDriver).Assembly);

			var tracer = new SME.Render.VHDL.CSVTracer(Path.Combine(csvfolder ?? targetfolder, testname + ".csv"));

			var begin = DateTime.Now;

			// Simulate !
			var dg = Loader.RunUntilCompletion(processes, () => 	{
				tracer.OnClockTick();

				/*if (Clock.DefaultClock.Ticks < 50)
					Console.WriteLine("-------- Ticked {0} --------", Clock.DefaultClock.Ticks);
				if (Clock.DefaultClock.Ticks == 50)
					Console.WriteLine("-------- Still ticking .... --------");*/
			});

			// Flush to ensure we are done
			for (var i = 0; i < FLUSH_TICKS; i++)
				dg.Execute();

			var end = DateTime.Now;

			setup.PostExecute();

			Console.WriteLine("Executed {0} in {1} ticks", setup.GetType().Name, Clock.DefaultClock.Ticks);

			setup.PreValidate();

			// Propagate data back to a byte-based layout
			processes.Where(x => x is MockMemoryItem).Cast<MockMemoryItem>().First().WriteMemoryBack();

			var errors = false;
			if (setup.PostMemory != null)
			{
				for (var i = 0u; i < setup.PostMemory.Length; i++)
				{
					var actual = (uint)EmulatedMemoryStorage.Read(i * RegisterData.DATA_LENGTH);
					var expected = setup.PostMemory[i];
					if (actual != expected)
					{
						Console.WriteLine("Error in memory at {0}(*{3}), value is: {1} expected {2}", i, actual, expected, RegisterData.DATA_LENGTH);
						errors = true;
					}
				}
			}

			setup.PostValidate();

			return !errors;
		}
	}
}

