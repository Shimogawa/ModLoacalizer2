using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace ModFileCore.ModLoader.Core
{
	[JsonObject(MemberSerialization.Fields)]
	public class BuildProperties
	{
		// Token: 0x06001992 RID: 6546 RVA: 0x004143DC File Offset: 0x004125DC
		public IEnumerable<ModReference> Refs(bool includeWeak)
		{
			if (!includeWeak)
			{
				return this.ModReferences;
			}
			return this.ModReferences.Concat(this.WeakReferences);
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x00414406 File Offset: 0x00412606
		public IEnumerable<string> RefNames(bool includeWeak)
		{
			return from dep in this.Refs(includeWeak)
				   select dep.Mod;
		}

		// Token: 0x06001994 RID: 6548 RVA: 0x00414434 File Offset: 0x00412634
		private static IEnumerable<string> ReadList(string value)
		{
			return from s in value.Split(new char[]
			{
				','
			})
				   select s.Trim() into s
				   where s.Length > 0
				   select s;
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x0041449C File Offset: 0x0041269C
		private static IEnumerable<string> ReadList(BinaryReader reader)
		{
			List<string> list = new List<string>();
			string text = reader.ReadString();
			while (text.Length > 0)
			{
				list.Add(text);
				text = reader.ReadString();
			}
			return list;
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x004144D0 File Offset: 0x004126D0
		private static void WriteList<T>(IEnumerable<T> list, BinaryWriter writer)
		{
			foreach (T t in list)
			{
				writer.Write(t.ToString());
			}
			writer.Write("");
		}

		public byte[] ToBytes()
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					if (DllReferences.Length != 0)
					{
						binaryWriter.Write("dllReferences");
						BuildProperties.WriteList<string>(this.DllReferences, binaryWriter);
					}
					if (ModReferences.Length != 0)
					{
						binaryWriter.Write("modReferences");
						BuildProperties.WriteList<BuildProperties.ModReference>(this.ModReferences, binaryWriter);
					}
					if (WeakReferences.Length != 0)
					{
						binaryWriter.Write("weakReferences");
						BuildProperties.WriteList<BuildProperties.ModReference>(this.WeakReferences, binaryWriter);
					}
					if (SortAfter.Length != 0)
					{
						binaryWriter.Write("sortAfter");
						BuildProperties.WriteList<string>(this.SortAfter, binaryWriter);
					}
					if (SortBefore.Length != 0)
					{
						binaryWriter.Write("sortBefore");
						BuildProperties.WriteList<string>(this.SortBefore, binaryWriter);
					}
					if (this.Author.Length > 0)
					{
						binaryWriter.Write("author");
						binaryWriter.Write(this.Author);
					}
					binaryWriter.Write("version");
					binaryWriter.Write(Version);
					if (this.DisplayName.Length > 0)
					{
						binaryWriter.Write("displayName");
						binaryWriter.Write(this.DisplayName);
					}
					if (this.Homepage.Length > 0)
					{
						binaryWriter.Write("homepage");
						binaryWriter.Write(this.Homepage);
					}
					if (this.Description.Length > 0)
					{
						binaryWriter.Write("description");
						binaryWriter.Write(this.Description);
					}
					if (this.NoCompile)
					{
						binaryWriter.Write("noCompile");
					}
					if (!this.HideCode)
					{
						binaryWriter.Write("!hideCode");
					}
					if (!this.HideResources)
					{
						binaryWriter.Write("!hideResources");
					}
					if (this.IncludeSource)
					{
						binaryWriter.Write("includeSource");
					}
					if (this.IncludePdb)
					{
						binaryWriter.Write("includePDB");
					}
					if (this.EacPath.Length > 0)
					{
						binaryWriter.Write("eacPath");
						binaryWriter.Write(this.EacPath);
					}
					if (this.Side != ModReference.ModSide.Both)
					{
						binaryWriter.Write("side");
						binaryWriter.Write((byte)this.Side);
					}
					binaryWriter.Write("buildVersion");
					binaryWriter.Write(BuildVersion);
					binaryWriter.Write("");
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		public static BuildProperties ReadFromModFile(TmodFile file)
		{
			using (var ms = new MemoryStream(file.GetTrueBytes(TConstants.InfoFileName)))
			{
				return ReadFromStream(ms);
			}
		}

//		// Token: 0x06001999 RID: 6553 RVA: 0x00414DDC File Offset: 0x00412FDC
//		internal static BuildProperties ReadModFile(TmodFile modFile)
//		{
//			return BuildProperties.ReadFromStream(modFile.GetStream("Info", false));
//		}

		// Token: 0x0600199A RID: 6554 RVA: 0x00414DF0 File Offset: 0x00412FF0
		public static BuildProperties ReadFromStream(Stream stream)
		{
			BuildProperties buildProperties = new BuildProperties();
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				string text = binaryReader.ReadString();
				while (text.Length > 0)
				{
					if (text == "dllReferences")
					{
						buildProperties.DllReferences = BuildProperties.ReadList(binaryReader).ToArray<string>();
					}
					if (text == "modReferences")
					{
						buildProperties.ModReferences = BuildProperties.ReadList(binaryReader).Select(new Func<string, BuildProperties.ModReference>(BuildProperties.ModReference.Parse)).ToArray<BuildProperties.ModReference>();
					}
					if (text == "weakReferences")
					{
						buildProperties.WeakReferences = BuildProperties.ReadList(binaryReader).Select(new Func<string, BuildProperties.ModReference>(BuildProperties.ModReference.Parse)).ToArray<BuildProperties.ModReference>();
					}
					if (text == "sortAfter")
					{
						buildProperties.SortAfter = BuildProperties.ReadList(binaryReader).ToArray<string>();
					}
					if (text == "sortBefore")
					{
						buildProperties.SortBefore = BuildProperties.ReadList(binaryReader).ToArray<string>();
					}
					if (text == "author")
					{
						buildProperties.Author = binaryReader.ReadString();
					}
					if (text == "version")
					{
						buildProperties.Version = binaryReader.ReadString();
					}
					if (text == "displayName")
					{
						buildProperties.DisplayName = binaryReader.ReadString();
					}
					if (text == "homepage")
					{
						buildProperties.Homepage = binaryReader.ReadString();
					}
					if (text == "description")
					{
						buildProperties.Description = binaryReader.ReadString();
					}
					if (text == "noCompile")
					{
						buildProperties.NoCompile = true;
					}
					if (text == "!hideCode")
					{
						buildProperties.HideCode = false;
					}
					if (text == "!hideResources")
					{
						buildProperties.HideResources = false;
					}
					if (text == "includeSource")
					{
						buildProperties.IncludeSource = true;
					}
					if (text == "includePDB")
					{
						buildProperties.IncludePdb = true;
					}
					if (text == "eacPath")
					{
						buildProperties.EacPath = binaryReader.ReadString();
					}
					if (text == "side")
					{
						buildProperties.Side = (ModReference.ModSide)binaryReader.ReadByte();
					}
					if (text == "beta")
					{
						buildProperties.Beta = true;
					}
					if (text == "buildVersion")
					{
						buildProperties.BuildVersion = binaryReader.ReadString();
					}
					text = binaryReader.ReadString();
				}
			}
			return buildProperties;
		}

		// Token: 0x0600199B RID: 6555 RVA: 0x00415054 File Offset: 0x00413254
		public static void InfoToBuildTxt(Stream src, Stream dst)
		{
			BuildProperties buildProperties = BuildProperties.ReadFromStream(src);
			StringBuilder stringBuilder = new StringBuilder();
			if (buildProperties.DisplayName.Length > 0)
			{
				stringBuilder.AppendLine("displayName = " + buildProperties.DisplayName);
			}
			if (buildProperties.Author.Length > 0)
			{
				stringBuilder.AppendLine("author = " + buildProperties.Author);
			}
			stringBuilder.AppendLine(string.Format("version = {0}", buildProperties.Version));
			if (buildProperties.Homepage.Length > 0)
			{
				stringBuilder.AppendLine("homepage = " + buildProperties.Homepage);
			}
			if (buildProperties.DllReferences.Length != 0)
			{
				stringBuilder.AppendLine("dllReferences = " + string.Join(", ", buildProperties.DllReferences));
			}
			if (buildProperties.ModReferences.Length != 0)
			{
				stringBuilder.AppendLine("modReferences = " + string.Join<BuildProperties.ModReference>(", ", buildProperties.ModReferences));
			}
			if (buildProperties.WeakReferences.Length != 0)
			{
				stringBuilder.AppendLine("weakReferences = " + string.Join<BuildProperties.ModReference>(", ", buildProperties.WeakReferences));
			}
			if (buildProperties.NoCompile)
			{
				stringBuilder.AppendLine("noCompile = true");
			}
			if (buildProperties.HideCode)
			{
				stringBuilder.AppendLine("hideCode = true");
			}
			if (buildProperties.HideResources)
			{
				stringBuilder.AppendLine("hideResources = true");
			}
			if (buildProperties.IncludeSource)
			{
				stringBuilder.AppendLine("includeSource = true");
			}
			if (buildProperties.IncludePdb)
			{
				stringBuilder.AppendLine("includePDB = true");
			}
			if (buildProperties.Side != ModReference.ModSide.Both)
			{
				stringBuilder.AppendLine(string.Format("side = {0}", buildProperties.Side));
			}
			if (buildProperties.SortAfter.Length != 0)
			{
				stringBuilder.AppendLine("sortAfter = " + string.Join(", ", buildProperties.SortAfter));
			}
			if (buildProperties.SortBefore.Length != 0)
			{
				stringBuilder.AppendLine("sortBefore = " + string.Join(", ", buildProperties.SortBefore));
			}
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			dst.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x0600199C RID: 6556 RVA: 0x0041526C File Offset: 0x0041346C
		internal bool ignoreFile(string resource)
		{
			return this.BuildIgnores.Any((string fileMask) => this.FitsMask(resource, fileMask));
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x004152A4 File Offset: 0x004134A4
		private bool FitsMask(string fileName, string fileMask)
		{
			return new Regex("^" + Regex.Escape(fileMask.Replace(".", "__DOT__").Replace("*", "__STAR__").Replace("?", "__QM__")).Replace("__DOT__", "[.]").Replace("__STAR__", ".*").Replace("__QM__", ".") + "$", RegexOptions.IgnoreCase).IsMatch(fileName);
		}

		internal string[] DllReferences = new string[0];

		internal ModReference[] ModReferences = new ModReference[0];

		internal ModReference[] WeakReferences = new ModReference[0];

		internal string[] SortAfter = new string[0];

		internal string[] SortBefore = new string[0];

		internal string[] BuildIgnores = new string[0];

		internal string Author = "";

		internal string Version = "1.0";

		internal string DisplayName = "";

		internal bool NoCompile;

		internal bool HideCode;
		
		internal bool HideResources;

		internal bool IncludeSource;

		internal bool IncludePdb = true;

		internal string EacPath = "";

		internal bool Beta = false;

		internal string BuildVersion = TConstants.NewestTmodVersion.ToString();

		internal string Homepage = "";

		internal string Description = "";

		internal ModReference.ModSide Side;

		[JsonObject(MemberSerialization.Fields)]
		public struct ModReference
		{
			public ModReference(string mod, string target)
			{
				this.Mod = mod;
				this.Target = target;
			}

			// Token: 0x06002981 RID: 10625 RVA: 0x004902FE File Offset: 0x0048E4FE
			public override string ToString()
			{
				if (!(this.Target == null))
				{
					return this.Mod + "@" + this.Target;
				}
				return this.Mod;
			}

			public static ModReference Parse(string spec)
			{
				string[] array = spec.Split(new char[]
				{
					'@'
				});
				if (array.Length == 1)
				{
					return new ModReference(array[0], null);
				}
				if (array.Length > 2)
				{
					throw new Exception("Invalid mod reference: " + spec);
				}
				ModReference result;
				try
				{
					result = new ModReference(array[0], array[1]);
				}
				catch
				{
					throw new Exception("Invalid mod reference: " + spec);
				}
				return result;
			}

			public string Mod;

			public string Target;

			public enum ModSide
			{
				// Token: 0x04000054 RID: 84
				Both,
				// Token: 0x04000055 RID: 85
				Client,
				// Token: 0x04000056 RID: 86
				Server,
				// Token: 0x04000057 RID: 87
				NoSync
			}
		}
	}
}
