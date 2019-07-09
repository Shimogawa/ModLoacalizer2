using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModLocalizer2.ModLoader.Core;

namespace ConsoleTest
{
	class Program
	{
		static void Main(string[] args)
		{
			TmodFile file = new TmodFile("test.tmod");
			using (file.Open())
			{
				foreach (var e in file)
				{
					if (e.Key == "Windows.dll")
					{
						Console.WriteLine(e.Value.ToString());
					}
				}
			}

//			file.ReplaceFile("Windows.dll", new byte[0]);
//			file.Save("test2.tmod");

			Console.Read();
		}
	}
}
