using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using ModFileCore.ModLoader.Core;

namespace ModLocalizer2
{
	public static class Util
	{
		public static string GetFolderPath(this TmodFile file, params string[] paths)
		{
			return Path.Combine(file.Name, Path.Combine(paths));
		}

		public static bool HasBaseType(this ITypeDefOrRef type, string name)
		{
			while ((type = type.GetBaseType()) != null && (type.FullName != name)) { }

			return type.FullName == name;
		}
	}
}
