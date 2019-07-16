using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLocalizer2.Entries
{
	public interface IEntityTranslation
	{
		string TypeName { get; }

		string Namespace { get; }
	}
}
