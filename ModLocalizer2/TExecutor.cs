using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModFileCore.ModLoader.Core;

namespace ModLocalizer2
{
	public abstract class TExecutor
	{
		protected Program.ProgramArgs Args { get; }

		protected TmodFile ModFile { get; }

		protected byte[] Assembly { get; }

		public TExecutor(Program.ProgramArgs args, TmodFile modFile)
		{
			Args = args;
			ModFile = modFile;
		}

		public abstract void Run();
	}
}
