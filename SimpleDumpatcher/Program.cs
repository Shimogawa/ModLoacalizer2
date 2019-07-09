
using System;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;

namespace SimpleDumpatcher
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("1——拆dll，2——插入dll");
			try
			{
				int p = int.Parse(Console.ReadLine());
				if (p == 1)
				{
					GetDumper().Dump();
				}
				else if (p == 2)
				{
					GetPatcher().Patch();
				}
				else
				{
					throw new Exception("选项错误");
				}
				Console.WriteLine("完成。");
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine(e.StackTrace);
			}
			Console.ReadLine();
		}

		public static Dumper GetDumper()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.tmod");
			for (int i = 0; i < files.Length; i++)
			{
				Console.WriteLine($"{i}: {Path.GetFileName(files[i])}");
			}
			Console.Write("数字选择需要获取dll的文件：");
			int choose = int.Parse(Console.ReadLine());
			return new Dumper(files[choose]);
		}

		public static Patcher GetPatcher()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dll");
			for (int i = 0; i < files.Length; i++)
			{
				if (!File.Exists(files[i].Replace(".dll", ".tmod")))
					continue;
				Console.WriteLine($"{i}: {Path.GetFileName(files[i])}");
			}
			Console.Write("数字选择需要插入dll的文件：");
			int choose = int.Parse(Console.ReadLine());
			return new Patcher(files[choose]);
		}
	}
}
