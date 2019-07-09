using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModLocalizer2.ModLoader.Core
{
	internal class BuildProperties
	{
		// Token: 0x06001992 RID: 6546 RVA: 0x004143DC File Offset: 0x004125DC
		public IEnumerable<BuildProperties.ModReference> Refs(bool includeWeak)
		{
			if (!includeWeak)
			{
				return this.modReferences;
			}
			return this.modReferences.Concat(this.weakReferences);
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x00414406 File Offset: 0x00412606
		public IEnumerable<string> RefNames(bool includeWeak)
		{
			return from dep in this.Refs(includeWeak)
				   select dep.mod;
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

		// Token: 0x06001998 RID: 6552 RVA: 0x00414B40 File Offset: 0x00412D40
		internal byte[] ToBytes()
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					if (this.dllReferences.Length != 0)
					{
						binaryWriter.Write("dllReferences");
						BuildProperties.WriteList<string>(this.dllReferences, binaryWriter);
					}
					if (this.modReferences.Length != 0)
					{
						binaryWriter.Write("modReferences");
						BuildProperties.WriteList<BuildProperties.ModReference>(this.modReferences, binaryWriter);
					}
					if (this.weakReferences.Length != 0)
					{
						binaryWriter.Write("weakReferences");
						BuildProperties.WriteList<BuildProperties.ModReference>(this.weakReferences, binaryWriter);
					}
					if (this.sortAfter.Length != 0)
					{
						binaryWriter.Write("sortAfter");
						BuildProperties.WriteList<string>(this.sortAfter, binaryWriter);
					}
					if (this.sortBefore.Length != 0)
					{
						binaryWriter.Write("sortBefore");
						BuildProperties.WriteList<string>(this.sortBefore, binaryWriter);
					}
					if (this.author.Length > 0)
					{
						binaryWriter.Write("author");
						binaryWriter.Write(this.author);
					}
					binaryWriter.Write("version");
					binaryWriter.Write(this.version.ToString());
					if (this.displayName.Length > 0)
					{
						binaryWriter.Write("displayName");
						binaryWriter.Write(this.displayName);
					}
					if (this.homepage.Length > 0)
					{
						binaryWriter.Write("homepage");
						binaryWriter.Write(this.homepage);
					}
					if (this.description.Length > 0)
					{
						binaryWriter.Write("description");
						binaryWriter.Write(this.description);
					}
					if (this.noCompile)
					{
						binaryWriter.Write("noCompile");
					}
					if (!this.hideCode)
					{
						binaryWriter.Write("!hideCode");
					}
					if (!this.hideResources)
					{
						binaryWriter.Write("!hideResources");
					}
					if (this.includeSource)
					{
						binaryWriter.Write("includeSource");
					}
					if (this.includePDB)
					{
						binaryWriter.Write("includePDB");
					}
					if (this.eacPath.Length > 0)
					{
						binaryWriter.Write("eacPath");
						binaryWriter.Write(this.eacPath);
					}
					if (this.side != ModReference.ModSide.Both)
					{
						binaryWriter.Write("side");
						binaryWriter.Write((byte)this.side);
					}
					binaryWriter.Write("buildVersion");
					binaryWriter.Write(this.buildVersion.ToString());
					binaryWriter.Write("");
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

//		// Token: 0x06001999 RID: 6553 RVA: 0x00414DDC File Offset: 0x00412FDC
//		internal static BuildProperties ReadModFile(TmodFile modFile)
//		{
//			return BuildProperties.ReadFromStream(modFile.GetStream("Info", false));
//		}

		// Token: 0x0600199A RID: 6554 RVA: 0x00414DF0 File Offset: 0x00412FF0
		internal static BuildProperties ReadFromStream(Stream stream)
		{
			BuildProperties buildProperties = new BuildProperties();
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				string text = binaryReader.ReadString();
				while (text.Length > 0)
				{
					if (text == "dllReferences")
					{
						buildProperties.dllReferences = BuildProperties.ReadList(binaryReader).ToArray<string>();
					}
					if (text == "modReferences")
					{
						buildProperties.modReferences = BuildProperties.ReadList(binaryReader).Select(new Func<string, BuildProperties.ModReference>(BuildProperties.ModReference.Parse)).ToArray<BuildProperties.ModReference>();
					}
					if (text == "weakReferences")
					{
						buildProperties.weakReferences = BuildProperties.ReadList(binaryReader).Select(new Func<string, BuildProperties.ModReference>(BuildProperties.ModReference.Parse)).ToArray<BuildProperties.ModReference>();
					}
					if (text == "sortAfter")
					{
						buildProperties.sortAfter = BuildProperties.ReadList(binaryReader).ToArray<string>();
					}
					if (text == "sortBefore")
					{
						buildProperties.sortBefore = BuildProperties.ReadList(binaryReader).ToArray<string>();
					}
					if (text == "author")
					{
						buildProperties.author = binaryReader.ReadString();
					}
					if (text == "version")
					{
						buildProperties.version = new Version(binaryReader.ReadString());
					}
					if (text == "displayName")
					{
						buildProperties.displayName = binaryReader.ReadString();
					}
					if (text == "homepage")
					{
						buildProperties.homepage = binaryReader.ReadString();
					}
					if (text == "description")
					{
						buildProperties.description = binaryReader.ReadString();
					}
					if (text == "noCompile")
					{
						buildProperties.noCompile = true;
					}
					if (text == "!hideCode")
					{
						buildProperties.hideCode = false;
					}
					if (text == "!hideResources")
					{
						buildProperties.hideResources = false;
					}
					if (text == "includeSource")
					{
						buildProperties.includeSource = true;
					}
					if (text == "includePDB")
					{
						buildProperties.includePDB = true;
					}
					if (text == "eacPath")
					{
						buildProperties.eacPath = binaryReader.ReadString();
					}
					if (text == "side")
					{
						buildProperties.side = (ModReference.ModSide)binaryReader.ReadByte();
					}
					if (text == "beta")
					{
						buildProperties.beta = true;
					}
					if (text == "buildVersion")
					{
						buildProperties.buildVersion = new Version(binaryReader.ReadString());
					}
					text = binaryReader.ReadString();
				}
			}
			return buildProperties;
		}

		// Token: 0x0600199B RID: 6555 RVA: 0x00415054 File Offset: 0x00413254
		internal static void InfoToBuildTxt(Stream src, Stream dst)
		{
			BuildProperties buildProperties = BuildProperties.ReadFromStream(src);
			StringBuilder stringBuilder = new StringBuilder();
			if (buildProperties.displayName.Length > 0)
			{
				stringBuilder.AppendLine("displayName = " + buildProperties.displayName);
			}
			if (buildProperties.author.Length > 0)
			{
				stringBuilder.AppendLine("author = " + buildProperties.author);
			}
			stringBuilder.AppendLine(string.Format("version = {0}", buildProperties.version));
			if (buildProperties.homepage.Length > 0)
			{
				stringBuilder.AppendLine("homepage = " + buildProperties.homepage);
			}
			if (buildProperties.dllReferences.Length != 0)
			{
				stringBuilder.AppendLine("dllReferences = " + string.Join(", ", buildProperties.dllReferences));
			}
			if (buildProperties.modReferences.Length != 0)
			{
				stringBuilder.AppendLine("modReferences = " + string.Join<BuildProperties.ModReference>(", ", buildProperties.modReferences));
			}
			if (buildProperties.weakReferences.Length != 0)
			{
				stringBuilder.AppendLine("weakReferences = " + string.Join<BuildProperties.ModReference>(", ", buildProperties.weakReferences));
			}
			if (buildProperties.noCompile)
			{
				stringBuilder.AppendLine("noCompile = true");
			}
			if (buildProperties.hideCode)
			{
				stringBuilder.AppendLine("hideCode = true");
			}
			if (buildProperties.hideResources)
			{
				stringBuilder.AppendLine("hideResources = true");
			}
			if (buildProperties.includeSource)
			{
				stringBuilder.AppendLine("includeSource = true");
			}
			if (buildProperties.includePDB)
			{
				stringBuilder.AppendLine("includePDB = true");
			}
			if (buildProperties.side != ModReference.ModSide.Both)
			{
				stringBuilder.AppendLine(string.Format("side = {0}", buildProperties.side));
			}
			if (buildProperties.sortAfter.Length != 0)
			{
				stringBuilder.AppendLine("sortAfter = " + string.Join(", ", buildProperties.sortAfter));
			}
			if (buildProperties.sortBefore.Length != 0)
			{
				stringBuilder.AppendLine("sortBefore = " + string.Join(", ", buildProperties.sortBefore));
			}
			byte[] bytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
			dst.Write(bytes, 0, bytes.Length);
		}

		// Token: 0x0600199C RID: 6556 RVA: 0x0041526C File Offset: 0x0041346C
		internal bool ignoreFile(string resource)
		{
			return this.buildIgnores.Any((string fileMask) => this.FitsMask(resource, fileMask));
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x004152A4 File Offset: 0x004134A4
		private bool FitsMask(string fileName, string fileMask)
		{
			return new Regex("^" + Regex.Escape(fileMask.Replace(".", "__DOT__").Replace("*", "__STAR__").Replace("?", "__QM__")).Replace("__DOT__", "[.]").Replace("__STAR__", ".*").Replace("__QM__", ".") + "$", RegexOptions.IgnoreCase).IsMatch(fileName);
		}

		// Token: 0x04001615 RID: 5653
		internal string[] dllReferences = new string[0];

		// Token: 0x04001616 RID: 5654
		internal BuildProperties.ModReference[] modReferences = new BuildProperties.ModReference[0];

		// Token: 0x04001617 RID: 5655
		internal BuildProperties.ModReference[] weakReferences = new BuildProperties.ModReference[0];

		// Token: 0x04001618 RID: 5656
		internal string[] sortAfter = new string[0];

		// Token: 0x04001619 RID: 5657
		internal string[] sortBefore = new string[0];

		// Token: 0x0400161A RID: 5658
		internal string[] buildIgnores = new string[0];

		// Token: 0x0400161B RID: 5659
		internal string author = "";

		// Token: 0x0400161C RID: 5660
		internal Version version = new Version(1, 0);

		// Token: 0x0400161D RID: 5661
		internal string displayName = "";

		// Token: 0x0400161E RID: 5662
		internal bool noCompile;

		// Token: 0x0400161F RID: 5663
		internal bool hideCode;

		// Token: 0x04001620 RID: 5664
		internal bool hideResources;

		// Token: 0x04001621 RID: 5665
		internal bool includeSource;

		// Token: 0x04001622 RID: 5666
		internal bool includePDB = true;

		// Token: 0x04001623 RID: 5667
		internal string eacPath = "";

		// Token: 0x04001624 RID: 5668
		internal bool beta;

		// Token: 0x04001625 RID: 5669
		internal Version buildVersion = DefaultConfig.ModLoaderVersion;

		// Token: 0x04001626 RID: 5670
		internal string homepage = "";

		// Token: 0x04001627 RID: 5671
		internal string description = "";

		// Token: 0x04001628 RID: 5672
		internal ModReference.ModSide side;

		// Token: 0x020004A6 RID: 1190
		internal struct ModReference
		{
			// Token: 0x06002980 RID: 10624 RVA: 0x004902EE File Offset: 0x0048E4EE
			public ModReference(string mod, Version target)
			{
				this.mod = mod;
				this.target = target;
			}

			// Token: 0x06002981 RID: 10625 RVA: 0x004902FE File Offset: 0x0048E4FE
			public override string ToString()
			{
				if (!(this.target == null))
				{
					return this.mod + "@" + this.target;
				}
				return this.mod;
			}

			// Token: 0x06002982 RID: 10626 RVA: 0x0049032C File Offset: 0x0048E52C
			public static BuildProperties.ModReference Parse(string spec)
			{
				string[] array = spec.Split(new char[]
				{
					'@'
				});
				if (array.Length == 1)
				{
					return new BuildProperties.ModReference(array[0], null);
				}
				if (array.Length > 2)
				{
					throw new Exception("Invalid mod reference: " + spec);
				}
				BuildProperties.ModReference result;
				try
				{
					result = new BuildProperties.ModReference(array[0], new Version(array[1]));
				}
				catch
				{
					throw new Exception("Invalid mod reference: " + spec);
				}
				return result;
			}

			// Token: 0x0400416F RID: 16751
			public string mod;

			// Token: 0x04004170 RID: 16752
			public Version target;

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
