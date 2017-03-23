using System;
using System.Collections.Generic;
using SQLite;
using Newtonsoft.Json;

namespace Noteworthy
{
	[JsonObject(MemberSerialization.OptIn)]
	public class Sensitivity
	{
		[PrimaryKey, AutoIncrement]
		public int RowId { get; set; }

		[JsonIgnore]
		public int SensitivityIndex { get; set; }

	}
}
