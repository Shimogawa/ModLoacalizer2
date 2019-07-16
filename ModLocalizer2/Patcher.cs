using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModFileCore.ModLoader.Core;

namespace ModLocalizer2
{
	public class Patcher : TExecutor
	{
		public Patcher(Program.ProgramArgs args, TmodFile modFile) : base(args, modFile) { }
		public override void Run()
		{
			throw new NotImplementedException();
		}
	}
}
