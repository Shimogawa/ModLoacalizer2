using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModFileCore.ModLoader.Core;

namespace SimpleDumpatcher
{
	public class Patcher
	{
		private string file;

		public Patcher(string file)
		{
			this.file = file;
		}

		public void Patch()
		{
			string o = Path.GetFileName(file).Replace(".dll", ".tmod");

			if (!File.Exists(o))
			{
				throw new Exception("未找到相应tmod文件");
			}

			TmodFile tmf = new TmodFile(o);
			using (tmf.Open())
			{
				tmf.ReplacePrimaryAssembly(File.ReadAllBytes(file), false);
			}

			tmf.Save(o.Substring(0, o.Length - 5) + "_Patched.tmod");
		}
	}
}
