using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModFileCore.ModLoader.Core
{
	public static class TConstants
	{
		public const string InfoFileName = "Info";

		public const string MagicHeader = "TMOD";

		public static readonly Version OldTmodVersion = new Version(0, 10, 1, 5);

		public static readonly Version NewTmodVersion = new Version(0, 11);

		public static readonly Version NewestTmodVersion = new Version(0, 11, 1);

		public static readonly Version TerrariaVersion = new Version(1, 3, 5, 3);
	}
}
