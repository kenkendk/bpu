using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

namespace bpusmecompiler
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			var file = args.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(file))
				file = file.Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.Personal));

			if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
			{
				Console.WriteLine("No such file: {0}", file);
				return 4;
			}

			var parentfolder = Path.GetDirectoryName(file);
			var basename = Path.GetFileNameWithoutExtension(file) ?? "";
			if (basename.StartsWith("instr-"))
				basename = basename.Substring("instr".Length);
			else if (basename.StartsWith("map-"))
				basename = basename.Substring("map".Length);
			else if (basename.StartsWith("mem-"))
				basename = basename.Substring("mem".Length);
			else
			{
				Console.WriteLine("Unsupported filename: {0}", basename);
				return 4;
			}

			var instrname = Path.ChangeExtension(Path.Combine(parentfolder, "instr" + basename), "bin");
			var mapname = Path.ChangeExtension(Path.Combine(parentfolder, "map" + basename), "bin");
			var memname = Path.ChangeExtension(Path.Combine(parentfolder, "mem" + basename), "bin");

			if (!File.Exists(instrname))
			{
				Console.WriteLine("Missing file: {0}", instrname);
				return 4;
			}

			if (!File.Exists(mapname))
			{
				Console.WriteLine("Missing file: {0}", mapname);
				return 4;
			}

			if (!File.Exists(memname))
			{
				Console.WriteLine("Missing file: {0}", memname);
				return 4;
			}

			var instr = Deserializer.DeserializeKernel(instrname);
			var map = Deserializer.DeserializeMemoryMap(mapname);

			var cs = Compiler.Compile(instr, map);

			File.WriteAllText("output.json", Newtonsoft.Json.JsonConvert.SerializeObject(new {
				Code = cs.code,
				Constants = cs.constantmap
			}));

			File.WriteAllText("output.vhdl", new BPUSimulator.CodeDump(cs.code.ToArray()).TransformText());
			File.WriteAllText("register-preload.mif", new BPUSimulator.MemoryDumpMIF(cs.constantmap.Select(x => (BPUImplementation.RegisterData)(uint)x).ToArray()).TransformText());

			var buf = new byte[BPUImplementation.RegisterData.DATA_LENGTH];
			var r = 0;
			var mem = new List<BPUImplementation.RegisterData>();
			using (var fs = File.OpenRead(memname))
				do
				{
					r = fs.Read(buf, 0, buf.Length);
					if (r != buf.Length && r != 0)
						throw new Exception("Bad file data");
					mem.Add((BPUImplementation.RegisterData)BitConverter.ToUInt16(buf, 0));
				}
				while(r != 0);
				
			File.WriteAllText("data.mif", new BPUSimulator.MemoryDumpMIF(mem.ToArray()).TransformText());

			if (!Directory.Exists("output"))
				Directory.CreateDirectory("output");

			BPUSimulator.TestRunner.RunTest("output", new TestRunner(mem.ToArray(), cs.code.ToArray()));

			return 0;
		}
	}
}
