using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLocalizer2.ModLoader.Core;

namespace SimpleDumpatcher
{
	public class Dumper
	{
		private string file;

		public Dumper(string file)
		{
			this.file = file;
		}

		public void Dump()
		{
			string o = Path.GetFileName(file).Replace(".tmod", ".dll");
			if (File.Exists(o))
			{
				File.Copy(o, o + ".bak");
			}
			
			TmodFile tmf = new TmodFile(file);
			byte[] data;
			using (tmf.Open())
			{
				data = tmf.GetPrimaryAssembly(false);
			}
			if (data == null)
				throw new Exception("未找到dll");
			File.WriteAllBytes(o, data);
		}
	}
}
