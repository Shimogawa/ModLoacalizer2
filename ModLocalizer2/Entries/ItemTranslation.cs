using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLocalizer2.Entries
{
	public class ItemTranslation : IEntityTranslation
	{
		public string TypeName { get; set; }

		public string Namespace { get; set; }

		public string Name { get; set; }

		public string ToolTip { get; set; }

		public string SetBonus { get; set; }

		public List<string> ModifyTooltips { get; set; } = new List<string>();
	}
}
