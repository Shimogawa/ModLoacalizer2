using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModFileCore.ModLoader.Core;

namespace ModUpDowngrader
{
	class Program
	{
		static void Main(string[] args)
		{
#if DEBUG
			Downgrade("testdown.tmod");
			Upgrade("testdown_down.tmod");
			return;
#endif
			if (args.Length != 2)
			{
				Console.WriteLine("使用方式：``ModUpDowngrader up/down filename``");
				return;
			}
			switch (args[0].ToLower())
			{
				case "down":
					Downgrade(args[1]);
					break;
				case "up":
					Upgrade(args[1]);
					break;
			}
		}

		static void Upgrade(string file)
		{
			TmodFile tmf = new TmodFile(file);
			using (tmf.Open())
			{
				tmf.Upgrade();
				tmf.Save(file.Replace(".tmod", "_up.tmod"));
			}
		}

		static void Downgrade(string file)
		{
			TmodFile tmodFile = new TmodFile(file);
			using (tmodFile.Open())
			{
				tmodFile.Downgrade();
				tmodFile.Save(file.Replace(".tmod", "_down.tmod"));
			}
		}
	}
}
