using SME;
using System;
using BPUImplementation;
using System.IO;

namespace BPUSimulator
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			//Loader.DebugBusAssignments = true;
			var vhdlfolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Udvikler", "bpu-sme", "vhdl");
			if (!Directory.Exists(vhdlfolder))
				Directory.CreateDirectory(vhdlfolder);

			var csvfolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Udvikler", "bpu-sme", "bpu-sme.sim");

			TestRunner.RunTests(vhdlfolder, csvfolder, new ITestSetup[] {
				new SimpleAddMultiply(),
				//new SimpleTrulsProgram(), // Requires divisions, which is currently disabled
				new SimpleReduction(),
				new SimpleRange(),
			});

			SME.Render.GraphViz.Renderer.Render(Loader.LoadAssembly(typeof(BPUImplementation.MicrocodeDriver).Assembly), "myfile.dot");
		}
	}
}
