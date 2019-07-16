using System;
using System.IO;
using Microsoft.Extensions.CommandLineUtils;
using ModFileCore.ModLoader.Core;

namespace ModLocalizer2
{
	public class Program
	{
		public static readonly Version version = typeof(Program).Assembly.GetName().Version;

		private static ProgramArgs config;

		static void Main(string[] args)
		{
#if DEBUG
			GetExecutor(new ProgramArgs()
			{
				FilePath = "test.tmod",
				IsDump = true
			}).Run();
#else
			if (args.Length == 0)
			{
				args = new[] { "--help" };
			}

			GetApp().Execute(args);
			GetExecutor(config).Run();
#endif
		}

		static TExecutor GetExecutor(ProgramArgs args)
		{
			if (!File.Exists(args.FilePath))
			{
				Console.Error.WriteLine("未找到mod文件。");
				return null;
			}

			TmodFile file = new TmodFile(args.FilePath);
			using (file.Open()) { }

			if (args.IsDump)
			{
				return new Dumper(args, file);
			}

			return new Patcher(args, file);
		}

		static CommandLineApplication GetApp()
		{
			CommandLineApplication app = new CommandLineApplication()
			{
				FullName = typeof(Program).Namespace,
				Description = "Not provided yet",
				ShortVersionGetter = () => version.ToString(2),
				LongVersionGetter = () => version.ToString(3)
			};
			app.HelpOption("--help | -h");
			app.VersionOption("--version | -v", version.ToString(2), version.ToString(3));
			var arg = app.Argument("path", "文件夹路径");
			var modeOpt = app.Option("-m | --mode", "模式，patch或dump，默认dump", CommandOptionType.SingleValue);
			var folderOpt = app.Option("-f | --folder", "需要打包文件所在的文件夹", CommandOptionType.SingleValue);
			var languageOpt = app.Option("-l | --language", "默认中文(Chinese)。", CommandOptionType.SingleValue);

			app.OnExecute(() =>
			{
				config = new ProgramArgs();
				if (modeOpt.HasValue())
				{
					config.IsDump = modeOpt.Value().ToLower() == "dump";
				}

				if (folderOpt.HasValue())
				{
					config.Folder = folderOpt.Value();
				}

				if (languageOpt.HasValue())
				{
					config.Lang = languageOpt.Value();
				}

				config.FilePath = arg.Value;

				if (string.IsNullOrWhiteSpace(config.FilePath))
				{
					Console.Error.WriteLine("请指定模组文件。");
					Environment.Exit(1);
				}
				if (!config.IsDump && string.IsNullOrWhiteSpace(config.FilePath))
				{
					Console.Error.WriteLine("请指定打包的文件夹。");
					Environment.Exit(1);
				}
				return 0;
			});

			return app;
		}

		public class ProgramArgs
		{
			public bool IsDump { get; internal set; } = true;

			public string FilePath { get; internal set; } = null;

			public string Folder { get; internal set; } = null;

			public string Lang { get; internal set; } = "Chinese";
		}
	}
}
