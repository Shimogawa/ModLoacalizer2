using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using ModFileCore.ModLoader.Core;
using ModLocalizer2.Entries;
using Newtonsoft.Json;

namespace ModLocalizer2
{
	public class Dumper : TExecutor
	{
		private ModuleDef module;

		public Dumper(Program.ProgramArgs args, TmodFile modFile) : base(args, modFile) { }

		public override void Run()
		{
			try
			{
				Directory.Delete(ModFile.Name, true);
			}
			catch (DirectoryNotFoundException) { }

			CreateDirectories();
			LoadAsm();
			ExecuteDump();
		}

		private void LoadAsm()
		{
			AssemblyDef asmDef = AssemblyDef.Load(Assembly);
			module = asmDef.Modules.Single();
		}

		private void ExecuteDump()
		{
			DumpBuildProp();

		}

		private void DumpBuildProp()
		{
			BuildProperties bp;
			using (MemoryStream ms = new MemoryStream(ModFile.GetTrueBytes("Info")))
			{
				bp = BuildProperties.ReadFromStream(ms);
			}

			using (FileStream fs = new FileStream(ModFile.GetFolderPath("Info.json"), FileMode.Create))
			{
				using (StreamWriter sw = new StreamWriter(fs))
				{
					sw.Write(JsonConvert.SerializeObject(bp));
				}
			}
		}

		private void DumpItems()
		{
			List<ItemTranslation> itemTranslations = new List<ItemTranslation>();
			var itemTypes = from t in module.Types
						where t.HasBaseType("Terraria.ModLoader.ModItem")
						select t;
			foreach (var itemType in itemTypes)
			{
				ItemTranslation translation = new ItemTranslation
				{
					Namespace = itemType.Namespace,
					TypeName = itemType.Name
				};

				// Name and Description
				MethodDef methodDef = itemType.FindMethod("SetStaticDefaults",
					MethodSig.CreateInstance(module.CorLibTypes.Void));
				if (methodDef != null && methodDef.HasBody)
				{
					var instructions = methodDef.Body.Instructions;
					for (int i = 0; i < instructions.Count; i++)
					{
						Instruction cur = instructions[i];
						if (cur.OpCode == OpCodes.Ldstr)
						{
							string val = cur.Operand as string;
							i++;
							cur = instructions[i];
							IMethodDefOrRef m;
							if ((m = cur.Operand as IMethodDefOrRef) != null
							    && m.Name == "SetDefault"
							    && m.DeclaringType.Name == "ModTranslation")
							{
								cur = instructions[i - 2];
								if (!(cur?.Operand is IMethodDefOrRef mdr))
								{
									// some translation objects may get from stack;
									// In this case, we can't know their type. skip
									continue;
								}
								switch (mdr.Name)
								{
									case "get_Tooltip":
										translation.ToolTip = val;
										break;
									case "get_DisplayName":
										translation.Name = val;
										break;
								}
							}
						}
					}
				}	// End Name and Description

				// Tooltips
				methodDef = itemType.FindMethod("ModifyTooltips");
				if (methodDef != null && methodDef.HasBody)
				{
					var instructions = methodDef.Body.Instructions;
					for (int i = 0; i < instructions.Count; i++)
					{
						Instruction ins = instructions[i];
						if (ins.OpCode != OpCodes.Newobj
						    || !(ins.Operand is MemberRef memberRef)
						    || !(memberRef.DeclaringType.Name == "TooltipLine"))
							continue;

						ins = instructions[i - 1];
						if (ins.OpCode == OpCodes.Ldstr && instructions[i - 2].OpCode.Equals(OpCodes.Ldstr))
						{
							translation.ModifyTooltips.Add(instructions[i - 2].Operand as string);
							translation.ModifyTooltips.Add(instructions[i - 1].Operand as string);
						}
						else if (ins.OpCode == OpCodes.Call && ins.Operand is MemberRef m && m.Name == "Concat")
						{

						}
					}
				}
			}
		}

		private void CreateDirectories()
		{
			Directory.CreateDirectory(ModFile.Name);
			Directory.CreateDirectory(ModFile.GetFolderPath("Items"));
			Directory.CreateDirectory(ModFile.GetFolderPath("NPCs"));
			Directory.CreateDirectory(ModFile.GetFolderPath("Buffs"));
			Directory.CreateDirectory(ModFile.GetFolderPath("Miscs"));
			Directory.CreateDirectory(ModFile.GetFolderPath("Tiles"));
			Directory.CreateDirectory(ModFile.GetFolderPath("Customs"));
		}
	}
}
