using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModLocalizer2.ModLoader.Core;

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
				if (tmf.HasFile("Windows.dll"))
					tmf.ReplaceFile("Windows.dll", File.ReadAllBytes(file));
				else
				{
					var l = tmf.Where(r => r.Key.EndsWith(".XNA.dll")).ToList();
					if (l.Count != 0)
					{
						var name = l[0].Key;
						tmf.ReplaceFile(name, File.ReadAllBytes(file));
					}
					else
					{
						throw new Exception("没有找到可以替换的dll");
					}
				}
			}

			tmf.Save(o.Substring(0, o.Length - 5) + "_Patched.tmod");
		}
	}
}
